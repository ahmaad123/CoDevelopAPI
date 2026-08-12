using CoDevelopAPI.Models;
using CoDevelopAPI.Models.DTOs;

namespace CoDevelopAPI.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ApiResponse<ProjectResponseDto>> CreateProjectAsync(CreateProjectDto dto);
        Task<ApiResponse<ProjectResponseDto>> GetProjectByIdAsync(int projectId);
        Task<ApiResponse<List<ProjectCardDto>>> GetAllProjectsAsync();
        Task<ApiResponse<List<ProjectCardDto>>> GetProjectsByStatusAsync(string status);
        Task<ApiResponse<List<ProjectCardDto>>> GetProjectsByClientAsync(int clientId);
        Task<ApiResponse<List<ProjectCardDto>>> GetProjectsByAssigneeAsync(int userId);
        Task<ApiResponse<ProjectResponseDto>> UpdateProjectAsync(int projectId, UpdateProjectDto dto);
        Task<ApiResponse<bool>> DeleteProjectAsync(int projectId);
        Task<ApiResponse<ProjectResponseDto>> UpdateProjectStatusAsync(int projectId, string status);
        Task<ApiResponse<ProjectResponseDto>> UpdateProjectProgressAsync(int projectId, int progress);
        Task<ApiResponse<ProjectResponseDto>> AddProjectAssigneesAsync(int projectId, AssignProjectMembersDto dto);
        Task<ApiResponse<ProjectResponseDto>> RemoveProjectAssigneeAsync(int projectId, int userId);
    }
}
