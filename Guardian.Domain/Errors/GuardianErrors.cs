using Guardian.Domain.Abstractions;

namespace Guardian.Domain.Errors
{
    public static class GuardianErrors
    {
        public static Error InvalidAction<T>(string message, T data) => new(message, data);
        public static Error Exception(Exception ex) => new(ex.Message, ex.Data);
        public static Error HandleException(Exception ex) => new(ex.Message, ex);
        public static readonly string MotionAlreadyRunningMsg = "Motion is already running.";
        public static readonly string MotionAlreadyStoppedMsg = "Motion is already stopped.";
    }
}