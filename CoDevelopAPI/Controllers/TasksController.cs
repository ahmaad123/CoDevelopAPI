using Microsoft.AspNetCore.Mvc;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>
        /// Creates a new task
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<TaskResponseDto>.ErrorResponse("Validation failed", 400));

            var result = await _taskService.CreateTaskAsync(dto);
            return result.Success
                ? CreatedAtAction(nameof(GetTaskById), new { taskId = result.Data.TaskId }, result)
                : StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Gets a specific task by ID
        /// </summary>
        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTaskById(int taskId)
        {
            var result = await _taskService.GetTaskByIdAsync(taskId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Gets all tasks for a project (optionally filtered by status)
        /// </summary>
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTasksByProject(int projectId, [FromQuery] string? status = null)
        {
            if (!string.IsNullOrWhiteSpace(status))
                return Ok(await _taskService.GetTasksByStatusAsync(projectId, status));

            return Ok(await _taskService.GetTasksByProjectAsync(projectId));
        }

        /// <summary>
        /// Gets tasks assigned to a specific user
        /// </summary>
        [HttpGet("assignee/{userId}")]
        public async Task<IActionResult> GetTasksByAssignee(int userId)
        {
            return Ok(await _taskService.GetTasksByAssigneeAsync(userId));
        }

        /// <summary>
        /// Updates a task
        /// </summary>
        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateTask(int taskId, [FromBody] UpdateTaskDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<TaskResponseDto>.ErrorResponse("Validation failed", 400));

            var result = await _taskService.UpdateTaskAsync(taskId, dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Deletes a task
        /// </summary>
        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var result = await _taskService.DeleteTaskAsync(taskId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Updates task status
        /// </summary>
        [HttpPatch("{taskId}/status")]
        public async Task<IActionResult> UpdateStatus(int taskId, [FromBody] UpdateTaskStatusDto dto)
        {
            var result = await _taskService.UpdateTaskStatusAsync(taskId, dto.Status);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
