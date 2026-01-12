using Microsoft.AspNetCore.Mvc;
using WarrantySystem.Model.DTO;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;

namespace WarrantySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SerialCheckController : Controller
    {
        private readonly IGenericRepo _repo;

        public SerialCheckController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> CheckStatus([FromQuery(Name = "serial")] string? serial)
        {
            try
            {
                serial ??= "";
                var data = await _repo.ProcedureToList<CheckStatusBySerialDTO>("spCheckStatusBySerial",
                    ["p_Serial"],
                    [serial]);
                if (data.Count == 0)
                {
                    return BadRequest(ApiResponseFactory.Fail(null, "Không tìm thấy dữ liệu phù hợp"));
                }
                return Ok(ApiResponseFactory.Success(data[0]));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve warranty claims."));
            }
        }
    }
}
