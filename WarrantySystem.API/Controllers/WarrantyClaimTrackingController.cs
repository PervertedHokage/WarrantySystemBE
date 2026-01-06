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
        private readonly IGenericRepo _repo;

        public WarrantyClaimTrackingController(IGenericRepo repo)
        {
            _repo = repo;
        }
        [HttpGet]
        public async Task<IActionResult> GetData([FromQuery(Name = "claim-id")] int? ClaimId)
        {
            try
            {
                var warrantyClaims = (await _repo.FindByExpression<WarrantyClaimTracking>(
                    track => track.WarrantyClaimId == ClaimId)).OrderByDescending(track => track.CreatedDate);

                return Ok(ApiResponseFactory.Success(warrantyClaims));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve warranty claim tracking."));
            }
        }
    }
}
