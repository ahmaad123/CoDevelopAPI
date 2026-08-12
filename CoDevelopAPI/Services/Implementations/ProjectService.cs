using Microsoft.EntityFrameworkCore;
using CoDevelopAPI.Data;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Models.Entities;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Services.Implementations
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(ApplicationDbContext context, ILogger<ProjectService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<ProjectResponseDto>> CreateProjectAsync(CreateProjectDto dto)
        {
            try
            {
                var client = await _context.Clients.FindAsync(dto.ClientId);
                if (client == null)
                    return ApiResponse<ProjectResponseDto>.ErrorResponse($"Client with ID {dto.ClientId} not found", 400);

                var createdByUser = await _context.Users.FindAsync(dto.CreatedBy);
                if (createdByUser == null)
                    return ApiResponse<ProjectResponseDto>.ErrorResponse($"User with ID {dto.CreatedBy} not found", 400);

                if (dto.AssigneeIds.Any())
                {
                    var validUserIds = await _context.Users
                        .Where(u => dto.AssigneeIds.Contains(u.Userid))
                        .Select(u => u.Userid)
                        .ToListAsync();
                    var invalidIds = dto.AssigneeIds.Except(validUserIds).ToList();
                    if (invalidIds.Any())
                        return ApiResponse<ProjectResponseDto>.ErrorResponse($"Invalid user IDs: {string.Join(", ", invalidIds)}", 400);
                }

                var project = new Project
                {
                    Projectname = dto.ProjectName,
                    Clientid = dto.ClientId,
                    Developer = dto.Developer,
                    Manager = dto.Manager,
                    Progress = dto.Progress ?? 0,
                    Budget = dto.Budget,
                    Deadline = dto.Deadline,
                    Status = dto.Status ?? "Active",
                    Description = dto.Description,
                    Createdby = dto.CreatedBy
                };

                await _context.Projects.AddAsync(project);
                await _context.SaveChangesAsync();

                if (dto.AssigneeIds.Any())
                {
                    foreach (var userId in dto.AssigneeIds)
                    {
                        await _context.Projectassignees.AddAsync(new Projectassignee
                        {
                            Projectid = project.Projectid,
                            Userid = userId,
                            Assignedby = dto.CreatedBy,
                            Assigneddate = DateTime.UtcNow
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                var result = await GetProjectWithDetailsAsync(project.Projectid);
                return ApiResponse<ProjectResponseDto>.SuccessResponse(MapToResponseDto(result), "Project created successfully", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return ApiResponse<ProjectResponseDto>.ErrorResponse("An error occurred while creating the project", 500);
            }
        }

        public async Task<ApiResponse<ProjectResponseDto>> GetProjectByIdAsync(int projectId)
        {
            try
            {
                var project = await GetProjectWithDetailsAsync(projectId);
                if (project == null)
                    return ApiResponse<ProjectResponseDto>.ErrorResponse($"Project with ID {projectId} not found", 404);

                return ApiResponse<ProjectResponseDto>.SuccessResponse(MapToResponseDto(project));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving project ID: {projectId}");
                return ApiResponse<ProjectResponseDto>.ErrorResponse("An error occurred while retrieving the project", 500);
            }
        }

        public async Task<ApiResponse<List<ProjectCardDto>>> GetAllProjectsAsync()
        {
            try
            {
                var projects = await _context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Projectassignees)
                    .Include(p => p.Tasks)
                    .OrderByDescending(p => p.Projectid)
                    .ToListAsync();

                return ApiResponse<List<ProjectCardDto>>.SuccessResponse(projects.Select(MapToCardDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all projects");
                return ApiResponse<List<ProjectCardDto>>.ErrorResponse("An error occurred while retrieving projects", 500);
            }
        }

        public async Task<ApiResponse<List<ProjectCardDto>>> GetProjectsByStatusAsync(string status)
        {
            try
            {
                var projects = await _context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Projectassignees)
                    .Include(p => p.Tasks)
                    .Where(p => p.Status.ToLower() == status.ToLower())
                    .OrderByDescending(p => p.Projectid)
                    .ToListAsync();

                return ApiResponse<List<ProjectCardDto>>.SuccessResponse(projects.Select(MapToCardDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving projects by status: {status}");
                return ApiResponse<List<ProjectCardDto>>.ErrorResponse("An error occurred while retrieving projects", 500);
            }
        }

        public async Task<ApiResponse<List<ProjectCardDto>>> GetProjectsByClientAsync(int clientId)
        {
            try
            {
                var projects = await _context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Projectassignees)
                    .Include(p => p.Tasks)
                    .Where(p => p.Clientid == clientId)
                    .OrderByDescending(p => p.Projectid)
                    .ToListAsync();

                return ApiResponse<List<ProjectCardDto>>.SuccessResponse(projects.Select(MapToCardDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving projects for client ID: {clientId}");
                return ApiResponse<List<ProjectCardDto>>.ErrorResponse("An error occurred while retrieving projects", 500);
            }
        }

        public async Task<ApiResponse<List<ProjectCardDto>>> GetProjectsByAssigneeAsync(int userId)
        {
            try
            {
                var projects = await _context.Projects
                    .Include(p => p.Client)
                    .Include(p => p.Projectassignees)
                    .Include(p => p.Tasks)
                    .Where(p => p.Projectassignees.Any(pa => pa.Userid == userId))
                    .OrderByDescending(p => p.Projectid)
                    .ToListAsync();

                return ApiResponse<List<ProjectCardDto>>.SuccessResponse(projects.Select(MapToCardDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving projects for assignee ID: {userId}");
                return ApiResponse<List<ProjectCardDto>>.ErrorResponse("An error occurred while retrieving projects", 500);
            }
        }

        public async Task<ApiResponse<ProjectResponseDto>> UpdateProjectAsync(int projectId, UpdateProjectDto dto)
        {
            try
            {
                var project = await _context.Projects.FindAsync(projectId);
                if (project == null)
                    return ApiResponse<ProjectResponseDto>.ErrorResponse($"Project with ID {projectId} not found", 404);

                var client = await _context.Clients.FindAsync(dto.ClientId);
                if (client == null)
                    return ApiResponse<ProjectResponseDto>.ErrorResponse($"Client with ID {dto.ClientId} not found", 400);

                project.Projectname = dto.ProjectName;
                project.Clientid = dto.ClientId;
                project.Developer = dto.Developer;
                project.Manager = dto.Manager;
                project.Progress = dto.Progress;
                project.Budget = dto.Budget;
                project.Deadline = dto.Deadline;
                project.Status = dto.Status ?? project.Status;
                project.Description = dto.Description;

                if (dto.AssigneeIds != null)
                {
                    if (dto.AssigneeIds.Any())
                    {
                        var validUserIds = await _context.Users
                            .Where(u => dto.AssigneeIds.Contains(u.Userid))
                            .Select(u => u.Userid)
                            .ToListAsync();
                        var invalidIds = dto.AssigneeIds.Except(validUserIds).ToList();
                        if (invalidIds.Any())
                            return ApiResponse<ProjectResponseDto>.ErrorResponse($"Invalid user IDs: {string.Join(", ", invalidIds)}", 400);
                    }

                    var existingAssignees = await _context.Projectassignees
                        .Where(pa => pa.Projectid == projectId)
                        .ToListAsync();
                    _context.Projectassignees.RemoveRange(existingAssignees);

                    if (dto.AssigneeIds.Any())
                    {
                        foreach (var userId in dto.AssigneeIds)
                        {
                            await _context.Projectassignees.AddAsync(new Projectassignee
                            {
                                Projectid = projectId,
                                Userid = userId,
                                Assigneddate = DateTime.UtcNow
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();

                var result = await GetProjectWithDetailsAsync(projectId);
                return ApiResponse<ProjectResponseDto>.SuccessResponse(MapToResponseDto(result), "Project updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating project ID: {projectId}");
                return ApiResponse<ProjectResponseDto>.ErrorResponse("An error occurred while updating the project", 500);
            }
        }

        public async Task<ApiResponse<bool>> DeleteProjectAsync(int projectId)
        {
            try
            {
                var project = await _context.Projects.FindAsync(projectId);
                if (project == null)
                    return ApiResponse<bool>.ErrorResponse($"Project with ID {projectId} not found", 404);

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Project deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting project ID: {projectId}");
                return ApiResponse<bool>.ErrorResponse("An error occurred while deleting the project", 500);
            }
        }

        public async Task<ApiResponse<ProjectResponseDto>> UpdateProjectStatusAsync(int projectId, string status)
        {
            try
            {
                var validStatuses = new[] { "Active", "On Hold", "Pending", "Completed" };
                if (!validStatuses.Contains(status))
                    return ApiResponse<ProjectResponseDto>.ErrorResponse($"Invalid status. Valid: {string.Join(", ", validStatuses)}", 400);

                var project = await _context.Projects.FindAsync(projectId);
                if (project == null)
                    return ApiResponse<ProjectResponseDto>.ErrorResponse($"Project with ID {projectId} not found", 404);

                project.Status = status;
                if (status == "Completed")
                    project.Progress = 100;

                await _context.SaveChangesAsync();

                var result = await GetProjectWithDetailsAsync(projectId);
                return ApiResponse<ProjectResponseDto>.SuccessResponse(MapToResponseDto(result), "Status updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for project ID: {projectId}");
                return ApiResponse<ProjectResponseDto>.ErrorResponse("An error occurred", 500);
            }
        }

        public async Task<ApiResponse<ProjectResponseDto>> UpdateProjectProgressAsync(int projectId, int progress)
        {
            try
            {
                if (progress < 0 || progress > 100)
                    return ApiResponse<ProjectResponseDto>.ErrorResponse("Progress must be between 0 and 100", 400);

                var project = await _context.Projects.FindAsync(projectId);
                if (project == null)
                    return ApiResponse<ProjectResponseDto>.ErrorResponse($"Project with ID {projectId} not found", 404);

                project.Progress = progress;
                if (progress == 100)
                    project.Status = "Completed";

                await _context.SaveChangesAsync();

                var result = await GetProjectWithDetailsAsync(projectId);
                return ApiResponse<ProjectResponseDto>.SuccessResponse(MapToResponseDto(result), "Progress updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating progress for project ID: {projectId}");
                return ApiResponse<ProjectResponseDto>.ErrorResponse("An error occurred", 500);
            }
        }

        public async Task<ApiResponse<ProjectResponseDto>> AddProjectAssigneesAsync(int projectId, AssignProjectMembersDto dto)
        {
            try
            {
                var project = await _context.Projects.FindAsync(projectId);
                if (project == null)
                    return ApiResponse<ProjectResponseDto>.ErrorResponse($"Project with ID {projectId} not found", 404);

                var validUserIds = await _context.Users
                    .Where(u => dto.UserIds.Contains(u.Userid))
                    .Select(u => u.Userid)
                    .ToListAsync();
                var invalidIds = dto.UserIds.Except(validUserIds).ToList();
                if (invalidIds.Any())
                    return ApiResponse<ProjectResponseDto>.ErrorResponse($"Invalid user IDs: {string.Join(", ", invalidIds)}", 400);

                var existingIds = await _context.Projectassignees
                    .Where(pa => pa.Projectid == projectId)
                    .Select(pa => pa.Userid)
                    .ToListAsync();

                var newAssignees = dto.UserIds.Except(existingIds).ToList();
                foreach (var userId in newAssignees)
                {
                    await _context.Projectassignees.AddAsync(new Projectassignee
                    {
                        Projectid = projectId,
                        Userid = userId,
                        Assignedby = dto.AssignedBy,
                        Assigneddate = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();

                var result = await GetProjectWithDetailsAsync(projectId);
                return ApiResponse<ProjectResponseDto>.SuccessResponse(MapToResponseDto(result), $"{newAssignees.Count} assignee(s) added");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding assignees to project ID: {projectId}");
                return ApiResponse<ProjectResponseDto>.ErrorResponse("An error occurred", 500);
            }
        }

        public async Task<ApiResponse<ProjectResponseDto>> RemoveProjectAssigneeAsync(int projectId, int userId)
        {
            try
            {
                var assignee = await _context.Projectassignees
                    .FirstOrDefaultAsync(pa => pa.Projectid == projectId && pa.Userid == userId);

                if (assignee == null)
                    return ApiResponse<ProjectResponseDto>.ErrorResponse("User not assigned to this project", 404);

                _context.Projectassignees.Remove(assignee);
                await _context.SaveChangesAsync();

                var result = await GetProjectWithDetailsAsync(projectId);
                return ApiResponse<ProjectResponseDto>.SuccessResponse(MapToResponseDto(result), "Assignee removed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing assignee from project ID: {projectId}");
                return ApiResponse<ProjectResponseDto>.ErrorResponse("An error occurred", 500);
            }
        }

        // ========== HELPERS ==========
        private async Task<Project> GetProjectWithDetailsAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Projectassignees)
                    .ThenInclude(pa => pa.User)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Projectid == projectId);
        }

        private ProjectResponseDto MapToResponseDto(Project project)
        {
            var tasks = project.Tasks?.ToList() ?? new List<Models.Entities.Task>();

            // Get created by user info manually
            string? createdByName = null;
            if (project.Createdby.HasValue)
            {
                var createdByUser = _context.Users.Find(project.Createdby.Value);
                if (createdByUser != null)
                    createdByName = $"{createdByUser.Firstname} {createdByUser.Lastname}";
            }

            return new ProjectResponseDto
            {
                ProjectId = project.Projectid,
                ProjectName = project.Projectname,
                ClientId = project.Clientid,
                ClientName = project.Client?.Businessname,
                Developer = project.Developer,
                Manager = project.Manager,
                Progress = project.Progress,
                Budget = project.Budget,
                Deadline = project.Deadline,
                Status = project.Status,
                Description = project.Description,
                CreatedBy = project.Createdby,
                CreatedByName = createdByName,
                Assignees = project.Projectassignees?.Select(pa =>
                {
                    string? assignedByName = null;
                    if (pa.Assignedby.HasValue)
                    {
                        var assignedByUser = _context.Users.Find(pa.Assignedby.Value);
                        if (assignedByUser != null)
                            assignedByName = $"{assignedByUser.Firstname} {assignedByUser.Lastname}";
                    }

                    return new ProjectAssigneeDto
                    {
                        UserId = pa.Userid,
                        FirstName = pa.User?.Firstname,
                        LastName = pa.User?.Lastname,
                        Email = pa.User?.Email,
                        AssignedBy = pa.Assignedby,
                        AssignedByName = assignedByName,
                        AssignedDate = pa.Assigneddate
                    };
                }).ToList() ?? new List<ProjectAssigneeDto>(),
                TaskCount = tasks.Count,
                CompletedTaskCount = tasks.Count(t => t.Status == "Completed")
            };
        }

        private ProjectCardDto MapToCardDto(Project project)
        {
            var tasks = project.Tasks?.ToList() ?? new List<Models.Entities.Task>();
            return new ProjectCardDto
            {
                ProjectId = project.Projectid,
                ProjectName = project.Projectname,
                ClientName = project.Client?.Businessname,
                Progress = project.Progress,
                Deadline = project.Deadline,
                Status = project.Status,
                TaskCount = tasks.Count,
                CompletedTaskCount = tasks.Count(t => t.Status == "Completed"),
                AssigneeCount = project.Projectassignees?.Count ?? 0
            };
        }
    }
}
