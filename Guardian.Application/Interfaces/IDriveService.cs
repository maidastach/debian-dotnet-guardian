using System.Diagnostics;
using Guardian.Domain.Abstractions;

namespace Guardian.Application.Interfaces
{
    public interface IDriveService
    {
        Result<Process> Mount();
        Result<Process> Unmount();
    }
}