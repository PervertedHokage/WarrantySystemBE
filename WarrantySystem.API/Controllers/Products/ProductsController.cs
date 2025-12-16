using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using WarrantySystem.Model.Entities;
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

        [HttpPost("save-product")]
        public async Task<IActionResult> SaveData([FromBody] Product products)
        {
            try
            {
                // Check trùng mã sản phẩm
                var existed = (await _repo.FindByExpression<Product>(x => x.Code == products.Code
                     && x.Id != products.Id && x.IsDeleted == false));

                if (existed.Any())
                {
                    return Ok(new
                    {
                        status = 0,
                        message = "Mã sản phẩm đã tồn tại."
                    });
                }

                if (products.Id <= 0)
                {
                    await _repo.Insert(products);
                }
                else
                {
                    await _repo.Update(products);
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
