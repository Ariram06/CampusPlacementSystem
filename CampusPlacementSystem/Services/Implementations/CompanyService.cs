using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Company;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Services.Implementations;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(ICompanyRepository companyRepository, AppDbContext context, ILogger<CompanyService> logger)
    {
        _companyRepository = companyRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<CompanyResponseDto>> RegisterCompanyAsync(CreateCompanyRequestDto request)
    {
        var nameExists = await _companyRepository.CompanyNameExistsAsync(request.CompanyName);
        if (nameExists)
        {
            return ApiResponse<CompanyResponseDto>.FailureResult("A company with this name is already registered.");
        }

        var emailExists = await _companyRepository.CompanyEmailExistsAsync(request.CompanyEmail);
        if (emailExists)
        {
            return ApiResponse<CompanyResponseDto>.FailureResult("A company with this email address is already registered.");
        }

        var company = new Company
        {
            CompanyName = request.CompanyName.Trim(),
            CompanyEmail = request.CompanyEmail.Trim().ToLower(),
            Website = request.Website?.Trim(),
            Industry = request.Industry.Trim(),
            Address = request.Address.Trim(),
            Description = request.Description?.Trim(),
            Status = CompanyStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _companyRepository.AddAsync(company);
        _logger.LogInformation("Company registered successfully with ID: {CompanyId}", company.CompanyId);

        return ApiResponse<CompanyResponseDto>.SuccessResult(MapToResponseDto(company), "Company registered successfully. Awaiting SuperAdmin approval.");
    }

    public async Task<ApiResponse<PagedResult<CompanyResponseDto>>> GetAllCompaniesAsync(PaginationParams paginationParams, CompanyStatus? status = null)
    {
        var result = await _companyRepository.GetAllAsync(paginationParams, status);
        return ApiResponse<PagedResult<CompanyResponseDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<PagedResult<CompanyResponseDto>>> GetPendingCompaniesAsync(PaginationParams paginationParams)
    {
        var result = await _companyRepository.GetAllAsync(paginationParams, CompanyStatus.Pending);
        return ApiResponse<PagedResult<CompanyResponseDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<CompanyResponseDto>> GetCompanyByIdAsync(int id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        if (company == null)
        {
            return ApiResponse<CompanyResponseDto>.FailureResult("Company not found.");
        }

        return ApiResponse<CompanyResponseDto>.SuccessResult(MapToResponseDto(company));
    }

    public async Task<ApiResponse<CompanyResponseDto>> UpdateCompanyAsync(int id, UpdateCompanyRequestDto request, string currentUserId, string currentUserRole)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        if (company == null)
        {
            return ApiResponse<CompanyResponseDto>.FailureResult("Company not found.");
        }

        // Ownership validation
        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _context.CompanyHRs
                .FirstOrDefaultAsync(h => h.UserId == currentUserId && h.CompanyId == id);

            if (hr == null)
            {
                return ApiResponse<CompanyResponseDto>.FailureResult("You are not authorized to update this company's details.");
            }
        }

        var emailExists = await _companyRepository.CompanyEmailExistsAsync(request.CompanyEmail, id);
        if (emailExists)
        {
            return ApiResponse<CompanyResponseDto>.FailureResult("Another company is already registered with this email address.");
        }

        company.CompanyName = request.CompanyName.Trim();
        company.CompanyEmail = request.CompanyEmail.Trim().ToLower();
        company.Website = request.Website?.Trim();
        company.Industry = request.Industry.Trim();
        company.Address = request.Address.Trim();
        company.Description = request.Description?.Trim();

        await _companyRepository.UpdateAsync(company);
        _logger.LogInformation("Company ID {CompanyId} updated by user {UserId}", id, currentUserId);

        return ApiResponse<CompanyResponseDto>.SuccessResult(MapToResponseDto(company), "Company details updated successfully.");
    }

    public async Task<ApiResponse<bool>> ApproveCompanyAsync(int id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        if (company == null)
        {
            return ApiResponse<bool>.FailureResult("Company not found.");
        }

        if (company.Status == CompanyStatus.Approved)
        {
            return ApiResponse<bool>.FailureResult("Company is already approved.");
        }

        company.Status = CompanyStatus.Approved;
        await _companyRepository.UpdateAsync(company);
        _logger.LogInformation("Company ID {CompanyId} approved by SuperAdmin", id);

        return ApiResponse<bool>.SuccessResult(true, "Company approved successfully.");
    }

    public async Task<ApiResponse<bool>> RejectCompanyAsync(int id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        if (company == null)
        {
            return ApiResponse<bool>.FailureResult("Company not found.");
        }

        company.Status = CompanyStatus.Rejected;
        await _companyRepository.UpdateAsync(company);
        _logger.LogInformation("Company ID {CompanyId} rejected by SuperAdmin", id);

        return ApiResponse<bool>.SuccessResult(true, "Company rejected successfully.");
    }

    public async Task<ApiResponse<bool>> SuspendCompanyAsync(int id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        if (company == null)
        {
            return ApiResponse<bool>.FailureResult("Company not found.");
        }

        company.Status = CompanyStatus.Suspended;
        await _companyRepository.UpdateAsync(company);
        _logger.LogInformation("Company ID {CompanyId} suspended by SuperAdmin", id);

        return ApiResponse<bool>.SuccessResult(true, "Company suspended successfully.");
    }

    private static CompanyResponseDto MapToResponseDto(Company c)
    {
        return new CompanyResponseDto
        {
            CompanyId = c.CompanyId,
            CompanyName = c.CompanyName,
            CompanyEmail = c.CompanyEmail,
            Website = c.Website,
            Industry = c.Industry,
            Address = c.Address,
            Description = c.Description,
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            TotalHRUsers = c.HRUsers?.Count ?? 0,
            TotalJobOpenings = c.JobOpenings?.Count ?? 0
        };
    }
}
