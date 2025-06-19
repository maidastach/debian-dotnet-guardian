namespace Guardian.Application.Interfaces
{
    public interface IFileStabilityCheckerService
    {
        Task CheckFileSizeStabilityAsync(string filePath);
    }
}