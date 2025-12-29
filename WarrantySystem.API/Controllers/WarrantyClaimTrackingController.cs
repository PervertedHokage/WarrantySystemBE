using Microsoft.AspNetCore.Mvc;
using WarrantySystem.Model.Entities;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;

namespace WarrantySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarrantyClaimTrackingController : Controller
    {
        private IGenericRepo _repo;

        public WarrantyClaimTrackingController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery(Name = "claim-id")] int claimId)
        {
            try
            {
                var warrantyClaimTrackings = await _repo.FindByExpression<WarrantyClaimTracking>
                    (t => t.WarrantyClaimId == claimId);
                return Ok(ApiResponseFactory.Success(warrantyClaimTrackings));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve warrantyClaimTrackings."));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var warrantyClaimTracking = await _repo.GetById<WarrantyClaimTracking>(id);
                if (warrantyClaimTracking == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "WarrantyClaimTracking not found."));
                }
                return Ok(ApiResponseFactory.Success(warrantyClaimTracking));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve warrantyClaimTracking."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WarrantyClaimTracking warrantyClaimTracking)
        {
            try
            {
                var createdWarrantyClaimTracking = await _repo.Insert(warrantyClaimTracking);
                return Ok(ApiResponseFactory.Success(createdWarrantyClaimTracking, "WarrantyClaimTracking created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to create warrantyClaimTracking."));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WarrantyClaimTracking warrantyClaimTracking)
        {
            try
            {
                var existingWarrantyClaimTracking = await _repo.GetById<WarrantyClaimTracking>(id);
                if (existingWarrantyClaimTracking == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "WarrantyClaimTracking not found."));
                }
                warrantyClaimTracking.Id = id; // Ensure the ID is set for update
                var updatedWarrantyClaimTracking = await _repo.Update(warrantyClaimTracking);
                return Ok(ApiResponseFactory.Success(updatedWarrantyClaimTracking, "WarrantyClaimTracking updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to update warrantyClaimTracking."));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingWarrantyClaimTracking = await _repo.GetById<WarrantyClaimTracking>(id);
                if (existingWarrantyClaimTracking == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "WarrantyClaimTracking not found."));
                }
                await _repo.DeleteById<WarrantyClaimTracking>(id);
                return Ok(ApiResponseFactory.Success(null, "WarrantyClaimTracking deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to delete warrantyClaimTracking."));
            }
        }
    }
}
