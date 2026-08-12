using Microsoft.EntityFrameworkCore;
using CoDevelopAPI.Data;
using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;
using CoDevelopAPI.Models.Entities;
using CoDevelopAPI.Services.Interfaces;

namespace CoDevelopAPI.Services.Implementations
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TaskService> _logger;

        public TaskService(ApplicationDbContext context, ILogger<TaskService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<TaskResponseDto>> CreateTaskAsync(CreateTaskDto dto)
        {
            try
            {
                // Validate project exists
                var project = await _context.Projects.FindAsync(dto.ProjectId);
                if (project == null)
                    return ApiResponse<TaskResponseDto>.ErrorResponse($"Project with ID {dto.ProjectId} not found", 400);

                // Validate assigned to user exists
                var assignedToUser = await _context.Users.FindAsync(dto.AssignedTo);
                if (assignedToUser == null)
                    return ApiResponse<TaskResponseDto>.ErrorResponse($"User with ID {dto.AssignedTo} not found", 400);

                // Validate assigned by user exists
                var assignedByUser = await _context.Users.FindAsync(dto.AssignedBy);
                if (assignedByUser == null)
                    return ApiResponse<TaskResponseDto>.ErrorResponse($"User with ID {dto.AssignedBy} not found", 400);

                // Validate dates
                if (dto.EndDateTime <= dto.StartDateTime)
                    return ApiResponse<TaskResponseDto>.ErrorResponse("End date must be after start date", 400);

                var task = new Models.Entities.Task
                {
                    Taskname = dto.TaskName,
                    Projectid = dto.ProjectId,
                    Startdatetime = dto.StartDateTime,
                    Enddatetime = dto.EndDateTime,
                    Status = dto.Status ?? "Pending",
                    Assignedto = dto.AssignedTo,
                    Assignedby = dto.AssignedBy,
                    Createddate = DateTime.UtcNow
                };

                await _context.Tasks.AddAsync(task);
                await _context.SaveChangesAsync();

                var result = await GetTaskWithDetailsAsync(task.Taskid);
                return ApiResponse<TaskResponseDto>.SuccessResponse(MapToResponseDto(result), "Task created successfully", 201);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return ApiResponse<TaskResponseDto>.ErrorResponse("An error occurred while creating the task", 500);
            }
        }

        public async Task<ApiResponse<TaskResponseDto>> GetTaskByIdAsync(int taskId)
        {
            try
            {
                var task = await GetTaskWithDetailsAsync(taskId);
                if (task == null)
                    return ApiResponse<TaskResponseDto>.ErrorResponse($"Task with ID {taskId} not found", 404);

                return ApiResponse<TaskResponseDto>.SuccessResponse(MapToResponseDto(task));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving task ID: {taskId}");
                return ApiResponse<TaskResponseDto>.ErrorResponse("An error occurred while retrieving the task", 500);
            }
        }

        public async Task<ApiResponse<List<TaskResponseDto>>> GetTasksByProjectAsync(int projectId)
        {
            try
            {
                var project = await _context.Projects.FindAsync(projectId);
                if (project == null)
                    return ApiResponse<List<TaskResponseDto>>.ErrorResponse($"Project with ID {projectId} not found", 404);

                var tasks = await _context.Tasks
                    .Include(t => t.Project)
                    .Include(t => t.AssignedtoNavigation)
                    .Include(t => t.AssignedbyNavigation)
                    .Where(t => t.Projectid == projectId)
                    .OrderBy(t => t.Startdatetime)
                    .ToListAsync();

                return ApiResponse<List<TaskResponseDto>>.SuccessResponse(
                    tasks.Select(MapToResponseDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tasks for project ID: {projectId}");
                return ApiResponse<List<TaskResponseDto>>.ErrorResponse("An error occurred while retrieving tasks", 500);
            }
        }

        public async Task<ApiResponse<List<TaskResponseDto>>> GetTasksByAssigneeAsync(int userId)
        {
            try
            {
                var tasks = await _context.Tasks
                    .Include(t => t.Project)
                    .Include(t => t.AssignedtoNavigation)
                    .Include(t => t.AssignedbyNavigation)
                    .Where(t => t.Assignedto == userId)
                    .OrderByDescending(t => t.Createddate)
                    .ToListAsync();

                return ApiResponse<List<TaskResponseDto>>.SuccessResponse(
                    tasks.Select(MapToResponseDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tasks for user ID: {userId}");
                return ApiResponse<List<TaskResponseDto>>.ErrorResponse("An error occurred while retrieving tasks", 500);
            }
        }

        public async Task<ApiResponse<List<TaskResponseDto>>> GetTasksByStatusAsync(int projectId, string status)
        {
            try
            {
                var validStatuses = new[] { "Pending", "In Progress", "Completed", "On Hold" };
                if (!validStatuses.Contains(status))
                    return ApiResponse<List<TaskResponseDto>>.ErrorResponse(
                        $"Invalid status. Valid values: {string.Join(", ", validStatuses)}", 400);

                var tasks = await _context.Tasks
                    .Include(t => t.Project)
                    .Include(t => t.AssignedtoNavigation)
                    .Include(t => t.AssignedbyNavigation)
                    .Where(t => t.Projectid == projectId && t.Status == status)
                    .OrderBy(t => t.Startdatetime)
                    .ToListAsync();

                return ApiResponse<List<TaskResponseDto>>.SuccessResponse(
                    tasks.Select(MapToResponseDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving tasks by status for project ID: {projectId}");
                return ApiResponse<List<TaskResponseDto>>.ErrorResponse("An error occurred while retrieving tasks", 500);
            }
        }

        public async Task<ApiResponse<TaskResponseDto>> UpdateTaskAsync(int taskId, UpdateTaskDto dto)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
                    return ApiResponse<TaskResponseDto>.ErrorResponse($"Task with ID {taskId} not found", 404);

                if (dto.EndDateTime <= dto.StartDateTime)
                    return ApiResponse<TaskResponseDto>.ErrorResponse("End date must be after start date", 400);

                if (dto.AssignedTo.HasValue)
                {
                    var user = await _context.Users.FindAsync(dto.AssignedTo.Value);
                    if (user == null)
                        return ApiResponse<TaskResponseDto>.ErrorResponse($"User with ID {dto.AssignedTo} not found", 400);
                }

                task.Taskname = dto.TaskName;
                task.Startdatetime = dto.StartDateTime;
                task.Enddatetime = dto.EndDateTime;
                task.Assignedto = dto.AssignedTo ?? task.Assignedto;
                task.Status = dto.Status ?? task.Status;

                await _context.SaveChangesAsync();

                var result = await GetTaskWithDetailsAsync(taskId);
                return ApiResponse<TaskResponseDto>.SuccessResponse(MapToResponseDto(result), "Task updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating task ID: {taskId}");
                return ApiResponse<TaskResponseDto>.ErrorResponse("An error occurred while updating the task", 500);
            }
        }

        public async Task<ApiResponse<bool>> DeleteTaskAsync(int taskId)
        {
            try
            {
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
                    return ApiResponse<bool>.ErrorResponse($"Task with ID {taskId} not found", 404);

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Task deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting task ID: {taskId}");
                return ApiResponse<bool>.ErrorResponse("An error occurred while deleting the task", 500);
            }
        }

        public async Task<ApiResponse<TaskResponseDto>> UpdateTaskStatusAsync(int taskId, string status)
        {
            try
            {
                var validStatuses = new[] { "Pending", "In Progress", "Completed", "On Hold" };
                if (!validStatuses.Contains(status))
                    return ApiResponse<TaskResponseDto>.ErrorResponse(
                        $"Invalid status. Valid values: {string.Join(", ", validStatuses)}", 400);

                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
                    return ApiResponse<TaskResponseDto>.ErrorResponse($"Task with ID {taskId} not found", 404);

                task.Status = status;
                await _context.SaveChangesAsync();

                var result = await GetTaskWithDetailsAsync(taskId);
                return ApiResponse<TaskResponseDto>.SuccessResponse(MapToResponseDto(result), "Task status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for task ID: {taskId}");
                return ApiResponse<TaskResponseDto>.ErrorResponse("An error occurred", 500);
            }
        }
        #pragma warning disable CS8601, CS8602, CS8603, CS8604, CS8618, CS8619
        // ========== HELPERS ==========
        private async Task<Models.Entities.Task> GetTaskWithDetailsAsync(int taskId)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedtoNavigation)
                .Include(t => t.AssignedbyNavigation)
                .FirstOrDefaultAsync(t => t.Taskid == taskId);
        }

        private TaskResponseDto MapToResponseDto(Models.Entities.Task task)
        {
            return new TaskResponseDto
            {
                TaskId = task.Taskid,
                TaskName = task.Taskname,
                ProjectId = task.Projectid,
                ProjectName = task.Project?.Projectname,
                StartDateTime = task.Startdatetime,
                EndDateTime = task.Enddatetime,
                Status = task.Status,
                AssignedTo = task.Assignedto,
                AssignedToName = task.AssignedtoNavigation != null
                    ? $"{task.AssignedtoNavigation.Firstname} {task.AssignedtoNavigation.Lastname}"
                    : null,
                AssignedBy = task.Assignedby,
                AssignedByName = task.AssignedbyNavigation != null
                    ? $"{task.AssignedbyNavigation.Firstname} {task.AssignedbyNavigation.Lastname}"
                    : null,
                CreatedDate = task.Createddate
            };
        }
    }
}
