using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Student;
using CampusPlacementSystem.Models;

namespace CampusPlacementSystem.Repositories.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id);
    Task<Student?> GetByUserIdAsync(string userId);
    Task<Student?> GetByCollegeAndRegisterNumberAsync(int collegeId, string registerNumber);
    Task<bool> RegisterNumberExistsInCollegeAsync(int collegeId, string registerNumber, int? excludeStudentId = null);
    Task<PagedResult<StudentResponseDto>> GetAllAsync(StudentFilterParams filterParams);
    Task<Student> AddAsync(Student student);
    Task UpdateAsync(Student student);
    Task<bool> ExistsAsync(int id);
    Task SaveChangesAsync();
}
