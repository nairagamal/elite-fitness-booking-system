using elite.DTOs;
using elite.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace elite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnlineSessionsController : ControllerBase
    {
        private readonly IOnlineSessionService _sessionService;
        private readonly ILogger<OnlineSessionsController> _logger;

        public OnlineSessionsController(IOnlineSessionService sessionService, ILogger<OnlineSessionsController> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSessions()
        {
            try
            {
                var sessions = await _sessionService.GetAllSessionsAsync();
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all sessions");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSessionById(int id)
        {
            try
            {
                var session = await _sessionService.GetSessionByIdAsync(id);
                return Ok(session);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session by ID");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSession([FromBody] OnlineSessionCreateDto sessionCreateDto)
        {
            try
            {
                var result = await _sessionService.CreateSessionAsync(sessionCreateDto);
                return CreatedAtAction(nameof(GetSessionById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating session");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSession(int id, [FromBody] OnlineSessionCreateDto sessionUpdateDto)
        {
            try
            {
                var result = await _sessionService.UpdateSessionAsync(id, sessionUpdateDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating session");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSession(int id)
        {
            try
            {
                var result = await _sessionService.DeleteSessionAsync(id);
                return Ok(new { message = "Session deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting session");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}/schedules")]
        public async Task<IActionResult> GetSessionSchedules(int id)
        {
            try
            {
                var schedules = await _sessionService.GetSessionSchedulesAsync(id);
                return Ok(schedules);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session schedules");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("schedules")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSessionSchedule([FromBody] OnlineSessionScheduleCreateDto scheduleCreateDto)
        {
            try
            {
                var result = await _sessionService.CreateSessionScheduleAsync(scheduleCreateDto);
                return CreatedAtAction(nameof(GetSessionSchedules), new { id = result.OnlineSessionId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating session schedule");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("schedules/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSessionSchedule(int id)
        {
            try
            {
                var result = await _sessionService.DeleteSessionScheduleAsync(id);
                return Ok(new { message = "Session schedule deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting session schedule");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
