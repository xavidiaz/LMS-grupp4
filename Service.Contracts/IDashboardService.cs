
using LMS.Shared.DTOs.DashboardDtos;

namespace Service.Contracts;

public interface IDashboardService
{
    Task<DashboardDto?> GetStudentDashboardAsync(string userId);

    Task<TeacherDashboardDto?> GetTeacherDashboardAsync(string userId);
}
