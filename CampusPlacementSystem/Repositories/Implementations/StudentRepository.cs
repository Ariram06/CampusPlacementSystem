using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Student;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Repositories.Implementations;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.College)
            .Include(s => s.EnrolledByOfficer)
                .ThenInclude(po => po.User)
            .Include(s => s.JobApplications)
            .FirstOrDefaultAsync(s => s.StudentId == id);
    }

    public async Task<Student?> GetByUserIdAsync(string userId)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.College)
            .Include(s => s.EnrolledByOfficer)
                .ThenInclude(po => po.User)
            .Include(s => s.JobApplications)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<Student?> GetByCollegeAndRegisterNumberAsync(int collegeId, string registerNumber)
    {
        return await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.CollegeId == collegeId && s.RegisterNumber == registerNumber);
    }

    public async Task<bool> RegisterNumberExistsInCollegeAsync(int collegeId, string registerNumber, int? excludeStudentId = null)
    {
        return await _context.Students
            .AnyAsync(s => s.CollegeId == collegeId 
                        && s.RegisterNumber == registerNumber 
                        && (!excludeStudentId.HasValue || s.StudentId != excludeStudentId.Value));
    }

    public async Task<PagedResult<StudentResponseDto>> GetAllAsync(StudentFilterParams filterParams)
    {
        var query = _context.Students
            .Include(s => s.User)
            .Include(s => s.College)
            .Include(s => s.EnrolledByOfficer)
                .ThenInclude(po => po.User)
            .Include(s => s.JobApplications)
            .AsNoTracking()
            .AsQueryable();

        if (filterParams.CollegeId.HasValue)
        {
            query = query.Where(s => s.CollegeId == filterParams.CollegeId.Value);
        }

        if (filterParams.Status.HasValue)
        {
            query = query.Where(s => s.Status == filterParams.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Department))
        {
            var dept = filterParams.Department.Trim().ToLower();
            query = query.Where(s => s.Department.ToLower() == dept);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Degree))
        {
            var deg = filterParams.Degree.Trim().ToLower();
            query = query.Where(s => s.Degree.ToLower() == deg);
        }

        if (filterParams.GraduationYear.HasValue)
        {
            query = query.Where(s => s.GraduationYear == filterParams.GraduationYear.Value);
        }

        if (filterParams.MinCGPA.HasValue)
        {
            query = query.Where(s => s.CGPA >= filterParams.MinCGPA.Value);
        }

        if (filterParams.MaxBacklogs.HasValue)
        {
            query = query.Where(s => s.ActiveBacklogs <= filterParams.MaxBacklogs.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Search))
        {
            var search = filterParams.Search.Trim().ToLower();
            query = query.Where(s => s.User.FullName.ToLower().Contains(search)
                                  || s.RegisterNumber.ToLower().Contains(search)
                                  || (s.User.Email != null && s.User.Email.ToLower().Contains(search))
                                  || s.Department.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(s => new StudentResponseDto
            {
                StudentId = s.StudentId,
                UserId = s.UserId,
                FullName = s.User.FullName,
                Email = s.User.Email ?? string.Empty,
                PhoneNumber = s.User.PhoneNumber,
                CollegeId = s.CollegeId,
                CollegeName = s.College.CollegeName,
                CollegeCode = s.College.CollegeCode,
                PlacementOfficerId = s.PlacementOfficerId,
                PlacementOfficerName = s.EnrolledByOfficer.User.FullName,
                RegisterNumber = s.RegisterNumber,
                Department = s.Department,
                Degree = s.Degree,
                GraduationYear = s.GraduationYear,
                CGPA = s.CGPA,
                ActiveBacklogs = s.ActiveBacklogs,
                ResumeUrl = s.ResumeUrl,
                Status = s.Status,
                AccountStatus = s.User.AccountStatus,
                ApprovedAt = s.ApprovedAt,
                CreatedAt = s.CreatedAt,
                TotalApplications = s.JobApplications.Count
            })
            .ToListAsync();

        return new PagedResult<StudentResponseDto>(items, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<Student> AddAsync(Student student)
    {
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();
        return student;
    }

    public async Task UpdateAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Students.AnyAsync(s => s.StudentId == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
