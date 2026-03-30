using Viora.Domain.Abstractions;

namespace Viora.Domain.Plans.Internal;

public record ContentForm(string Value)
{
    public static readonly ContentForm MD = new("MD");
    public static readonly ContentForm TXT = new("TXT");

    public static Result<ContentForm> CheckContentForm(string contentForm)
    {
        if (contentForm == MD.Value)
            return Result.Success(MD);
        else if (contentForm == TXT.Value)
            return Result.Success(TXT);
        else
            return Result.Failure<ContentForm>(PlanError.NotValid);
    }
}
