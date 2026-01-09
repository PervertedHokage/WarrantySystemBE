using Microsoft.AspNetCore.Mvc;
using WarrantySystem.Model.Entities;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;

namespace WarrantySystem.API.Controllers
{
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

        [HttpPost] 
        public async Task<IActionResult> SaveData([FromBody] Customer customer) 
        {
            try
            {
                var existed = await _repo.FindByExpression<Customer>(x =>
                    x.IsDeleted == false &&
                    x.Id != customer.Id &&
                    (x.CustomerPhoneNumber == customer.CustomerPhoneNumber
                    || x.CustomerEmail == customer.CustomerEmail)
                );

                if (existed.Any(x => x.CustomerPhoneNumber == customer.CustomerPhoneNumber))
                {
                    return BadRequest(ApiResponseFactory.Fail(null, "Số điện thoại đã tồn tại."));
                }

                if (existed.Any(x => x.CustomerEmail == customer.CustomerEmail))
                {
                    return BadRequest(ApiResponseFactory.Fail(null, "Email đã tồn tại."));
                }

                if (customer.Id <=0)
                {
                    await _repo.Insert(customer);
                }
                else
                {
                    customer.IsDeleted = false;
                    await _repo.Update(customer);
                }
                    return Ok(ApiResponseFactory.Success(customer, "Lưu dữ liệu thành công"));

            }
            catch(Exception ex)
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

                    var unit = await _repo.GetById<Customer>(item);
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
