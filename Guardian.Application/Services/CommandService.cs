using System.Diagnostics;
using System.Text;
using Guardian.Application.Interfaces;
using Guardian.Domain.Abstractions;
using Guardian.Domain.DependencyInjection;
using Guardian.Domain.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Guardian.Application.Services
{
    [ServiceLifetime(ServiceLifetime.Scoped)]
    public sealed class CommandService(ILogger<CommandService> logger) : ICommandService
    {
        private readonly ILogger<CommandService> _logger = logger;
        private const string _SUDO = "sudo";

        public Result<Process> Execute(string command, bool keepAlive = false, bool isSudo = false, bool dispose = true)
        {
            if (isSudo)
            {
                command = $"{_SUDO} {command}";
            }

            Process process = CreateProcess(command, keepAlive);

            process.Start();
            if (keepAlive)
            {
                return Result<Process>.Success(process);
            }

            process.WaitForExit();
            int exitCode = process.ExitCode;
            process.Kill();

            if (dispose)
            {
                process.Dispose();
            }

            if (exitCode != 0)
            {
                var errorMessage = $"Command {command} failed to execute with exit code {exitCode}";
                _logger.LogError("{CommandRunError}", errorMessage);
                return Result<Process>.Failure(GuardianErrors.InvalidAction(errorMessage, process));
            }

            _logger.LogInformation("Command {Command} executed succesfully", command);
            return Result<Process>.Success(process);
        }

        public Result<Process> ExecuteMany(IEnumerable<string> commands, bool keepAlive = false, bool isSudo = false, bool dispose = true)
        {
            if (!commands.Any())
            {
                return Result<Process>.Success(new());
            }

            var command = JoinCommands(commands.ToArray(), isSudo);

            Process process = CreateProcess(command, keepAlive);

            process.Start();

            if (keepAlive)
            {
                return Result<Process>.Success(process);
            }

            process.WaitForExit();
            int exitCode = process.ExitCode;
            process.Kill();

            if (dispose)
            {
                process.Dispose();
            }

            if (exitCode != 0)
            {
                var errorMessage = $"Command {command} failed to execute with exit code {exitCode}";
                _logger.LogError("{CommandRunError}", errorMessage);
                return Result<Process>.Failure(GuardianErrors.InvalidAction(errorMessage, process));
            }

            _logger.LogInformation("Command {Command} executed succesfully", command);
            return Result<Process>.Success(process);
        }

        public Result<Process> RevokeSudo()
        {
            return Execute("-k", isSudo: true);
        }

        private static Process CreateProcess(string command, bool keepAlive)
        {
            var (fileName, arguments) = SplitCommand(command);
            ProcessStartInfo processStartInfo = new(fileName, arguments);
            // process.EnableRaisingEvents = true;

            if (!keepAlive)
            {
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
            }
            else
            {
                processStartInfo.CreateNoWindow = true;
            }

            return new()
            {
                StartInfo = processStartInfo
            };
        }

        private static (string, string) SplitCommand(string command)
        {
            int firstSpaceIndex = command.IndexOf(' ');
            if (firstSpaceIndex == -1)
            {
                return (command, string.Empty);
            }

            return (command[..firstSpaceIndex], command[(firstSpaceIndex + 1)..]);
        }

        private static string JoinCommands(string[] commands, bool isSudo)
        {
            StringBuilder sb = new();
            if (isSudo)
            {
                sb.Append($"{_SUDO} ");
            }
            for (int i = 0; i < commands.Length; i++)
            {
                if (i == 0)
                {
                    sb.Append(commands[i]);
                }
                else
                {
                    sb.Append($" && {(isSudo ? (_SUDO + ' ') : "")}{commands[1]}");
                }
            }
            if (isSudo)
            {
                sb.Append($" && {_SUDO} -k");
            }
            return sb.ToString();
        }
    }
}