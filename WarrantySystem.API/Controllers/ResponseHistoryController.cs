using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarrantySystem.Model.DTO;
using WarrantySystem.Model.Entities;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;

namespace WarrantySystem.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ResponseHistoryController : Controller
    {
        private readonly IGenericRepo _repo;

        public ResponseHistoryController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet("claim/{claimId}")]
        public async Task<IActionResult> GetByClaimId(int claimId)
        {
            try
            {
                var dtos = await _repo.ProcedureToList<ResponseHistoryDTO>("spGetResponseHistoryByClaimId",
                    ["p_ClaimId"], [claimId]);

                return Ok(ApiResponseFactory.Success(dtos));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve response history."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ResponseHistoryDTO dto)
        {
            try
            {
                var entity = new ResponseHistory
                {
                    Id = 0,
                    WarrantyClaimId = dto.WarrantyClaimId,
                    ResponseDate = dto.ResponseDate,
                    CustomerId = dto.CustomerId,
                    ResponseText = dto.ResponseContent,
                    ReceptionWorkerId = dto.ReceptionWorkerId,
                    CreatedBy = dto.HandledBy,
                    CreatedDate = DateTime.Now,
                    Status = 1, // Default status
                    Note = dto.Note
                };
                var created = await _repo.Insert(entity);
                return Ok(ApiResponseFactory.Success(created, "Response added successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to add response."));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ResponseHistoryDTO dto)
        {
            try
            {
                var existing = await _repo.GetById<ResponseHistory>(id);
                if (existing == null) return NotFound(ApiResponseFactory.Fail(null, "Response not found."));

                existing.ResponseDate = dto.ResponseDate;
                existing.ResponseText = dto.ResponseContent;
                existing.ReceptionWorkerId = dto.ReceptionWorkerId;
                existing.CreatedBy = dto.HandledBy; // Assuming we want to update who handled it last or keep it
                existing.UpdatedDate = DateTime.Now;
                existing.Note = dto.Note;

                var updated = await _repo.Update(existing);
                return Ok(ApiResponseFactory.Success(updated, "Response updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to update response."));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _repo.GetById<ResponseHistory>(id);
                if (existing == null) return NotFound(ApiResponseFactory.Fail(null, "Response not found."));

                await _repo.DeleteById<ResponseHistory>(id);
                return Ok(ApiResponseFactory.Success(null, "Response deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to delete response."));
            }
        }
    }
}
