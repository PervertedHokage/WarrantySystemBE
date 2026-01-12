using ClosedXML.Excel;
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

        [HttpGet]
        public async Task<IActionResult> GetListOrder([FromQuery] OrderSaleParam request)
        {
            try
            {
                var order = await _repo.ProcedureToList<SaleOrderFullDTO>("spGetOrder",
                    new string[] { "@OrderId", "@FromDateStart", "@ToDateStart" },
                    new object[] { request.OrderId, request.FromDateStart, request.ToDateStart });
                return Ok(ApiResponseFactory.Success(order, "Lấy dữ liệu thành công"));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }

        [HttpPost]
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
                            var DetailInfo = await _repo.Insert(itemDetailInfo);
                            var serial = new Serial
                            {
                                ProductSerial = DetailInfo.ProductSerial,
                                OrderDetailInfoId = DetailInfo.Id

                            };
                            await _repo.Insert(serial);
                        }
                        else
                        {
                            // Update detail
                            var existingDetail = await _repo.GetById<OrderDetailInfo>(itemDetailInfo.Id);
                            if (existingDetail != null && existingDetail.OrderDetailId == OrderDetailID)
                            {
                                await _repo.Update(itemDetailInfo);

                                var existingSerial = (await _repo.FindByExpression<Serial>(
                                    x => x.OrderDetailInfoId == itemDetailInfo.Id
                                 )).FirstOrDefault();

                                if (existingSerial != null)
                                {
                                    existingSerial.ProductSerial = itemDetailInfo.ProductSerial;
                                    await _repo.Update(existingSerial);
                                }
                                else
                                {
                                    // Trường hợp thiếu serial → insert bù
                                    var newSerial = new Serial
                                    {
                                        OrderDetailInfoId = itemDetailInfo.Id,
                                        ProductSerial = itemDetailInfo.ProductSerial
                                    };
                                    await _repo.Insert(newSerial);
                                }
                            }
                        }
                    }

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

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] List<int> ids)
        {
            try
            {
                if (ids == null || ids.Count == 0)
                    return BadRequest(ApiResponseFactory.Fail(null, "Vui lòng chọn đơn hàng để xóa"));

                foreach (var detailId in ids)
                {

                    var orderDetail = await _repo.GetById<OrderDetail>(detailId);
                    if (orderDetail == null) continue;

                    orderDetail.IsDeleted = true;
                    await _repo.Update(orderDetail);

                    var hasAnyDetail = await _repo.FindByExpression<OrderDetail>(
                        x => x.OrderId == orderDetail.OrderId && !x.IsDeleted
                    );

                    if (!hasAnyDetail.Any())
                    {
                        var order = await _repo.GetById<Order>(orderDetail.OrderId.Value);
                        if (order != null)
                        {
                            order.IsDeleted = true;
                            await _repo.Update(order);
                        }
                    }
                }

                return Ok(ApiResponseFactory.Success(ids, "Xóa thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }

        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportOrderExcel([FromQuery] OrderSaleParam request)
        {
            try
            {
                var orders = await _repo.ProcedureToList<SaleOrderFullDTO>("spGetOrder",
                    new string[] { "@OrderId", "@FromDateStart", "@ToDateStart" },
                    new object[] { request.OrderId, request.FromDateStart, request.ToDateStart });

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Orders");

                worksheet.Cell(1, 1).Value = "Code";
                worksheet.Cell(1, 2).Value = "Customer Name";
                worksheet.Cell(1, 3).Value = "Phone";
                worksheet.Cell(1, 4).Value = "Product";
                worksheet.Cell(1, 5).Value = "Imei1";
                worksheet.Cell(1, 6).Value = "Imei2";
                worksheet.Cell(1, 7).Value = "Quantity";
                worksheet.Cell(1, 8).Value = "Price";
                worksheet.Cell(1, 9).Value = "Date Start";
                worksheet.Cell(1, 10).Value = "Date End";

                int row = 2;
                foreach (var item in orders)
                {
                    worksheet.Cell(row, 1).Value = item.Code;
                    worksheet.Cell(row, 2).Value = item.CustomerName;
                    worksheet.Cell(row, 3).Value = item.CustomerPhoneNumber;
                    worksheet.Cell(row, 4).Value = item.ProductName;
                    worksheet.Cell(row, 5).Value = item.Imei1;
                    worksheet.Cell(row, 6).Value = item.Imei2;
                    worksheet.Cell(row, 7).Value = item.Quantity;
                    worksheet.Cell(row, 8).Value = item.Price;
                    worksheet.Cell(row, 9).Value = item.DateStart;
                    worksheet.Cell(row, 10).Value = item.DateEnd;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"SaleOrders_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }
    }
}
