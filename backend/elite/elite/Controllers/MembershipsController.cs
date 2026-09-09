using elite.Data;
using elite.DTOs;
using elite.Interfaces;
using elite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace elite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MembershipsController : ControllerBase
    {
        private readonly IMembershipService _membershipService;
        private readonly ILogger<MembershipsController> _logger;

        public MembershipsController(IMembershipService membershipService, ILogger<MembershipsController> logger)
        {
            _membershipService = membershipService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PurchaseMembership([FromBody] PurchaseMembershipDto purchaseDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                purchaseDto.UserId = userId;

                var result = await _membershipService.PurchaseMembershipAsync(purchaseDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error purchasing membership");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserMembership()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var result = await _membershipService.GetUserMembershipAsync(userId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user membership");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }

    public class PurchaseMembershipRequest
        {
            public int UserId { get; set; }
            public string Type { get; set; }
        }
    }

