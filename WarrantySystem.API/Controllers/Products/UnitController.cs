using Microsoft.AspNetCore.Mvc;
using WarrantySystem.Model.Entities;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;

namespace WarrantySystem.API.Controllers.Products
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitController : ControllerBase
    {
        private IGenericRepo _repo;

        public UnitController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet("get-unit")]
        public async Task<IActionResult> GetUnits()
        {
            try
            {
                var units = (await _repo.FindByExpression<Unit>(x => x.IsDeleted == false));
                return Ok(new
                {
                    status = 1,
                    data = units

                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));

            }
        }

        [HttpPost("save-data-unit")]
        public async Task<IActionResult> SaveData([FromBody] Unit units)
        {
            try
            {
                // Check trùng mã sản phẩm
                var existed = (await _repo.FindByExpression<Unit>(x => x.Code == units.Code
                     && x.Id != units.Id && x.IsDeleted == false));

                if (existed.Any())
                {
                    return Ok(new
                    {
                        status = 0,
                        message = "Mã đơn vị đã tồn tại."
                    });
                }

                if (units.Id <= 0)
                {
                    await _repo.Insert(units);
                }
                else
                {
                    await _repo.Update(units);
                }

                return Ok(new
                {
                    status = 1,
                    message = "Lưu thành công."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }

    }
}
