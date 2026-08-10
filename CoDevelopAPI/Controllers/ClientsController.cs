using Microsoft.AspNetCore.Mvc;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(IClientService clientService, ILogger<ClientsController> logger)
        {
            _clientService = clientService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new client
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ClientResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<ClientResponseDto>), 400)]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientDto createClientDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<ClientResponseDto>.ErrorResponse(
                    "Validation failed", 400, errors));
            }

            var result = await _clientService.CreateClientAsync(createClientDto);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return CreatedAtAction(
                nameof(GetClientById),
                new { clientId = result.Data.ClientId },
                result);
        }

        /// <summary>
        /// Retrieves all clients with optional filtering
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<ClientResponseDto>>), 200)]
        public async Task<IActionResult> GetAllClients([FromQuery] string? status = null)
        {
            if (!string.IsNullOrWhiteSpace(status))
            {
                var result = await _clientService.GetClientsByStatusAsync(status);
                return Ok(result);
            }

            var allClients = await _clientService.GetAllClientsAsync();
            return Ok(allClients);
        }

        /// <summary>
        /// Retrieves a specific client by ID
        /// </summary>
        [HttpGet("{clientId}")]
        [ProducesResponseType(typeof(ApiResponse<ClientResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ClientResponseDto>), 404)]
        public async Task<IActionResult> GetClientById(int clientId)
        {
            var result = await _clientService.GetClientByIdAsync(clientId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Gets all active clients
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponse<List<ClientResponseDto>>), 200)]
        public async Task<IActionResult> GetActiveClients()
        {
            var result = await _clientService.GetActiveClientsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing client
        /// </summary>
        [HttpPut("{clientId}")]
        [ProducesResponseType(typeof(ApiResponse<ClientResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<ClientResponseDto>), 400)]
        [ProducesResponseType(typeof(ApiResponse<ClientResponseDto>), 404)]
        public async Task<IActionResult> UpdateClient(int clientId, [FromBody] UpdateClientDto updateClientDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ApiResponse<ClientResponseDto>.ErrorResponse(
                    "Validation failed", 400, errors));
            }

            var result = await _clientService.UpdateClientAsync(clientId, updateClientDto);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        /// <summary>
        /// Deletes a client
        /// </summary>
        [HttpDelete("{clientId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
        public async Task<IActionResult> DeleteClient(int clientId)
        {
            var result = await _clientService.DeleteClientAsync(clientId);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        [HttpGet("check-email")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> CheckEmailExists(
            [FromQuery] string email,
            [FromQuery] int? excludeClientId = null)
        {
            var result = await _clientService.CheckEmailExistsAsync(email, excludeClientId);
            return Ok(result);
        }

        /// <summary>
        /// Check if mobile exists
        /// </summary>
        [HttpGet("check-mobile")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> CheckMobileExists(
            [FromQuery] string mobile,
            [FromQuery] int? excludeClientId = null)
        {
            var result = await _clientService.CheckMobileExistsAsync(mobile, excludeClientId);
            return Ok(result);
        }
    }
}
