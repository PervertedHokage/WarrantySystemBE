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
    public class WarrantyClaimController : Controller
    {
        private readonly IGenericRepo _repo;

        public WarrantyClaimController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetData([FromQuery(Name = "phone-number")] string? phoneNumber,
            [FromQuery(Name = "email")] string? email, [FromQuery(Name = "claim-no")] string? claimNo)
        {
            try
            {
                var warrantyClaims = await _repo.ProcedureToList<WarrantyClaimDTO>("spGetWarrantyClaims",
                    ["p_PhoneNumber", "p_Email", "p_ClaimNo", "p_FromDate", "p_ToDate", "p_Status"],
                    [phoneNumber, email, claimNo, null, null, 0]);

                return Ok(ApiResponseFactory.Success(warrantyClaims));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve warranty claims."));
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetDataAsAdmin(
            [FromQuery(Name = "phone-number")] string? phoneNumber,
            [FromQuery(Name = "email")] string? email,
            [FromQuery(Name = "claim-no")] string? claimNo,
            [FromQuery(Name = "from-date")] DateTime fromDate,
            [FromQuery(Name = "to-date")] DateTime toDate,
            [FromQuery(Name = "status")] int status)
        {
            try
            {
                var fromDateStart = fromDate.Date;
                var toDateEnd = toDate.Date.AddDays(1).AddMilliseconds(-1);
                var warrantyClaims = await _repo.ProcedureToList<WarrantyClaimDTO>("spGetWarrantyClaims",
                    ["p_PhoneNumber", "p_Email", "p_ClaimNo", "p_FromDate", "p_ToDate", "p_Status"],
                    [phoneNumber, email, claimNo, fromDateStart, toDateEnd, status]);

                return Ok(ApiResponseFactory.Success(warrantyClaims));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve warranty claims."));
            }
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetAllDataAsAdmin()
        {
            try
            {
                var warrantyClaims = await _repo.ProcedureToList<WarrantyClaimDTO>("spGetWarrantyClaimsDropdownData",
                   [],
                   []);
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
                    return NotFound(ApiResponseFactory.Fail(null, "Warranty claims not found."));
                var product = await _repo.GetById<Product>(warrantyClaim.ProductId ?? 0);
                var dto = new WarrantyClaimDTO(warrantyClaim)
                {
                    ProductName = product!.Name
                };
                return Ok(ApiResponseFactory.Success(dto));
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
                if (warrantyClaim.ProductId == null)
                    return BadRequest(ApiResponseFactory.Fail(null, "ProductId is required."));
                warrantyClaim.Id = 0;
                warrantyClaim.CreatedDate = DateTime.Now;
                warrantyClaim.CreatedBy = warrantyClaim.CustomerName;
                warrantyClaim.Status = 1;
                var createdWarrantyClaim = await _repo.Insert(warrantyClaim);

                var customer = new Customer
                {
                    WarrantyClaimId = createdWarrantyClaim.Id,
                    CustomerName = warrantyClaim.CustomerName,
                    CustomerPhoneNumber = warrantyClaim.CustomerPhoneNumber,
                    CustomerEmail = warrantyClaim.CustomerEmail,
                    CustomerAddress = warrantyClaim.CustomerAddress,
                    CreatedBy = warrantyClaim.CustomerName,
                    CreatedDate = DateTime.Now
                };

                await _repo.Insert(customer);
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