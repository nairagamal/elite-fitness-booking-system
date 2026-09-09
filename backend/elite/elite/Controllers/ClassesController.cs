using elite.DTOs;
using elite.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace elite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassesController : ControllerBase
    {
        private readonly IClassService _classService;
        private readonly ILogger<ClassesController> _logger;

        public ClassesController(IClassService classService, ILogger<ClassesController> logger)
        {
            _classService = classService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllClasses()
        {
            try
            {
                var classes = await _classService.GetAllClassesAsync();
                return Ok(classes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all classes");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClassById(int id)
        {
            try
            {
                var classObj = await _classService.GetClassByIdAsync(id);
                return Ok(classObj);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting class by ID");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateClass([FromBody] ClassCreateDto classCreateDto)
        {
            try
            {
                var result = await _classService.CreateClassAsync(classCreateDto);
                return CreatedAtAction(nameof(GetClassById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating class");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateClass(int id, [FromBody] ClassCreateDto classUpdateDto)
        {
            try
            {
                var result = await _classService.UpdateClassAsync(id, classUpdateDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating class");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            try
            {
                var result = await _classService.DeleteClassAsync(id);
                return Ok(new { message = "Class deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting class");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}/schedules")]
        public async Task<IActionResult> GetClassSchedules(int id)
        {
            try
            {
                var schedules = await _classService.GetClassSchedulesAsync(id);
                return Ok(schedules);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting class schedules");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("schedules")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateClassSchedule([FromBody] ClassScheduleCreateDto scheduleCreateDto)
        {
            try
            {
                var result = await _classService.CreateClassScheduleAsync(scheduleCreateDto);
                return CreatedAtAction(nameof(GetClassSchedules), new { id = result.ClassId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating class schedule");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("schedules/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteClassSchedule(int id)
        {
            try
            {
                var result = await _classService.DeleteClassScheduleAsync(id);
                return Ok(new { message = "Class schedule deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting class schedule");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }

}
