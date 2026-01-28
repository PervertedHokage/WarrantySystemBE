using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarrantySystem.API.Middlewares;
using WarrantySystem.Model.DTO;
using WarrantySystem.Model.Entities;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;

namespace WarrantySystem.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class QuotationController : Controller
    {
        private readonly IConfiguration _configuration;
        private IGenericRepo _repo;

        public QuotationController(IConfiguration configuration, IGenericRepo repo)
        {
            _repo = repo;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var quotations = await _repo.GetAll<Quotation>();
                return Ok(ApiResponseFactory.Success(quotations));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve quotations."));
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetDataAsAdmin(
        [FromQuery(Name = "from-date")] DateTime fromDate,
        [FromQuery(Name = "to-date")] DateTime toDate,
        [FromQuery(Name = "claim-no")] string? claimNo)
        {
            try
            {
                var fromDateStart = fromDate.Date;
                var toDateEnd = toDate.Date.AddDays(1).AddMilliseconds(-1);
                var quotations = await _repo.ProcedureToList<QuotationDTO>("spGetQuotations",
                    ["p_FromDate", "p_ToDate", "p_ClaimNo"],
                    [fromDateStart, toDateEnd, claimNo]);

                return Ok(ApiResponseFactory.Success(quotations));
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
                var quotation = await _repo.GetById<Quotation>(id);
                if (quotation == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "Quotation not found."));
                }
                return Ok(ApiResponseFactory.Success(quotation));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve quotation."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Quotation quotation)
        {
            try
            {
                var user = HttpContext.Session.GetObject<CurrentUser>(_configuration.GetValue<string>("SessionKey"));
                quotation.CreatedDate = DateTime.Now;
                quotation.CreatedBy = user.LoginName;
                var createdQuotation = await _repo.Insert(quotation);
                return Ok(ApiResponseFactory.Success(createdQuotation, "Quotation created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to create quotation."));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Quotation quotation)
        {
            try
            {
                var user = HttpContext.Session.GetObject<CurrentUser>(_configuration.GetValue<string>("SessionKey"));
                quotation.UpdatedDate = DateTime.Now;
                quotation.UpdatedBy = user.LoginName;
                var existingQuotation = await _repo.GetById<Quotation>(id);
                if (existingQuotation == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "Quotation not found."));
                }
                quotation.Id = id; // Ensure the ID is set for update
                var updatedQuotation = await _repo.Update(quotation);
                return Ok(ApiResponseFactory.Success(updatedQuotation, "Quotation updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to update quotation."));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingQuotation = await _repo.GetById<Quotation>(id);
                if (existingQuotation == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "Quotation not found."));
                }
                await _repo.DeleteById<Quotation>(id);
                return Ok(ApiResponseFactory.Success(null, "Quotation deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to delete quotation."));
            }
        }

        [HttpGet("details/{quotationId}")]
        public async Task<IActionResult> GetDetails(int quotationId)
        {
            try
            {
                var details = await _repo.ProcedureToList<QuotationDetailDTO>("spGetQuotationDetails",
                    ["p_QuotationId"], [quotationId]);
                return Ok(ApiResponseFactory.Success(details));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to delete quotation."));
            }
        }

        [HttpPost("details")]
        public async Task<IActionResult> UpdateDetails([FromBody] QuotationDetail[] data)
        {
            try
            {
                foreach (var d in data)
                {
                    if (d.Id == 0) await _repo.Insert<QuotationDetail>(d);
                    else await _repo.Update<QuotationDetail>(d);
                }
                return Ok(ApiResponseFactory.Success(data));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to update quotation details."));
            }
        }
    }
}