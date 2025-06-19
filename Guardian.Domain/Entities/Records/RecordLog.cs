using static Guardian.Domain.Helpers.DateHelper;

namespace Guardian.Domain.Entities.Records
{
    public class RecordLog(string message, bool isError = false)
    {
        public Guid Id { get; private set; }
        public DateTimeOffset Date { get; private set; } = GetTimeZoneDate();
        public bool IsError { get; private set; } = isError;
        public string Message { get; private set; } = message;
        public Guid? RecordId { get; private set; }
        public Record? Record { get; private set; }
    }
}