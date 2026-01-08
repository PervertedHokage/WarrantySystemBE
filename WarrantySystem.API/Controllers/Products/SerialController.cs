using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using WarrantySystem.Model.Entities;
using WarrantySystem.Model.Param;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;

namespace WarrantySystem.API.Controllers.Products
{
    [Route("api/[controller]")]
    [ApiController]
    public class SerialController : ControllerBase
    {
        private IGenericRepo _repo;

        public SerialController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetListIssues([FromQuery] int ProductID)
        {
            try
            {
                var serial = await _repo.ProcedureToList<dynamic>("spGetSerial",
                    new string[] { "@ProductId" },
                    new object[] { ProductID });
                return Ok(ApiResponseFactory.Success(serial, "Lấy dữ liệu thành công"));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }


        [HttpPost("save-serial")]
        public async Task<IActionResult> SaveData([FromBody] Serial serial)
        {
            try
            {
                // Check trùng mã kho
    
                var model = (await _repo.FindByExpression<Serial>(x => x.ProductSerial == serial.ProductSerial
                           && x.Id != serial.Id)); // loại trừ chính nó khi update
                if (model.Any())
                {
                    return Ok(new
                    {
                        status = 0,
                        message = "Mã serial đã tồn tại."
                    });
                }

                if (serial.Id <= 0)
                {
                    serial.IsDeleted = false;
                    await _repo.Insert(serial);
                }
                else
                {
                    await _repo.Update(serial);
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

                    var serial = await _repo.GetById<Serial>(item);
                    serial.IsDeleted = true;
                    await _repo.Update(serial);

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
