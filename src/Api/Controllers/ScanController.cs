using Api.Contracts.Scan;
using Application.Scan.Commands.AssignAgent;
using Application.Scan.Commands.RevokeAgent;
using Application.Scan.Commands.ScanTicket;
using Application.Scan.Commands.SyncScans;
using Application.Scan.Queries.GetAttendance;
using Application.Scan.Queries.GetOfflineBundle;
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
        [HttpPost("tickets")]
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
        /// Assigne un agent de scan à un événement.
        /// </summary>
        [Authorize(Policy = "OrganisateurOnly")]
        [HttpPost("events/{eventId:guid}/agents")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignAgent(
            Guid eventId,
            [FromBody] AssignAgentRequest request)
        {
            var cmd = new AssignAgentCommand(eventId, request.AgentPhone);
            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => Created($"api/scan/events/{eventId}/agents/{dto.AgentId}", dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }

        /// <summary>
        /// Révoque un agent de scan d'un événement.
        /// </summary>
        [Authorize(Policy = "OrganisateurOnly")]
        [HttpDelete("events/{eventId:guid}/agents/{agentId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevokeAgent(
            Guid eventId,
            Guid agentId)
        {
            var cmd = new RevokeAgentCommand(eventId, agentId);
            var result = await _mediator.Send(cmd, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: _ => NoContent(),
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

        /// <summary>
        /// Récupère le bundle de tickets pour le scan hors-ligne.
        /// </summary>
        [Authorize(Policy = "AgentOnly")]
        [HttpGet("events/{eventId:guid}/offline-bundle")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOfflineBundle(Guid eventId)
        {
            var query = new GetOfflineBundleQuery(eventId);
            var result = await _mediator.Send(query, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => Ok(dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }

        /// <summary>
        /// Récupère les statistiques de présence d'un événement.
        /// </summary>
        [Authorize(Policy = "OrganisateurOnly")]
        [HttpGet("events/{eventId:guid}/attendance")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAttendance(Guid eventId)
        {
            var query = new GetAttendanceQuery(eventId);
            var result = await _mediator.Send(query, CancellationToken.None);

            return result.Match<IActionResult>(
                onSuccess: dto => Ok(dto),
                onFailure: error => error.Code switch
                {
                    var c when c.Contains("NotFound") => NotFound(error),
                    _ => BadRequest(new { error.Code, error.Message })
                });
        }
    }
}
