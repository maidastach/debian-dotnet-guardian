using static Guardian.Domain.Helpers.DateHelper;

namespace Guardian.Domain.Entities.Records
{
    public class Record(string fileName, string fullPath)
    {
        private List<RecordLog> _RecordLogs = [];
        public virtual IEnumerable<RecordLog> RecordLogs => _RecordLogs.AsReadOnly();

        public Guid Id { get; private set; }
        public string FileName { get; private set; } = fileName;
        public string FullPath { get; private set; } = fullPath;
        public DateTimeOffset CreatedAt { get; private set; } = GetTimeZoneDate();
        public DateTimeOffset ModifiedAt { get; private set; } = GetTimeZoneDate();
        public bool IsUploaded { get; private set; }
        public string? GoogleDriveId { get; private set; }
        public DateTimeOffset? UploadedAt { get; private set; }
        public bool IsDeleted { get; private set; }

        public void AddLog(RecordLog recordLog)
        {
            _RecordLogs.Add(recordLog);
        }

        public void UploadCompleted(string? googleDriveId = "")
        {
            AddLog(new RecordLog($"Successfully uploaded {FileName}"));
            GoogleDriveId = googleDriveId;
            UploadedAt = GetTimeZoneDate();
            IsUploaded = true;
        }

        public void Update(
            string? fileName = null,
            string? fullPath = null,
            bool? isUploaded = null,
            string? googleDriveId = null,
            bool? isDeleted = null)
        {
            FileName = fileName ?? FileName;
            FullPath = fullPath ?? FullPath;
            GoogleDriveId = googleDriveId ?? GoogleDriveId;

            if (isUploaded.HasValue)
            {
                IsUploaded = isUploaded.Value;
            }
            if (isDeleted.HasValue)
            {
                IsDeleted = isDeleted.Value;
            }

            ModifiedAt = GetTimeZoneDate();
        }

        public void Delete()
        {
            IsDeleted = true;
            ModifiedAt = GetTimeZoneDate();
        }
    }
}