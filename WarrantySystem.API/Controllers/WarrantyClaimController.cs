using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarrantySystem.Model.Entities;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;

namespace WarrantySystem.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WarrantyClaimController : Controller
    {
        private readonly IGenericRepo _repo;

        public WarrantyClaimController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var warrantyClaims = await _repo.GetAll<WarrantyClaim>();
                return Ok(ApiResponseFactory.Success(warrantyClaims));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve warranty claims."));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var warrantyClaim = await _repo.GetById<WarrantyClaim>(id);
                if (warrantyClaim == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "Warranty claim not found."));
                }
                return Ok(ApiResponseFactory.Success(warrantyClaim));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve warranty claims."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WarrantyClaim warrantyClaim)
        {
            try
            {
                warrantyClaim.CreatedDate = DateTime.Now;
                var createdWarrantyClaim = await _repo.Insert(warrantyClaim);
                return Ok(ApiResponseFactory.Success(createdWarrantyClaim, "Warranty claim created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to create warranty claim."));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WarrantyClaim warrantyClaim)
        {
            try
            {
                var existingWarrantyClaim = await _repo.GetById<WarrantyClaim>(id);
                if (existingWarrantyClaim == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "Warranty claim not found."));
                }

                warrantyClaim.Id = id;
                var updatedWarrantyClaim = await _repo.Update(warrantyClaim);

                return Ok(ApiResponseFactory.Success(updatedWarrantyClaim, "Warranty claim updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to update warranty claim."));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingWarrantyClaim = await _repo.GetById<WarrantyClaim>(id);
                if (existingWarrantyClaim == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "Warranty claim not found."));
                }

                await _repo.DeleteById<WarrantyClaim>(id);

                return Ok(ApiResponseFactory.Success(null, "Warranty claim deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to delete warranty claim."));
            }
        }
    }
}