using Guardian.Application.Interfaces;
using Guardian.Domain.Dto;
using Guardian.Domain.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Guardian.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriveProviderController(IUploaderService uploaderService) : ControllerBase
    {
        private readonly IUploaderService _uploaderService = uploaderService;

        [HttpGet]
        [Route("upload")]
        public async Task<IActionResult> UploadMissingFiles(CancellationToken cancellationToken)
        {
            return (await _uploaderService.UploadMissingFilesAsync(cancellationToken))
                .Match<IActionResult, IEnumerable<UploadFilesDto>>(Ok, BadRequest);
        }

        [HttpGet]
        [Route("cleanoriginals")]
        public async Task<IActionResult> CleanDrive(CancellationToken cancellationToken)
        {
            return (await _uploaderService.CleanFilesAsync(cancellationToken))
                .Match<IActionResult, CleanFilesDto>(Ok, BadRequest);
        }
    }
}