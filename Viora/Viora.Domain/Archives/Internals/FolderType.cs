namespace Viora.Domain.Archives.Internals;

public record FolderType(string Value)
{
    public static readonly FolderType Root = new("Root");
    public static readonly FolderType System = new("System");
    public static readonly FolderType Normal = new("Normal");

}