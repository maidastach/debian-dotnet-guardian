using Guardian.Domain.Abstractions;
using Guardian.Domain.Dto;

namespace Guardian.Application.Interfaces
{
    public interface IMotionService
    {
        Result<MotionDto> GetStatus();
        Result<MotionDto> Start(bool shouldMountDrive = false);
        Result<MotionDto> Stop();
    }
}