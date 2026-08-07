using Microsoft.AspNetCore.Mvc;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(ITicketService ticketService, ILogger<TicketsController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new ticket
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), 400)]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketDto createTicketDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<TicketResponseDto>.ErrorResponse(
                    "Validation failed", 400, errors));
            }

            var result = await _ticketService.CreateTicketAsync(createTicketDto);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return CreatedAtAction(
                nameof(GetTicketById),
                new { ticketId = result.Data.TicketId },
                result);
        }

        /// <summary>
        /// Retrieves all tickets with optional filtering
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<TicketResponseDto>>), 200)]
        public async Task<IActionResult> GetAllTickets(
            [FromQuery] string? status = null,
            [FromQuery] string? priority = null,
            [FromQuery] int? clientId = null)
        {
            if (!string.IsNullOrWhiteSpace(status))
            {
                var result = await _ticketService.GetTicketsByStatusAsync(status);
                return Ok(result);
            }

            if (!string.IsNullOrWhiteSpace(priority))
            {
                var result = await _ticketService.GetTicketsByPriorityAsync(priority);
                return Ok(result);
            }

            if (clientId.HasValue)
            {
                var result = await _ticketService.GetTicketsByClientAsync(clientId.Value);
                return Ok(result);
            }

            var allTickets = await _ticketService.GetAllTicketsAsync();
            return Ok(allTickets);
        }

        /// <summary>
        /// Retrieves a specific ticket by ID
        /// </summary>
        [HttpGet("{ticketId}")]
        [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), 404)]
        public async Task<IActionResult> GetTicketById(int ticketId)
        {
            var result = await _ticketService.GetTicketByIdAsync(ticketId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Gets tickets assigned to a specific user
        /// </summary>
        [HttpGet("assigned/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<List<TicketResponseDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<List<TicketResponseDto>>), 404)]
        public async Task<IActionResult> GetMyAssignedTickets(int userId)
        {
            var result = await _ticketService.GetMyAssignedTicketsAsync(userId);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        /// <summary>
        /// Gets tickets reported by a specific user
        /// </summary>
        [HttpGet("reported/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<List<TicketResponseDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<List<TicketResponseDto>>), 404)]
        public async Task<IActionResult> GetMyReportedTickets(int userId)
        {
            var result = await _ticketService.GetMyReportedTicketsAsync(userId);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        /// <summary>
        /// Updates an existing ticket
        /// </summary>
        [HttpPut("{ticketId}")]
        [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), 404)]
        public async Task<IActionResult> UpdateTicket(int ticketId, [FromBody] UpdateTicketDto updateTicketDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<TicketResponseDto>.ErrorResponse(
                    "Validation failed", 400, errors));
            }

            var result = await _ticketService.UpdateTicketAsync(ticketId, updateTicketDto);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        /// <summary>
        /// Deletes a ticket
        /// </summary>
        [HttpDelete("{ticketId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
        public async Task<IActionResult> DeleteTicket(int ticketId)
        {
            var result = await _ticketService.DeleteTicketAsync(ticketId);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        /// <summary>
        /// Assigns a ticket to a user
        /// </summary>
        [HttpPatch("{ticketId}/assign")]
        [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), 404)]
        public async Task<IActionResult> AssignTicket(int ticketId, [FromBody] AssignTicketDto assignTicketDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<TicketResponseDto>.ErrorResponse(
                    "Validation failed", 400));
            }

            var result = await _ticketService.AssignTicketAsync(ticketId, assignTicketDto);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        /// <summary>
        /// Updates ticket status
        /// </summary>
        [HttpPatch("{ticketId}/status")]
        [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), 404)]
        public async Task<IActionResult> UpdateTicketStatus(int ticketId, [FromBody] UpdateTicketStatusDto updateStatusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<TicketResponseDto>.ErrorResponse(
                    "Validation failed", 400));
            }

            var result = await _ticketService.UpdateTicketStatusAsync(ticketId, updateStatusDto);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
    }
}