namespace Guardian.Domain.Configs
{
    public sealed class GuardianConfig
    {
        public required BackgroundTasks BackgroundTasks { get; init; }
        public required LocalDrivePaths LocalDrivePaths { get; init; }
        public required GoogleDrive GoogleDrive { get; init; }
        public required string TimeZoneString { get; init; }
    }

    public sealed class BackgroundTasks
    {
        public required BackgroundTasksInterval AmIOnline { get; init; }
        public required BackgroundTasksInterval CleanFiles { get; init; }
        public required BackgroundTasksInterval UploadFiles { get; init; }
    }

    public sealed class BackgroundTasksInterval
    {
        public required double DueTime_H { get; init; }
        public required double Interval_H { get; init; }
    }

    public sealed class LocalDrivePaths
    {
        public required string AmIOnline { get; init; }
        public required string AppStopped { get; init; }
        public required string Drive { get; init; }
        public required string Mount { get; init; }
        public required string Monitor { get; init; }
        public required string Uploaded { get; init; }
    }

    public sealed partial class GoogleDrive
    {
        // public required ushort Interval { get; init; }
        public required string Secret { get; init; }
        public required string ApplicationName { get; init; }
        // public required string TokenPath { get; init; }
        public required string FolderNameOnDrive { get; init; }
        public required string FolderMimeType { get; init; }
    }
}