using elite.DTOs;
using elite.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace elite.Controllers
{
    // Controllers/TrainersController.cs
    [ApiController]
    [Route("api/[controller]")]
    public class TrainersController : ControllerBase
    {
        private readonly ITrainerService _trainerService;
        private readonly ILogger<TrainersController> _logger;

        public TrainersController(ITrainerService trainerService, ILogger<TrainersController> logger)
        {
            _trainerService = trainerService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTrainers()
        {
            try
            {
                var trainers = await _trainerService.GetAllTrainersAsync();
                return Ok(trainers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all trainers");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrainerById(int id)
        {
            try
            {
                var trainer = await _trainerService.GetTrainerByIdAsync(id);
                return Ok(trainer);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trainer by ID");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateTrainer([FromForm] TrainerCreateDto trainerCreateDto)
        {
            try
            {
                var result = await _trainerService.CreateTrainerAsync(trainerCreateDto);
                return CreatedAtAction(nameof(GetTrainerById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trainer");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // Controllers/TrainersController.cs
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateTrainer(int id, [FromForm] TrainerUpdateDto trainerUpdateDto) // Changed parameter type
        {
            try
            {
                var result = await _trainerService.UpdateTrainerAsync(id, trainerUpdateDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trainer");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }




        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTrainer(int id)
        {
            try
            {
                var result = await _trainerService.DeleteTrainerAsync(id);
                return Ok(new { message = "Trainer deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting trainer");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}/classes")]
        public async Task<IActionResult> GetTrainerClasses(int id)
        {
            try
            {
                var classes = await _trainerService.GetTrainerClassesAsync(id);
                return Ok(classes);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trainer classes");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}/sessions")]
        public async Task<IActionResult> GetTrainerSessions(int id)
        {
            try
            {
                var sessions = await _trainerService.GetTrainerSessionsAsync(id);
                return Ok(sessions);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trainer sessions");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
