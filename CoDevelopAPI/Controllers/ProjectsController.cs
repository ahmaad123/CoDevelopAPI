using Microsoft.AspNetCore.Mvc;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ProjectResponseDto>.ErrorResponse("Validation failed", 400));

            var result = await _projectService.CreateProjectAsync(dto);
            return result.Success
                ? CreatedAtAction(nameof(GetProjectById), new { projectId = result.Data.ProjectId }, result)
                : StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects(
            [FromQuery] string? status = null,
            [FromQuery] int? clientId = null,
            [FromQuery] int? assigneeId = null)
        {
            if (!string.IsNullOrWhiteSpace(status))
                return Ok(await _projectService.GetProjectsByStatusAsync(status));
            if (clientId.HasValue)
                return Ok(await _projectService.GetProjectsByClientAsync(clientId.Value));
            if (assigneeId.HasValue)
                return Ok(await _projectService.GetProjectsByAssigneeAsync(assigneeId.Value));

            return Ok(await _projectService.GetAllProjectsAsync());
        }

        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetProjectById(int projectId)
        {
            var result = await _projectService.GetProjectByIdAsync(projectId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPut("{projectId}")]
        public async Task<IActionResult> UpdateProject(int projectId, [FromBody] UpdateProjectDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<ProjectResponseDto>.ErrorResponse("Validation failed", 400));

            var result = await _projectService.UpdateProjectAsync(projectId, dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{projectId}")]
        public async Task<IActionResult> DeleteProject(int projectId)
        {
            var result = await _projectService.DeleteProjectAsync(projectId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{projectId}/status")]
        public async Task<IActionResult> UpdateStatus(int projectId, [FromBody] UpdateProjectStatusDto dto)
        {
            var result = await _projectService.UpdateProjectStatusAsync(projectId, dto.Status);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{projectId}/progress")]
        public async Task<IActionResult> UpdateProgress(int projectId, [FromBody] UpdateProjectProgressDto dto)
        {
            var result = await _projectService.UpdateProjectProgressAsync(projectId, dto.Progress);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        [HttpPost("{projectId}/assignees")]
        public async Task<IActionResult> AddAssignees(int projectId, [FromBody] AssignProjectMembersDto dto)
        {
            var result = await _projectService.AddProjectAssigneesAsync(projectId, dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{projectId}/assignees/{userId}")]
        public async Task<IActionResult> RemoveAssignee(int projectId, int userId)
        {
            var result = await _projectService.RemoveProjectAssigneeAsync(projectId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
