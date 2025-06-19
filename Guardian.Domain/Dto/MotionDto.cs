using System.Diagnostics;

namespace Guardian.Domain.Dto
{
    public sealed record MotionDto(
        DateTime? StartTime = null,
        DateTime? ExitTime = null,
        string? ProcessName = null,
        bool IsRunning = false,
        string? Error = null,
        bool? Success = true,
        string? NextAction = "Start Guardian"
    )
    {
        public static MotionDto BuildMotionDto(Process? process, bool isRunning)
        {
            if (process == null)
            {
                return new();
            }
            var exitTime = process.HasExited ? process?.ExitTime : null;
            return new(process?.StartTime, exitTime, process?.ProcessName, isRunning, NextAction: isRunning ? "Stop Guardian" : "Start Guardian");
        }
    }
}