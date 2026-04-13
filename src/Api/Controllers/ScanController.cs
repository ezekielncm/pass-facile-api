using Api.Contracts.Scan;
using Application.Scan.Commands.ScanTicket;
using Application.Scan.Commands.SyncScans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ScanController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ScanController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Scanne un ticket via son QR code.
        /// </summary>
        [Authorize(Policy = "AgentOnly")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ScanTicket(
            [FromBody] ScanTicketRequest request)
        {
            var cmd = new ScanTicketCommand(
                request.QrPayload,
                request.DeviceId,
                request.ScannedAt);

            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => Ok(dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }

        /// <summary>
        /// Synchronise des scans effectués en mode hors-ligne.
        /// </summary>
        [Authorize(Policy = "AgentOnly")]
        [HttpPost("sync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SyncScans(
            [FromBody] SyncScansRequest request)
        {
            var scans = request.Scans
                .Select(s => new OfflineScanDto(s.QrPayload, s.ScannedAt, s.DeviceId))
                .ToList()
                .AsReadOnly();

            var cmd = new SyncScansCommand(scans);
            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => Ok(dto),
                onFailure: error => BadRequest(new { error.Code, error.Message }));
        }
    }
}
