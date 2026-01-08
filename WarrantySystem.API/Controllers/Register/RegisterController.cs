using Microsoft.AspNetCore.Mvc;
using WarrantySystem.Model.Entities;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;
using static WarrantySystem.API.Controllers.WorkOrders.WorkOrderController;

namespace WarrantySystem.API.Controllers.Register
{
    [Route("api/[controller]")]
    [ApiController]

    public class RegisterController : ControllerBase
    {
        private IGenericRepo _repo;

        public RegisterController(IGenericRepo repo)
        {
            _repo = repo;
        }
        [HttpGet]
        public async Task<IActionResult> GetListUsers([FromQuery] int? Status)
        {
            try
            {
                var work = await _repo.ProcedureToList<dynamic>(
                    "spGetUser",
                    new string[] { "@Status" },
                    new object[] { Status }
                );

                return Ok(ApiResponseFactory.Success(work, "Lấy dữ liệu thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveEmployee([FromBody] User employee)
        {
            try
            {
                string password = MaHoaMD5.EncryptPassword(employee.PasswordHash ?? "");
                employee.PasswordHash = password;
                if (employee.Id <= 0)
                { 
                    
                    await _repo.Insert(employee); }

                else await _repo.Update(employee);

                return Ok(ApiResponseFactory.Success(employee, ""));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }

        }

    }
}
