using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using WarrantySystem.Model.DTO;
using WarrantySystem.Model.Entities;
using WarrantySystem.Model.Param;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;

namespace WarrantySystem.API.Controllers.Products
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private IGenericRepo _repo;

        public ProductsController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet("get-products")]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = (await _repo.FindByExpression<Product>(x => x.IsDeleted == false));
                return Ok(new
                {
                    status = 1,
                    data = products

                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));

            }
        }

        [HttpGet("get-spare-parts-group")]
        public async Task<IActionResult> GetSparePartGroups()
        {
            try
            {
                var data = (await _repo.FindByExpression<SparePartsGroup>(x => x.IsDeleted == false));
                return Ok(new
                {
                    status = 1,
                    data = data

                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));

            }
        }

        [HttpPost("get-spare-parts")]
        public async Task<IActionResult> GetListIssues([FromBody] SparePartsParam request)
        {
            try
            {
                var sparePart = await _repo.ProcedureToList<dynamic>("spGetSpareParts",
                    new string[] { "@ProductId" },
                    new object[] { request.ProductId });
                return Ok(new
                {
                    status = 1,
                    data = new
                    {
                        asset = sparePart
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
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


        [HttpPost("save-data-product")]
        public async Task<IActionResult> SaveDataProduct([FromBody] sparePartGroupDTO dto)
        {
            try
            {
                var claims = User.Claims.ToDictionary(x => x.Type, x => x.Value);
                CurrentUser currentUser = ObjectMapper.GetCurrentUser(claims);

                if (dto == null || dto.Product == null)
                {
                    return BadRequest(new { status = 0, message = "Dữ liệu không hợp lệ" });
                }

                int ProductID = 0;

                // Master
                if (dto.Product.Id <= 0)

                {
                    await _repo.Insert(dto.Product);
                    ProductID = dto.Product.Id;
                }
                else
                {
                    await _repo.Update(dto.Product);
                    ProductID = dto.Product.Id;
                }

                // Xử lý xóa chi tiết trong group này (nếu edit)
                if (dto.DeletedSparePartGroup.Count > 0)
                {
                    foreach (var item in dto.DeletedSparePartGroup)
                    {
                        var deletedSparePartGroup = await _repo.GetById<SparePartsGroup>(item);
                        if (deletedSparePartGroup != null)
                        {
                            deletedSparePartGroup.IsDeleted = true;
                            await _repo.Update(deletedSparePartGroup);
                        }
                    }
                }

                foreach (var groupWithDetails in dto.SparePartsGroups)
                {
                    var groupDTO = groupWithDetails.SparePartsGroup;
                    int groupId;
                    groupDTO.ProductId = ProductID;
                    // Lưu group1
                    if (groupDTO.Id <= 0)
                    {
                        await _repo.Insert(groupDTO);
                        groupId = groupDTO.Id;
                    }
                    else
                    {
                       var group = await _repo.GetById<SparePartsGroup>(groupWithDetails.SparePartsGroup.Id);
                        if (group == null) continue; // an toàn
                        await _repo.Update(groupDTO);
                        groupId = group.Id;
                    }

                    // Lưu chi tiết của group này
                    foreach (var detailDto in groupWithDetails.SparePart)
                    {
                        detailDto.ProductId = ProductID;
                        detailDto.SparePartGroupId = groupId;
                        var existing = (await _repo.FindByExpression<SparePart>(x => x.Id == detailDto.Id));

                        if (existing == null || detailDto.Id <= 0)
                        {
                            await _repo.Insert(detailDto);
                        }
                        else
                        {
                            // Update detail
                            var existingDetail = await _repo.GetById<SparePart>(detailDto.Id);
                            if (existingDetail != null && existingDetail.SparePartGroupId == groupId)
                            {
                                await _repo.Update(detailDto);
                            }
                        }
                    }
                    // Xử lý xóa chi tiết trong group này (nếu edit)
                    if (groupWithDetails.DeletedSparePart != null && groupWithDetails.DeletedSparePart.Any())
                    {
                        foreach (var item in groupWithDetails.DeletedSparePart)
                        {
                            var deletedSparePart = await _repo.GetById<SparePart>(item);
                            if (deletedSparePart != null)
                            {
                                deletedSparePart.IsDeleted = true;
                                await _repo.Update(deletedSparePart);
                            }

                        }
                    }
                }

                return Ok(new
                {
                    status = 1,
                    message = "Lưu thành công",
                    id = ProductID,
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

    }
}
