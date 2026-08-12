using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;

namespace CoDevelopAPI.Services.Interfaces
{
    public interface ITaskService
    {
        Task<ApiResponse<TaskResponseDto>> CreateTaskAsync(CreateTaskDto dto);
        Task<ApiResponse<TaskResponseDto>> GetTaskByIdAsync(int taskId);
        Task<ApiResponse<List<TaskResponseDto>>> GetTasksByProjectAsync(int projectId);
        Task<ApiResponse<List<TaskResponseDto>>> GetTasksByAssigneeAsync(int userId);
        Task<ApiResponse<List<TaskResponseDto>>> GetTasksByStatusAsync(int projectId, string status);
        Task<ApiResponse<TaskResponseDto>> UpdateTaskAsync(int taskId, UpdateTaskDto dto);
        Task<ApiResponse<bool>> DeleteTaskAsync(int taskId);
        Task<ApiResponse<TaskResponseDto>> UpdateTaskStatusAsync(int taskId, string status);
    }
}
