namespace Viora.Application.Abstractions.Media;

public interface IStorageSettings
{
    string BasePath { get; }
    long MaxFileSizeBytes { get; }
}
