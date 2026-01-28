using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;

namespace WarrantySystem.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private IGenericRepo _repo;

        public CustomerController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet("customer")]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var work = await _repo.ProcedureToList<dynamic>(
                    "spGetCustomer",
                    Array.Empty<string>(),
                    Array.Empty<object>()
                );

                return Ok(ApiResponseFactory.Success(work, "Lấy dữ liệu thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }
    }
}