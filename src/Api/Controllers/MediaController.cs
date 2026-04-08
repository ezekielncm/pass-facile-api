using Application.Media.Commands.UploadMedia;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MediaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MediaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Upload un fichier média (image de couverture, logo, bannière, etc.).
        /// </summary>
        [Authorize(Policy = "OrganisateurOnly")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadMedia(
            IFormFile file,
            [FromForm] string context)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new { Code = "Media.InvalidFile", Message = "Le fichier est requis." });

            await using var stream = file.OpenReadStream();

            var cmd = new UploadMediaCommand(
                stream,
                file.FileName,
                file.ContentType,
                context);

            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => Created(dto.Url, dto),
                onFailure: error => BadRequest(new { error.Code, error.Message }));
        }
    }
}
