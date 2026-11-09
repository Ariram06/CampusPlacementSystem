using ClosedXML.Excel;
using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Student;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IPlacementOfficerRepository _officerRepository;
    private readonly IAuthRepository _authRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        IStudentRepository studentRepository,
        IPlacementOfficerRepository officerRepository,
        IAuthRepository authRepository,
        UserManager<ApplicationUser> userManager,
        AppDbContext context,
        ILogger<StudentService> logger)
    {
        _studentRepository = studentRepository;
        _officerRepository = officerRepository;
        _authRepository = authRepository;
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<StudentResponseDto>> EnrollStudentAsync(EnrollStudentRequestDto request, string officerUserId)
    {
        var officer = await _officerRepository.GetByUserIdAsync(officerUserId);
        if (officer == null || officer.Status != OfficerStatus.Approved)
        {
            return ApiResponse<StudentResponseDto>.FailureResult("Only approved placement officers can enroll students.");
        }

        var regExists = await _studentRepository.RegisterNumberExistsInCollegeAsync(officer.CollegeId, request.RegisterNumber.Trim());
        if (regExists)
        {
            return ApiResponse<StudentResponseDto>.FailureResult("A student with this register number is already enrolled in your college.");
        }

        var emailExists = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (emailExists != null)
        {
            return ApiResponse<StudentResponseDto>.FailureResult("A user with this email address is already registered.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new ApplicationUser
            {
                UserName = request.Email.Trim().ToLower(),
                Email = request.Email.Trim().ToLower(),
                FullName = request.FullName.Trim(),
                PhoneNumber = request.PhoneNumber?.Trim(),
                AccountStatus = AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var userResult = await _userManager.CreateAsync(user, request.Password);
            if (!userResult.Succeeded)
            {
                var errors = userResult.Errors.Select(e => e.Description).ToList();
                return ApiResponse<StudentResponseDto>.FailureResult("Failed to create student user account.", errors);
            }

            var student = new Student
            {
                UserId = user.Id,
                CollegeId = officer.CollegeId,
                PlacementOfficerId = officer.PlacementOfficerId,
                RegisterNumber = request.RegisterNumber.Trim().ToUpper(),
                Department = request.Department.Trim(),
                Degree = request.Degree.Trim(),
                GraduationYear = request.GraduationYear,
                CGPA = request.CGPA,
                ActiveBacklogs = request.ActiveBacklogs,
                ResumeUrl = request.ResumeUrl?.Trim(),
                Status = StudentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _studentRepository.AddAsync(student);
            await transaction.CommitAsync();

            _logger.LogInformation("Student enrolled successfully with StudentId: {StudentId}, RegisterNumber: {RegNo}", student.StudentId, student.RegisterNumber);

            var response = MapToResponseDto(student, user, officer.College, officer.User.FullName);
            return ApiResponse<StudentResponseDto>.SuccessResult(response, "Student enrolled successfully. Awaiting placement officer approval.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to enroll student.");
            throw;
        }
    }

    public async Task<ApiResponse<BulkEnrollmentResultDto>> BulkEnrollStudentsAsync(IFormFile file, string officerUserId)
    {
        var officer = await _officerRepository.GetByUserIdAsync(officerUserId);
        if (officer == null || officer.Status != OfficerStatus.Approved)
        {
            return ApiResponse<BulkEnrollmentResultDto>.FailureResult("Only approved placement officers can perform bulk enrollment.");
        }

        if (file == null || file.Length == 0)
        {
            return ApiResponse<BulkEnrollmentResultDto>.FailureResult("Uploaded file is empty.");
        }

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".xlsx" && ext != ".csv")
        {
            return ApiResponse<BulkEnrollmentResultDto>.FailureResult("Only .xlsx and .csv file formats are supported for bulk student enrollment.");
        }

        var result = new BulkEnrollmentResultDto();

        if (ext == ".xlsx")
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                return ApiResponse<BulkEnrollmentResultDto>.FailureResult("Excel workbook contains no worksheets.");
            }

            var rows = worksheet.RangeUsed()?.RowsUsed()?.Skip(1); // Skip header row
            if (rows == null)
            {
                return ApiResponse<BulkEnrollmentResultDto>.FailureResult("Excel sheet contains no data rows.");
            }

            int rowNumber = 1;
            foreach (var row in rows)
            {
                rowNumber++;
                result.TotalRecords++;

                try
                {
                    var fullName = row.Cell(1).GetString().Trim();
                    var email = row.Cell(2).GetString().Trim().ToLower();
                    var password = row.Cell(3).GetString().Trim();
                    var regNo = row.Cell(4).GetString().Trim().ToUpper();
                    var department = row.Cell(5).GetString().Trim();
                    var degree = row.Cell(6).GetString().Trim();
                    var gradYearStr = row.Cell(7).GetString().Trim();
                    var cgpaStr = row.Cell(8).GetString().Trim();
                    var backlogsStr = row.Cell(9).GetString().Trim();
                    var phone = row.Cell(10).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(regNo))
                    {
                        result.FailedEnrollments++;
                        result.Errors.Add(new BulkRowErrorDto { RowNumber = rowNumber, RegisterNumber = regNo, Email = email, ErrorMessage = "Missing mandatory fields (FullName, Email, Password, or RegisterNumber)." });
                        continue;
                    }

                    if (!int.TryParse(gradYearStr, out var gradYear) || gradYear < 2000 || gradYear > 2100)
                    {
                        result.FailedEnrollments++;
                        result.Errors.Add(new BulkRowErrorDto { RowNumber = rowNumber, RegisterNumber = regNo, Email = email, ErrorMessage = "Invalid graduation year." });
                        continue;
                    }

                    if (!decimal.TryParse(cgpaStr, out var cgpa) || cgpa < 0 || cgpa > 10)
                    {
                        result.FailedEnrollments++;
                        result.Errors.Add(new BulkRowErrorDto { RowNumber = rowNumber, RegisterNumber = regNo, Email = email, ErrorMessage = "Invalid CGPA (must be between 0.00 and 10.00)." });
                        continue;
                    }

                    int.TryParse(backlogsStr, out var backlogs);

                    var regExists = await _studentRepository.RegisterNumberExistsInCollegeAsync(officer.CollegeId, regNo);
                    if (regExists)
                    {
                        result.FailedEnrollments++;
                        result.Errors.Add(new BulkRowErrorDto { RowNumber = rowNumber, RegisterNumber = regNo, Email = email, ErrorMessage = $"Duplicate Register Number '{regNo}' in college." });
                        continue;
                    }

                    var emailExists = await _userManager.FindByEmailAsync(email);
                    if (emailExists != null)
                    {
                        result.FailedEnrollments++;
                        result.Errors.Add(new BulkRowErrorDto { RowNumber = rowNumber, RegisterNumber = regNo, Email = email, ErrorMessage = $"Email '{email}' is already registered." });
                        continue;
                    }

                    using var transaction = await _context.Database.BeginTransactionAsync();
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = fullName,
                        PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone,
                        AccountStatus = AccountStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };

                    var userResult = await _userManager.CreateAsync(user, password);
                    if (!userResult.Succeeded)
                    {
                        result.FailedEnrollments++;
                        result.Errors.Add(new BulkRowErrorDto { RowNumber = rowNumber, RegisterNumber = regNo, Email = email, ErrorMessage = string.Join("; ", userResult.Errors.Select(e => e.Description)) });
                        await transaction.RollbackAsync();
                        continue;
                    }

                    var student = new Student
                    {
                        UserId = user.Id,
                        CollegeId = officer.CollegeId,
                        PlacementOfficerId = officer.PlacementOfficerId,
                        RegisterNumber = regNo,
                        Department = department,
                        Degree = degree,
                        GraduationYear = gradYear,
                        CGPA = cgpa,
                        ActiveBacklogs = backlogs,
                        Status = StudentStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _studentRepository.AddAsync(student);
                    await transaction.CommitAsync();

                    result.SuccessfulEnrollments++;
                }
                catch (Exception ex)
                {
                    result.FailedEnrollments++;
                    result.Errors.Add(new BulkRowErrorDto { RowNumber = rowNumber, ErrorMessage = ex.Message });
                }
            }
        }
        else // CSV support
        {
            using var reader = new StreamReader(file.OpenReadStream());
            string? headerLine = await reader.ReadLineAsync(); // Skip header
            int rowNumber = 1;

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                rowNumber++;
                result.TotalRecords++;

                var cols = line.Split(',');
                if (cols.Length < 9)
                {
                    result.FailedEnrollments++;
                    result.Errors.Add(new BulkRowErrorDto { RowNumber = rowNumber, ErrorMessage = "Insufficient columns in CSV row." });
                    continue;
                }

                try
                {
                    var fullName = cols[0].Trim();
                    var email = cols[1].Trim().ToLower();
                    var password = cols[2].Trim();
                    var regNo = cols[3].Trim().ToUpper();
                    var department = cols[4].Trim();
                    var degree = cols[5].Trim();
                    int.TryParse(cols[6].Trim(), out var gradYear);
                    decimal.TryParse(cols[7].Trim(), out var cgpa);
                    int.TryParse(cols[8].Trim(), out var backlogs);
                    var phone = cols.Length > 9 ? cols[9].Trim() : null;

                    var regExists = await _studentRepository.RegisterNumberExistsInCollegeAsync(officer.CollegeId, regNo);
                    if (regExists)
                    {
                        result.FailedEnrollments++;
                        result.Errors.Add(new BulkRowErrorDto { RowNumber = rowNumber, RegisterNumber = regNo, Email = email, ErrorMessage = $"Duplicate Register Number '{regNo}'." });
                        continue;
                    }

                    var emailExists = await _userManager.FindByEmailAsync(email);
                    if (emailExists != null)
                    {
                        result.FailedEnrollments++;
                        result.Errors.Add(new BulkRowErrorDto { RowNumber = rowNumber, RegisterNumber = regNo, Email = email, ErrorMessage = $"Email '{email}' is already registered." });
                        continue;
                    }

                    using var transaction = await _context.Database.BeginTransactionAsync();
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = fullName,
                        PhoneNumber = phone,
                        AccountStatus = AccountStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };

                    var userResult = await _userManager.CreateAsync(user, password);
                    if (!userResult.Succeeded)
                    {
                        result.FailedEnrollments++;
                        result.Errors.Add(new BulkRowErrorDto { RowNumber = rowNumber, RegisterNumber = regNo, Email = email, ErrorMessage = string.Join("; ", userResult.Errors.Select(e => e.Description)) });
                        await transaction.RollbackAsync();
                        continue;
                    }

                    var student = new Student
                    {
                        UserId = user.Id,
                        CollegeId = officer.CollegeId,
                        PlacementOfficerId = officer.PlacementOfficerId,
                        RegisterNumber = regNo,
                        Department = department,
                        Degree = degree,
                        GraduationYear = gradYear,
                        CGPA = cgpa,
                        ActiveBacklogs = backlogs,
                        Status = StudentStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _studentRepository.AddAsync(student);
                    await transaction.CommitAsync();

                    result.SuccessfulEnrollments++;
                }
                catch (Exception ex)
                {
                    result.FailedEnrollments++;
                    result.Errors.Add(new BulkRowErrorDto { RowNumber = rowNumber, ErrorMessage = ex.Message });
                }
            }
        }

        return ApiResponse<BulkEnrollmentResultDto>.SuccessResult(result, $"Bulk import completed: {result.SuccessfulEnrollments} enrolled, {result.FailedEnrollments} failed.");
    }

    public async Task<ApiResponse<PagedResult<StudentResponseDto>>> GetAllStudentsAsync(StudentFilterParams filterParams, string currentUserId, string currentUserRole)
    {
        if (currentUserRole == AppRoles.PlacementOfficer)
        {
            var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
            if (officer == null)
            {
                return ApiResponse<PagedResult<StudentResponseDto>>.FailureResult("Placement officer not found.");
            }
            filterParams.CollegeId = officer.CollegeId;
        }

        var result = await _studentRepository.GetAllAsync(filterParams);
        return ApiResponse<PagedResult<StudentResponseDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<PagedResult<StudentResponseDto>>> GetPendingStudentsAsync(PaginationParams paginationParams, string currentUserId, string currentUserRole)
    {
        var filter = new StudentFilterParams
        {
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            Search = paginationParams.Search,
            Status = StudentStatus.Pending
        };

        if (currentUserRole == AppRoles.PlacementOfficer)
        {
            var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
            if (officer == null)
            {
                return ApiResponse<PagedResult<StudentResponseDto>>.FailureResult("Placement officer not found.");
            }
            filter.CollegeId = officer.CollegeId;
        }

        var result = await _studentRepository.GetAllAsync(filter);
        return ApiResponse<PagedResult<StudentResponseDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<StudentResponseDto>> GetStudentByIdAsync(int id, string currentUserId, string currentUserRole)
    {
        var student = await _studentRepository.GetByIdAsync(id);
        if (student == null)
        {
            return ApiResponse<StudentResponseDto>.FailureResult("Student not found.");
        }

        // Authorization check: Officer can view only their college students; Student can view only own profile
        if (currentUserRole == AppRoles.PlacementOfficer)
        {
            var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
            if (officer == null || officer.CollegeId != student.CollegeId)
            {
                return ApiResponse<StudentResponseDto>.FailureResult("You are not authorized to view students from another college.");
            }
        }
        else if (currentUserRole == AppRoles.Student && student.UserId != currentUserId)
        {
            return ApiResponse<StudentResponseDto>.FailureResult("You are not authorized to view this student profile.");
        }

        return ApiResponse<StudentResponseDto>.SuccessResult(MapToResponseDto(student, student.User, student.College, student.EnrolledByOfficer.User.FullName));
    }

    public async Task<ApiResponse<StudentResponseDto>> GetCurrentStudentProfileAsync(string userId)
    {
        var student = await _studentRepository.GetByUserIdAsync(userId);
        if (student == null)
        {
            return ApiResponse<StudentResponseDto>.FailureResult("Student profile not found.");
        }

        return ApiResponse<StudentResponseDto>.SuccessResult(MapToResponseDto(student, student.User, student.College, student.EnrolledByOfficer.User.FullName));
    }

    public async Task<ApiResponse<StudentResponseDto>> UpdateStudentProfileAsync(string userId, UpdateStudentProfileRequestDto request)
    {
        var student = await _studentRepository.GetByUserIdAsync(userId);
        if (student == null)
        {
            return ApiResponse<StudentResponseDto>.FailureResult("Student profile not found.");
        }

        student.User.FullName = request.FullName.Trim();
        student.User.PhoneNumber = request.PhoneNumber?.Trim();
        student.User.UpdatedAt = DateTime.UtcNow;
        student.CGPA = request.CGPA;
        student.ActiveBacklogs = request.ActiveBacklogs;
        student.ResumeUrl = request.ResumeUrl?.Trim();

        await _studentRepository.UpdateAsync(student);
        _logger.LogInformation("Student profile updated for UserId: {UserId}", userId);

        return ApiResponse<StudentResponseDto>.SuccessResult(MapToResponseDto(student, student.User, student.College, student.EnrolledByOfficer.User.FullName), "Profile updated successfully.");
    }

    public async Task<ApiResponse<bool>> ApproveStudentAsync(int id, string officerUserId, string currentUserRole)
    {
        var student = await _studentRepository.GetByIdAsync(id);
        if (student == null)
        {
            return ApiResponse<bool>.FailureResult("Student not found.");
        }

        if (currentUserRole == AppRoles.PlacementOfficer)
        {
            var officer = await _officerRepository.GetByUserIdAsync(officerUserId);
            if (officer == null || officer.CollegeId != student.CollegeId)
            {
                return ApiResponse<bool>.FailureResult("You can approve only students enrolled in your own college.");
            }
        }

        student.Status = StudentStatus.Approved;
        student.ApprovedAt = DateTime.UtcNow;
        student.User.AccountStatus = AccountStatus.Active;
        student.User.UpdatedAt = DateTime.UtcNow;

        if (!await _userManager.IsInRoleAsync(student.User, AppRoles.Student))
        {
            await _userManager.AddToRoleAsync(student.User, AppRoles.Student);
        }

        await _studentRepository.UpdateAsync(student);
        _logger.LogInformation("Student ID {StudentId} approved", id);

        return ApiResponse<bool>.SuccessResult(true, "Student approved successfully. Login access is now enabled.");
    }

    public async Task<ApiResponse<bool>> RejectStudentAsync(int id, string officerUserId, string currentUserRole)
    {
        var student = await _studentRepository.GetByIdAsync(id);
        if (student == null)
        {
            return ApiResponse<bool>.FailureResult("Student not found.");
        }

        if (currentUserRole == AppRoles.PlacementOfficer)
        {
            var officer = await _officerRepository.GetByUserIdAsync(officerUserId);
            if (officer == null || officer.CollegeId != student.CollegeId)
            {
                return ApiResponse<bool>.FailureResult("You can reject only students enrolled in your own college.");
            }
        }

        student.Status = StudentStatus.Rejected;
        student.User.AccountStatus = AccountStatus.Rejected;
        student.User.UpdatedAt = DateTime.UtcNow;

        await _studentRepository.UpdateAsync(student);
        _logger.LogInformation("Student ID {StudentId} rejected", id);

        return ApiResponse<bool>.SuccessResult(true, "Student rejected successfully.");
    }

    public async Task<ApiResponse<bool>> SuspendStudentAsync(int id, string currentUserId, string currentUserRole)
    {
        var student = await _studentRepository.GetByIdAsync(id);
        if (student == null)
        {
            return ApiResponse<bool>.FailureResult("Student not found.");
        }

        if (currentUserRole == AppRoles.PlacementOfficer)
        {
            var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
            if (officer == null || officer.CollegeId != student.CollegeId)
            {
                return ApiResponse<bool>.FailureResult("You can suspend only students enrolled in your own college.");
            }
        }

        student.Status = StudentStatus.Suspended;
        student.User.AccountStatus = AccountStatus.Suspended;
        student.User.UpdatedAt = DateTime.UtcNow;

        await _studentRepository.UpdateAsync(student);
        await _authRepository.RevokeUserRefreshTokensAsync(student.UserId);

        _logger.LogInformation("Student ID {StudentId} suspended", id);
        return ApiResponse<bool>.SuccessResult(true, "Student suspended successfully.");
    }

    private static StudentResponseDto MapToResponseDto(Student s, ApplicationUser u, College c, string officerName)
    {
        return new StudentResponseDto
        {
            StudentId = s.StudentId,
            UserId = s.UserId,
            FullName = u.FullName,
            Email = u.Email ?? string.Empty,
            PhoneNumber = u.PhoneNumber,
            CollegeId = s.CollegeId,
            CollegeName = c.CollegeName,
            CollegeCode = c.CollegeCode,
            PlacementOfficerId = s.PlacementOfficerId,
            PlacementOfficerName = officerName,
            RegisterNumber = s.RegisterNumber,
            Department = s.Department,
            Degree = s.Degree,
            GraduationYear = s.GraduationYear,
            CGPA = s.CGPA,
            ActiveBacklogs = s.ActiveBacklogs,
            ResumeUrl = s.ResumeUrl,
            Status = s.Status,
            AccountStatus = u.AccountStatus,
            ApprovedAt = s.ApprovedAt,
            CreatedAt = s.CreatedAt,
            TotalApplications = s.JobApplications?.Count ?? 0
        };
    }
}
