using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Viora.Application.Abstractions.Messaging;

namespace Viora.Api.OpenApi;

/// <summary>
/// Discovers the MediatR response type for a controller action by scanning its IL for the
/// command/query it constructs, then reading the response <c>T</c> from the message's
/// <see cref="ICommand{T}"/> / <see cref="IQuery{T}"/> interface.
/// </summary>
internal static class CommandResponseTypeResolver
{
    internal readonly record struct ResolvedResponse(Type? ResponseType, bool IsNoBody);

    private const int MaxDepth = 3;

    // op.Value -> OpCode, used to know each opcode's operand size while walking IL.
    private static readonly IReadOnlyDictionary<short, OpCode> OpCodes = BuildOpCodeTable();

    /// <summary>
    /// Returns the resolved response for the action, or <c>null</c> when it cannot be determined
    /// (no command found, ambiguous, or anything went wrong — callers should leave the action untouched).
    /// </summary>
    public static ResolvedResponse? Resolve(MethodInfo action)
    {
        try
        {
            return ScanBody(action, action.DeclaringType, new HashSet<MethodBase>(), 0);
        }
        catch
        {
            return null;
        }
    }

    private static ResolvedResponse? ScanBody(MethodBase method, Type? controllerType, HashSet<MethodBase> visited, int depth)
    {
        if (depth > MaxDepth || !visited.Add(method))
            return null;

        // Async methods compile their body into a state machine; the `new XxxCommand(...)` lives in MoveNext.
        var stateMachine = method.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType;
        var body = stateMachine?.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? method;

        var il = body.GetMethodBody()?.GetILAsByteArray();
        if (il is null)
            return null;

        var module = body.Module;
        var typeGenArgs = body.DeclaringType?.IsGenericType == true ? body.DeclaringType.GetGenericArguments() : null;
        var methodGenArgs = body.IsGenericMethod ? body.GetGenericArguments() : null;

        var calledHelpers = new List<MethodBase>();
        Type? foundMessage = null;
        var ambiguous = false;

        var pos = 0;
        while (pos < il.Length)
        {
            short code = il[pos++];
            if (code == 0xFE && pos < il.Length)
                code = (short)(0xFE00 | il[pos++]);

            if (!OpCodes.TryGetValue(code, out var op))
                return null; // unknown opcode -> walk would desync; bail safely

            if (op.OperandType == OperandType.InlineSwitch)
            {
                if (pos + 4 > il.Length) return null;
                var count = BitConverter.ToInt32(il, pos);
                pos += 4 + (4 * count);
                continue;
            }

            if (op == System.Reflection.Emit.OpCodes.Newobj ||
                op == System.Reflection.Emit.OpCodes.Call ||
                op == System.Reflection.Emit.OpCodes.Callvirt)
            {
                if (pos + 4 > il.Length) return null;
                var token = BitConverter.ToInt32(il, pos);
                var resolved = SafeResolveMethod(module, token, typeGenArgs, methodGenArgs);
                if (resolved is not null)
                {
                    var declaring = resolved.DeclaringType;
                    if (declaring is not null && MatchMessageType(declaring) is { } match)
                    {
                        if (foundMessage is null)
                            foundMessage = declaring;
                        else if (foundMessage != declaring)
                            ambiguous = true; // two distinct messages -> don't guess
                    }
                    else if ((op == System.Reflection.Emit.OpCodes.Call || op == System.Reflection.Emit.OpCodes.Callvirt) &&
                             IsPrivateSameControllerHelper(resolved, controllerType))
                    {
                        calledHelpers.Add(resolved);
                    }
                }
            }

            pos += OperandSize(op.OperandType);
        }

        if (ambiguous)
            return null;

        if (foundMessage is not null)
            return MatchMessageType(foundMessage);

        // No command in this body: follow private same-controller helpers (e.g. AddImageToGallery -> AddToGallery).
        foreach (var helper in calledHelpers)
        {
            var result = ScanBody(helper, controllerType, visited, depth + 1);
            if (result is not null)
                return result;
        }

        return null;
    }

    /// <summary>
    /// Returns the resolved response for a message type, or <c>null</c> if it is not a command/query.
    /// </summary>
    private static ResolvedResponse? MatchMessageType(Type type)
    {
        foreach (var iface in type.GetInterfaces())
        {
            if (!iface.IsGenericType)
                continue;

            var def = iface.GetGenericTypeDefinition();
            if (def == typeof(ICommand<>) || def == typeof(IQuery<>))
                return new ResolvedResponse(iface.GetGenericArguments()[0], false);
        }

        // Non-generic command -> 204 No Content.
        if (typeof(IBaseCommand).IsAssignableFrom(type))
            return new ResolvedResponse(null, true);

        return null;
    }

    private static bool IsPrivateSameControllerHelper(MethodBase method, Type? controllerType)
        => controllerType is not null
           && method.DeclaringType == controllerType
           && method is MethodInfo { IsPublic: false };

    private static MethodBase? SafeResolveMethod(Module module, int token, Type[]? typeGenArgs, Type[]? methodGenArgs)
    {
        try
        {
            return module.ResolveMethod(token, typeGenArgs, methodGenArgs);
        }
        catch
        {
            return null;
        }
    }

    private static int OperandSize(OperandType operandType) => operandType switch
    {
        OperandType.InlineNone => 0,
        OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
        OperandType.InlineVar => 2,
        OperandType.InlineI8 or OperandType.InlineR => 8,
        // InlineBrTarget, InlineField, InlineI, InlineMethod, InlineSig,
        // InlineString, InlineTok, InlineType, ShortInlineR -> 4
        _ => 4,
    };

    private static Dictionary<short, OpCode> BuildOpCodeTable()
    {
        var table = new Dictionary<short, OpCode>();
        foreach (var field in typeof(System.Reflection.Emit.OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetValue(null) is OpCode op)
                table[op.Value] = op;
        }
        return table;
    }
}
