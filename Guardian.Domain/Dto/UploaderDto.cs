namespace Guardian.Domain.Dto
{
    public record CleanFilesDto(IEnumerable<string> Files)
    {
        public IEnumerable<string> Deleted { get; set; } = [];
        public bool Trashed { get; set; }
    }

    public record UploadFilesDto(string FileName, string Message, bool Deleted = false);
}