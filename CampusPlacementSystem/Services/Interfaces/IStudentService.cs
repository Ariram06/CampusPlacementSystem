using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Student;

namespace CampusPlacementSystem.Services.Interfaces;

public interface IStudentService
{
    Task<ApiResponse<StudentResponseDto>> EnrollStudentAsync(EnrollStudentRequestDto request, string officerUserId);
    Task<ApiResponse<BulkEnrollmentResultDto>> BulkEnrollStudentsAsync(IFormFile file, string officerUserId);
    Task<ApiResponse<PagedResult<StudentResponseDto>>> GetAllStudentsAsync(StudentFilterParams filterParams, string currentUserId, string currentUserRole);
    Task<ApiResponse<PagedResult<StudentResponseDto>>> GetPendingStudentsAsync(PaginationParams paginationParams, string currentUserId, string currentUserRole);
    Task<ApiResponse<StudentResponseDto>> GetStudentByIdAsync(int id, string currentUserId, string currentUserRole);
    Task<ApiResponse<StudentResponseDto>> GetCurrentStudentProfileAsync(string userId);
    Task<ApiResponse<StudentResponseDto>> UpdateStudentProfileAsync(string userId, UpdateStudentProfileRequestDto request);
    Task<ApiResponse<bool>> ApproveStudentAsync(int id, string officerUserId, string currentUserRole);
    Task<ApiResponse<bool>> RejectStudentAsync(int id, string officerUserId, string currentUserRole);
    Task<ApiResponse<bool>> SuspendStudentAsync(int id, string currentUserId, string currentUserRole);
}
