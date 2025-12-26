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

        [HttpGet]
        public async Task<IActionResult> GetUnits()
        {
            try
            {
                var units = (await _repo.FindByExpression<Unit>(x => x.IsDeleted == false));
                return Ok(ApiResponseFactory.Success(units, "Lấy dữ liệu thành công"));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));

            }
        }

        [HttpPost]
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

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            try
            {
                var claims = User.Claims.ToDictionary(x => x.Type, x => x.Value);
                var currentUser = ObjectMapper.GetCurrentUser(claims);
                if (ids == null || ids.Count == 0)
                    return BadRequest(ApiResponseFactory.Fail(null, "Vui lòng chọn yccv để xóa"));
                foreach (var item in ids)
                {

                    var unit = await _repo.GetById<Unit>(item);
                    unit.IsDeleted = true;
                    await _repo.Update(unit);

                }
                return Ok(ApiResponseFactory.Success(ids, "Xóa thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }

    }
}
