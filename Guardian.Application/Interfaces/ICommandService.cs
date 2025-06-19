using System.Diagnostics;
using Guardian.Domain.Abstractions;

namespace Guardian.Application.Interfaces
{
    public interface ICommandService
    {
        Result<Process> Execute(string command, bool keepAlive = false, bool isSudo = false, bool dispose = true);
        Result<Process> ExecuteMany(IEnumerable<string> command, bool keepAlive = false, bool isSudo = false, bool dispose = true);
        Result<Process> RevokeSudo();
    }
}