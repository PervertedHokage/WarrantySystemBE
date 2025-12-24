using Microsoft.AspNetCore.Mvc;
using WarrantySystem.Model.DTO;
using WarrantySystem.Model.Entities;
using WarrantySystem.Model.Param;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;

namespace WarrantySystem.API.Controllers.WorkOrders
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkOrderController : ControllerBase
    {
        private IGenericRepo _repo;
        private CurrentUser _currentUser;

        public WorkOrderController(IGenericRepo repo, CurrentUser currentUser)
        {
            _repo = repo;
            _currentUser = currentUser;
        }

        public class WorkOrderParam
        {
            public int WorkOrderID { get; set; }

        }
        public class EmployeeParam
        {
            public int Status { get; set; }

        }

        [HttpPost("get-work-order")]
        public async Task<IActionResult> GetListWorkOrders([FromBody] WorkOrderParam request)
        {
            try
            {
                var work = await _repo.ProcedureToList<dynamic>("spGetWorkOrder",
                    new string[] { "@WorkOrderID" },
                    new object[] { request.WorkOrderID });

                return Ok(new
                {
                    status = 1,
                    data = new
                    {
                        asset = work
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }

        [HttpPost("get-work-order-detail")]
        public async Task<IActionResult> GetListWorkOrderDetails([FromBody] WorkOrderParam request)
        {
            try
            {
                var workdetail = await _repo.ProcedureToList<dynamic>("spGetWorkOrderDetail",
                    new string[] { "@WorkOrderID" },
                    new object[] { request.WorkOrderID });

                return Ok(new
                {
                    status = 1,
                    data = new
                    {
                        asset = workdetail
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }

        [HttpPost("get-employees")]
        public async Task<IActionResult> GetListEmployees([FromBody] EmployeeParam request)
        {
            try
            {
                var employees = await _repo.ProcedureToList<dynamic>("spGetUser",
                    new string[] { "@Status" },
                    new object[] { request.Status });
                return Ok(new
                {
                    status = 1,
                    data = new
                    {
                        asset = employees
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }

        [HttpGet("get-status")]
        public async Task<IActionResult> GetStatus()
        {
            try
            {
                var status = (await _repo.FindByExpression<WorkOrderStatus>(x => x.IsDeleted == false));
                return Ok(new
                {
                    status = 1,
                    data = status

                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));

            }
        }

        [HttpGet("get-quotation")]
        public async Task<IActionResult> GetQuotations()
        {
            try
            {
                var quotation = (await _repo.FindByExpression<Quotation>(x => x.IsDeleted == false));
                return Ok(new
                {
                    status = 1,
                    data = quotation

                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));

            }
        }

        [HttpGet("get-warranty-claim")]
        public async Task<IActionResult> GetWarrantyClaims()
        {
            try
            {
                var claim = (await _repo.FindByExpression<WarrantyClaim>(x => x.IsDeleted == false));
                return Ok(new
                {
                    status = 1,
                    data = claim

                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));

            }
        }


        [HttpPost("save-data-work-order")]
        public async Task<IActionResult> SaveData([FromBody] WorkOrderDTO dto)
        {
            try
            {
                var claims = User.Claims.ToDictionary(x => x.Type, x => x.Value);
                CurrentUser currentUser = ObjectMapper.GetCurrentUser(claims);

                if (dto == null || dto.WorkOrder == null)
                {
                    return BadRequest(new { status = 0, message = "Dữ liệu không hợp lệ" });
                }

                int WorkOrderID = 0;

                // Master
                if (dto.WorkOrder.Id <= 0)

                {
                    await _repo.Insert(dto.WorkOrder);
                    WorkOrderID = dto.WorkOrder.Id;
                }
                else
                {
                    await _repo.Update(dto.WorkOrder);
                    WorkOrderID = dto.WorkOrder.Id;
                }

                // SpareSpart
                if (dto.WorkOrderSpareParts != null && dto.WorkOrderSpareParts.Any())
                {
                    foreach (var item in dto.WorkOrderSpareParts)
                    {
                        item.WorkOrderId = WorkOrderID;
                        var existing = (await _repo.FindByExpression<WorkOrderSparePart>(x => x.WorkOrderId == WorkOrderID && x.Id == item.Id));

                        if (existing == null || item.Id <= 0)
                        {
                            await _repo.Insert(item);
                        }

                        else
                            await _repo.Update(item);
                    }
                }
                if (dto.DeletedSpareSpart?.Count > 0)
                {
                    foreach (var item in dto.DeletedSpareSpart)
                    {
                        var sparePart = await _repo.GetById<WorkOrderSparePart>(item);
                        if (sparePart == null) continue;
                        sparePart.IsDeleted = true;
                        await _repo.Update(sparePart);
                    }
                }
                return Ok(new
                {
                    status = 1,
                    message = "Lưu thành công",
                    id = WorkOrderID,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = 0,
                    message = ex.Message,
                    error = ex.ToString()
                });
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

                    var work = await _repo.GetById<WorkOrder>(item);
                    //if (work.UsersId != currentUser.EmployeeID)
                    //{
                    //    return BadRequest(ApiResponseFactory.Fail(null, $"Bạn không thể xóa phiếu yêu cầu công việc của người khác"));
                    //}
                    work.IsDeleted = true;
                    await _repo.Update(work);

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
