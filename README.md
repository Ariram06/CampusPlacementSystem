# Campus Placement Management System – Backend API

A high-performance, modular ASP.NET Core Web API built with **.NET 10**, **Entity Framework Core 10**, and **Microsoft SQL Server**, designed specifically for campus placement workflows, recruitment drives, candidate evaluations, and institutional placement reporting.

---

## 1. Technology Stack

- **Framework:** .NET 10.0 (ASP.NET Core Web API)
- **Data Access:** Entity Framework Core 10.0
- **Database:** Microsoft SQL Server (`CampusPlacementSystemDB`)
- **Authentication:** ASP.NET Core Identity + JWT Bearer Tokens
- **Token Security:** SHA-256 hashed refresh tokens, rotation & reuse detection
- **API Documentation:** Scalar OpenAPI (`/scalar/v1`)
- **Architecture:** Layered Architecture (`Controllers` &rarr; `Services` &rarr; `Repositories` &rarr; `Data`)

---

## 2. Solution Structure

```
CampusPlacementSystem/
├── Controllers/
│   ├── AuthController.cs
│   ├── CollegeController.cs
│   ├── PlacementOfficerController.cs
│   ├── CompanyController.cs
│   ├── CompanyHRController.cs
│   ├── StudentController.cs
│   ├── JobController.cs
│   ├── ApplicationController.cs
│   ├── RecruitmentDriveController.cs
│   ├── InterviewController.cs
│   ├── ReportController.cs
│   └── HealthCheckController.cs
│
├── Data/
│   ├── AppDbContext.cs
│   └── DbSeeder.cs
│
├── Models/
│   ├── ApplicationUser.cs
│   ├── College.cs
│   ├── PlacementOfficer.cs
│   ├── Company.cs
│   ├── CompanyHR.cs
│   ├── Student.cs
│   ├── JobOpening.cs
│   ├── JobTargetCollege.cs
│   ├── JobEligibleDepartment.cs
│   ├── JobEligibleGraduationYear.cs
│   ├── JobApplication.cs
│   ├── RecruitmentDrive.cs
│   ├── InterviewRound.cs
│   ├── InterviewSchedule.cs
│   ├── ApplicationStatusHistory.cs
│   └── RefreshToken.cs
│
├── DTOs/
│   ├── Auth/
│   ├── College/
│   ├── PlacementOfficer/
│   ├── Company/
│   ├── CompanyHR/
│   ├── Student/
│   ├── Job/
│   ├── Application/
│   ├── RecruitmentDrive/
│   ├── Interview/
│   ├── Reports/
│   └── Common/
│
├── Enums/
│   ├── AccountStatus.cs
│   ├── AppRoles.cs
│   ├── CollegeStatus.cs
│   ├── CompanyStatus.cs
│   ├── StudentStatus.cs
│   ├── JobStatus.cs
│   ├── JobType.cs
│   ├── ApplicationStatus.cs
│   ├── DriveStatus.cs
│   └── InterviewStatus.cs
│
├── Repositories/
│   ├── Interfaces/
│   └── Implementations/
│
├── Services/
│   ├── Interfaces/
│   └── Implementations/
│
├── Middleware/
│   └── GlobalExceptionMiddleware.cs
│
├── Migrations/
├── Program.cs
├── appsettings.json
└── CampusPlacementSystem.csproj
```

---

## 3. Role-Based Access Control Matrix

The system enforces 4 primary roles:

| Module / Operation | SuperAdmin | PlacementOfficer | CompanyHR | Student |
| :--- | :---: | :---: | :---: | :---: |
| **College Management** | Full Control (Approve/Reject/Suspend) | Read Institutional Profile | - | - |
| **Placement Officers** | Approve / Manage Accounts | Self-Profile | - | - |
| **Company Management** | Full Control (Approve/Suspend) | View Approved Companies | Read Company Profile | View Approved Companies |
| **Company HR** | Approve HR Accounts | - | Self-Profile | - |
| **Student Enrollment** | View All | Enroll, Bulk Import, Approve | - | Self-Profile |
| **Job Postings** | View All | View Targeted Jobs | Create, Publish, Manage | View Eligible Jobs |
| **Job Applications** | View All | View College Applicants | Screen, Shortlist, Select | Apply, Track, Withdraw |
| **Recruitment Drives** | View All | View College Drives | Schedule & Manage | View College Drives |
| **Interviews & Rounds** | View All | View College Schedules | Define Rounds, Schedule, Feedback | View Personal Schedules |
| **Reports & Analytics** | Platform Analytics | Institutional Report | Corporate Recruitment Report | Student Placement Status |

---

## 4. Key Security & Architectural Highlights

1. **SHA-256 Hashed Refresh Tokens:** Refresh tokens are never stored in plain text. Any attempt to reuse a consumed or revoked token immediately triggers a security revocation cascade across all active sessions for that user account.
2. **Deterministic Eligibility Engine:** Reusable pure business rule engine (`JobService.EvaluateEligibility`) that evaluates candidate CGPA, standing backlogs, department matching, graduation year matching, and college targeting.
3. **Audit Trail Logging:** Every job application status transition (`Applied` &rarr; `UnderReview` &rarr; `Shortlisted` &rarr; `InterviewScheduled` &rarr; `Selected` &rarr; `Rejected` &rarr; `Withdrawn`) is immutably recorded in `ApplicationStatusHistories`.
4. **Security Headers:** Enforced `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `X-XSS-Protection: 1; mode=block`, and `Referrer-Policy: strict-origin-when-cross-origin`.
5. **CORS Configured for Angular:** Configured to integrate seamlessly with an Angular frontend (`http://localhost:4200` and `https://localhost:4200`).

---

## 5. Getting Started

### Prerequisites
- .NET 10 SDK
- Microsoft SQL Server (LocalDB, Express, or standard instance)

### Database Configuration
Ensure `appsettings.json` points to your SQL Server instance:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=CampusPlacementSystemDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

### Run Migrations & Start API
```powershell
dotnet ef database update --project CampusPlacementSystem
dotnet run --project CampusPlacementSystem --urls "http://localhost:5000"
```

### Seeded SuperAdmin Account
- **Email:** `admin@campusplacement.com`
- **Password:** `Admin@123456`

### Interactive API Documentation
Navigate to `http://localhost:5000/scalar/v1` to explore and execute the API directly.
