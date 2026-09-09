// Controllers/MembershipTypesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using elite.DTOs;
using elite.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace elite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class MembershipTypesController : ControllerBase
    {
        private readonly IMembershipTypeService _membershipTypeService;
        private readonly ILogger<MembershipTypesController> _logger;

        public MembershipTypesController(IMembershipTypeService membershipTypeService, ILogger<MembershipTypesController> logger)
        {
            _membershipTypeService = membershipTypeService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous] // Allow anyone to see membership types
        public async Task<IActionResult> GetAllMembershipTypes()
        {
            try
            {
                var types = await _membershipTypeService.GetAllMembershipTypesAsync();
                return Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting membership types");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // Add this new endpoint
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMembershipTypeById(int id)
        {
            try
            {
                var membershipType = await _membershipTypeService.GetMembershipTypeByIdAsync(id);
                return Ok(membershipType);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting membership type by ID");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateMembershipType([FromBody] MembershipTypeCreateDto createDto)
        {
            try
            {
                var result = await _membershipTypeService.CreateMembershipTypeAsync(createDto);
                return CreatedAtAction(nameof(GetAllMembershipTypes), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating membership type");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMembershipType(int id, [FromBody] MembershipTypeCreateDto updateDto)
        {
            try
            {
                var result = await _membershipTypeService.UpdateMembershipTypeAsync(id, updateDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating membership type");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleMembershipTypeStatus(int id, [FromBody] bool isActive)
        {
            try
            {
                var result = await _membershipTypeService.ToggleMembershipTypeStatusAsync(id, isActive);
                return Ok(new { message = "Membership type status updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling membership type status");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMembershipType(int id)
        {
            try
            {
                var result = await _membershipTypeService.DeleteMembershipTypeAsync(id);
                return Ok(new { message = "Membership type deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting membership type");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}