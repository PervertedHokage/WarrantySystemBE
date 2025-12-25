using Microsoft.AspNetCore.Mvc;
using WarrantySystem.Model.DTO;
using WarrantySystem.Model.Entities;
using WarrantySystem.Model.Param;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;

namespace WarrantySystem.API.Controllers.SalesOrder
{
    [Route("api/[controller]")]
    [ApiController]

    public class SaleOrderController : ControllerBase
    {
        private IGenericRepo _repo;

        public SaleOrderController(IGenericRepo repo)
        {
            _repo = repo;
        }
        public class AllProduct
        {
            public int ProductId { get; set; }

        }

        [HttpPost]
        public async Task<IActionResult> GetListOrder([FromBody] OrderSaleParam request)
        {
            try
            {
                var order = await _repo.ProcedureToList<dynamic>("spGetOrder",
                    new string[] { "@OrderId" },
                    new object[] { request.OrderId });
                return Ok(ApiResponseFactory.Success(order, "Lấy dữ liệu thành công"));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }


        [HttpPost("product")]
        public async Task<IActionResult> GetListProduct([FromBody] AllProduct request)
        {
            try
            {
                var product = await _repo.ProcedureToList<dynamic>("spGetProduct",
                    new string[] { "@ProductId" },
                    new object[] { request.ProductId });
                return Ok(ApiResponseFactory.Success(product, "Lấy dữ liệu thành công"));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }

        [HttpPost("save-data-sale-order")]
        public async Task<IActionResult> SaveData([FromBody] SaleOrderDTO dto)
        {
            try
            {
                var claims = User.Claims.ToDictionary(x => x.Type, x => x.Value);
                CurrentUser currentUser = ObjectMapper.GetCurrentUser(claims);

                if (dto == null || dto.Order == null)
                {
                    return BadRequest(new { status = 0, message = "Dữ liệu không hợp lệ" });
                }

                int OrderID = 0;
                int OrderDetailID = 0;

                // Master
                if (dto.Order.Id <= 0)

                {
                    await _repo.Insert(dto.Order);
                    OrderID = dto.Order.Id;
                }
                else
                {
                    await _repo.Update(dto.Order);
                    OrderID = dto.Order.Id;
                }

                foreach (var itemDetail in dto.SaleOrderDetailDTO)
                {
                    var groupDTO = itemDetail.OrderDetails;
                    groupDTO.OrderId = OrderID;
                    OrderDetailID = groupDTO.Id;
                    // Lưu group
                    if (groupDTO.Id <= 0)
                    {
                        await _repo.Insert(groupDTO);
                        OrderDetailID = groupDTO.Id;
                    }
                    else
                    {
                        var group = await _repo.GetById<OrderDetail>(itemDetail.OrderDetails.Id);
                        if (group == null) continue; // an toàn
                        await _repo.Update(groupDTO);
                        OrderDetailID = group.Id;
                    }

                    // Lưu chi tiết của group này
                    foreach (var itemDetailInfo in itemDetail.OrderDetailInfo)
                    {
                        itemDetailInfo.OrderDetailId = OrderDetailID;


                        var existing = (await _repo.FindByExpression<OrderDetailInfo>(x => x.OrderDetailId == OrderDetailID && x.Id == itemDetailInfo.Id));

                        if (existing == null || itemDetailInfo.Id <= 0)
                        {
                            await _repo.Insert(itemDetailInfo);
                        }
                        else
                        {
                            // Update detail
                            var existingDetail = await _repo.GetById<OrderDetailInfo>(itemDetailInfo.Id);
                            if (existingDetail != null && existingDetail.OrderDetailId == OrderDetailID)
                            {
                                await _repo.Update(itemDetailInfo);
                            }
                        }
                    }

                    // order detail
                    //if (dto.OrderDetails != null && dto.OrderDetails.Any())
                    //{
                    //    foreach (var item in dto.OrderDetails)
                    //    {
                    //        item.OrderId = OrderID;
                    //        var existing = (await _repo.FindByExpression<OrderDetail>(x => x.OrderId == OrderID && x.Id == item.Id));

                    //        if (existing == null || item.Id <= 0)
                    //        {
                    //            await _repo.Insert(item);
                    //            OrderDetailID = item.Id;
                    //        }

                    //        else
                    //        {
                    //            await _repo.Update(item);
                    //            OrderDetailID = item.Id;
                    //        }

                    //    }
                    //}

                    //// order-detail-info
                    //if (dto.SaleOrderDetailDTO. != null && dto.OrderDetailInfo.Any())
                    //{
                    //    foreach (var item in dto.OrderDetailInfo)
                    //    {
                    //        item.OrderDetailId = OrderDetailID;
                    //        //var existing = (await _repo.FindByExpression<OrderDetailInfo>(x => x.OrderDetailId == OrderDetailID && x.Id == item.Id));

                    //        if (item.Id <= 0)
                    //        {
                    //            await _repo.Insert(item);
                    //        }

                    //        else
                    //            await _repo.Update(item);
                    //    }
                    //}
                    if (dto.DeletedOrder?.Count > 0)
                    {
                        foreach (var item in dto.DeletedOrder)
                        {
                            var issue = await _repo.GetById<OrderDetail>(item);
                            if (issue == null) continue;
                            issue.IsDeleted = true;
                            await _repo.Update(issue);
                        }
                    }
                }
                    return Ok(new
                    {
                        status = 1,
                        message = "Lưu thành công",
                        id = OrderID,
                    });
                
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));

            }
        }
    }
}
