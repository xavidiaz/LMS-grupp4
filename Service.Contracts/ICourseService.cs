using LMS.Shared.DTOs.CourseDtos;

namespace Service.Contracts;

public interface ICourseService
{
    Task<IEnumerable<CourseDto>> GetCoursesAsync();
    Task<CourseDto?> GetCourseAsync(int id);
    Task<CourseDto> NewCourseAsync(NewCourseDto course);
    Task<bool> DeleteCourseAsync(int id);
    Task<CourseDto> PatchCourseAsync(PatchCourseDto course);
}
