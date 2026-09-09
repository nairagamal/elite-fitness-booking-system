using elite.DTOs;

namespace elite.Interfaces
{
    // Interfaces/IClassService.cs
    public interface IClassService
    {
        Task<IEnumerable<ClassDto>> GetAllClassesAsync();
        Task<ClassDto> GetClassByIdAsync(int id);
        Task<ClassDto> CreateClassAsync(ClassCreateDto classCreateDto);
        Task<ClassDto> UpdateClassAsync(int id, ClassCreateDto classUpdateDto);
        Task<bool> DeleteClassAsync(int id);
        Task<IEnumerable<ClassScheduleDto>> GetClassSchedulesAsync(int classId);
        Task<ClassScheduleDto> CreateClassScheduleAsync(ClassScheduleCreateDto scheduleCreateDto);
        Task<bool> DeleteClassScheduleAsync(int scheduleId);
    }
}
