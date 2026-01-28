using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarrantySystem.Model.DTO;
using WarrantySystem.Model.Entities;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;

namespace WarrantySystem.API.Controllers.WarrantyClaims
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WarrantyClaimUsedSparePartController : Controller
    {
        private readonly IGenericRepo _repo;

        public WarrantyClaimUsedSparePartController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet("claim/{claimId}")]
        public async Task<IActionResult> GetByClaimId(int claimId)
        {
            try
            {
                var dtos = await _repo.ProcedureToList<WarrantyClaimUsedSparePartDTO>("spGetUsedPartByClaimId",
                    ["p_ClaimId"], [claimId]);

                return Ok(ApiResponseFactory.Success(dtos));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve used spare parts."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WarrantyClaimUsedSparePartDTO dto)
        {
            try
            {
                var entity = new WarrantyClaimUsedSparePart
                {
                    WarrantyClaimId = dto.WarrantyClaimId,
                    SparePartId = dto.SparePartId,
                    UnitId = dto.UnitId,
                    Quantity = dto.Quantity,
                    Note = dto.Note,
                    IsDeleted = false
                };
                var created = await _repo.Insert(entity);
                return Ok(ApiResponseFactory.Success(created, "Spare part added successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to add spare part."));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WarrantyClaimUsedSparePartDTO dto)
        {
            try
            {
                var existing = await _repo.GetById<WarrantyClaimUsedSparePart>(id);
                if (existing == null) return NotFound(ApiResponseFactory.Fail(null, "Spare part not found."));

                existing.SparePartId = dto.SparePartId;
                existing.UnitId = dto.UnitId;
                existing.Quantity = dto.Quantity;
                existing.Note = dto.Note;

                var updated = await _repo.Update(existing);
                return Ok(ApiResponseFactory.Success(updated, "Spare part updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to update spare part."));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existing = await _repo.GetById<WarrantyClaimUsedSparePart>(id);
                if (existing == null) return NotFound(ApiResponseFactory.Fail(null, "Spare part not found."));

                // Soft delete
                existing.IsDeleted = true;
                await _repo.Update(existing);
                return Ok(ApiResponseFactory.Success(null, "Spare part deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to delete spare part."));
            }
        }
    }
}