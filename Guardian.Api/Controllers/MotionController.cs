using Guardian.Application.Interfaces;
using Guardian.Domain.Dto;
using Guardian.Domain.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Guardian.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MotionController(IMotionService motionService) : ControllerBase
    {
        private readonly IMotionService _motionService = motionService;

        [HttpGet]
        [Route("status")]
        public IActionResult GetMotionStatus()
        {
            var result = _motionService.GetStatus();
            return result.Match<IActionResult, MotionDto>(Ok, BadRequest);
        }

        [HttpPost]
        [Route("start")]
        public IActionResult StartMotion()
        {
            var result = _motionService.Start();
            return result.Match<IActionResult, MotionDto>(Ok, BadRequest);
        }

        [HttpPost]
        [Route("stop")]
        public IActionResult StopMotion()
        {
            var result = _motionService.Stop();
            return result.Match<IActionResult, MotionDto>(Ok, BadRequest);
        }
    }
}