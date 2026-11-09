namespace CampusPlacementSystem.DTOs.Student;

public class BulkEnrollmentResultDto
{
    public int TotalRecords { get; set; }
    public int SuccessfulEnrollments { get; set; }
    public int FailedEnrollments { get; set; }
    public List<BulkRowErrorDto> Errors { get; set; } = new();
}

public class BulkRowErrorDto
{
    public int RowNumber { get; set; }
    public string? RegisterNumber { get; set; }
    public string? Email { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
