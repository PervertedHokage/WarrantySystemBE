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
        public class SerialParam
        {
            public int ProductId { get; set; }

        }

        [HttpPost("get-serial")]
        public async Task<IActionResult> GetListIssues([FromBody] SerialParam request)
        {
            try
            {
                var serial = await _repo.ProcedureToList<dynamic>("spGetSerial",
                    new string[] { "@ProductId" },
                    new object[] { request.ProductId });
                return Ok(new
                {
                    status = 1,
                    data = new
                    {
                        asset = serial
                    }
                });
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


    }
}
