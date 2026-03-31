using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarrantySystem.Model.DTO;
using WarrantySystem.Model.Entities;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;
using System.IO;

namespace WarrantySystem.API.Controllers.WarrantyClaims
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WarrantyClaimController : Controller
    {
        private readonly IGenericRepo _repo;

        public WarrantyClaimController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetData([FromQuery(Name = "phone-number")] string? phoneNumber,
            [FromQuery(Name = "email")] string? email, [FromQuery(Name = "claim-no")] string? claimNo)
        {
            try
            {
                var warrantyClaims = await _repo.ProcedureToList<WarrantyClaimDTO>("spGetWarrantyClaims",
                    ["p_PhoneNumber", "p_Email", "p_ClaimNo", "p_FromDate", "p_ToDate", "p_Status"],
                    [phoneNumber, email, claimNo, null, null, 0]);

                return Ok(ApiResponseFactory.Success(warrantyClaims));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve warranty claims."));
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetDataAsAdmin(
            [FromQuery(Name = "phone-number")] string? phoneNumber,
            [FromQuery(Name = "email")] string? email,
            [FromQuery(Name = "claim-no")] string? claimNo,
            [FromQuery(Name = "from-date")] DateTime fromDate,
            [FromQuery(Name = "to-date")] DateTime toDate,
            [FromQuery(Name = "status")] int status)
        {
            try
            {
                var fromDateStart = fromDate.Date;
                var toDateEnd = toDate.Date.AddDays(1).AddMilliseconds(-1);
                var warrantyClaims = await _repo.ProcedureToList<WarrantyClaimDTO>("spGetWarrantyClaims",
                    ["p_PhoneNumber", "p_Email", "p_ClaimNo", "p_FromDate", "p_ToDate", "p_Status"],
                    [phoneNumber, email, claimNo, fromDateStart, toDateEnd, status]);

                return Ok(ApiResponseFactory.Success(warrantyClaims));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve warranty claims."));
            }
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetAllDataAsAdmin()
        {
            try
            {
                var warrantyClaims = await _repo.ProcedureToList<WarrantyClaimDTO>("spGetWarrantyClaimsDropdownData",
                   [],
                   []);
                return Ok(ApiResponseFactory.Success(warrantyClaims));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve warranty claims."));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var warrantyClaim = await _repo.GetById<WarrantyClaim>(id);
                if (warrantyClaim == null)
                    return NotFound(ApiResponseFactory.Fail(null, "Warranty claims not found."));
                var product = await _repo.GetById<Product>(warrantyClaim.ProductId ?? 0);
                var attachments = await _repo.FindByExpression<WarrantyClaimAttachment>(a => a.WarrantyClaimId == id);
                var dto = new WarrantyClaimDTO(warrantyClaim)
                {
                    ProductName = product?.Name ?? "",
                    Attachments = attachments
                };
                return Ok(ApiResponseFactory.Success(dto));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve warranty claims."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WarrantyClaim warrantyClaim)
        {
            try
            {
                if (warrantyClaim.ProductId == null)
                    return BadRequest(ApiResponseFactory.Fail(null, "ProductId is required."));
                warrantyClaim.Id = 0;
                warrantyClaim.CreatedDate = DateTime.Now;
                warrantyClaim.CreatedBy = warrantyClaim.CustomerName;
                warrantyClaim.Status = 1;
                var createdWarrantyClaim = await _repo.Insert(warrantyClaim);

                var customer = new Customer
                {
                    WarrantyClaimId = createdWarrantyClaim.Id,
                    CustomerName = warrantyClaim.CustomerName,
                    CustomerPhoneNumber = warrantyClaim.CustomerPhoneNumber,
                    CustomerEmail = warrantyClaim.CustomerEmail,
                    CustomerAddress = warrantyClaim.CustomerAddress,
                    CreatedBy = warrantyClaim.CustomerName,
                    CreatedDate = DateTime.Now
                };

                await _repo.Insert(customer);
                return Ok(ApiResponseFactory.Success(createdWarrantyClaim, "Warranty claim created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to create warranty claim."));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WarrantyClaim warrantyClaim)
        {
            try
            {
                var existingWarrantyClaim = await _repo.GetById<WarrantyClaim>(id);
                if (existingWarrantyClaim == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "Warranty claim not found."));
                }

                warrantyClaim.Id = id;
                var updatedWarrantyClaim = await _repo.Update(warrantyClaim);

                return Ok(ApiResponseFactory.Success(updatedWarrantyClaim, "Warranty claim updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to update warranty claim."));
            }
        }

        [HttpPost("{id}/upload")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFiles(int id, [FromForm] List<IFormFile> files)
        {
            try
            {
                var warrantyClaim = await _repo.GetById<WarrantyClaim>(id);
                if (warrantyClaim == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "Warranty claim not found."));
                }

                if (files == null || files.Count == 0)
                {
                    return BadRequest(ApiResponseFactory.Fail(null, "No files uploaded."));
                }

                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "warrantyclaims", id.ToString());
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var createdAttachments = new List<WarrantyClaimAttachment>();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var attachment = new WarrantyClaimAttachment
                        {
                            WarrantyClaimId = id,
                            FileName = file.FileName,
                            FilePath = $"/uploads/warrantyclaims/{id}/{fileName}",
                            FileSize = file.Length,
                            FileType = file.ContentType,
                            CreatedDate = DateTime.Now,
                            CreatedBy = warrantyClaim.CustomerName // Or use authenticated user if available
                        };

                        var created = await _repo.Insert(attachment);
                        createdAttachments.Add(created);
                    }
                }

                return Ok(ApiResponseFactory.Success(createdAttachments, "Files uploaded successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to upload files."));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingWarrantyClaim = await _repo.GetById<WarrantyClaim>(id);
                if (existingWarrantyClaim == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "Warranty claim not found."));
                }

                await _repo.DeleteById<WarrantyClaim>(id);

                return Ok(ApiResponseFactory.Success(null, "Warranty claim deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to delete warranty claim."));
            }
        }

        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportExcel(
            [FromQuery(Name = "phone-number")] string? phoneNumber,
            [FromQuery(Name = "email")] string? email,
            [FromQuery(Name = "claim-no")] string? claimNo,
            [FromQuery(Name = "from-date")] DateTime fromDate,
            [FromQuery(Name = "to-date")] DateTime toDate,
            [FromQuery(Name = "status")] int status)
        {
            try
            {
                var fromDateStart = fromDate.Date;
                var toDateEnd = toDate.Date.AddDays(1).AddMilliseconds(-1);
                var warrantyClaims = await _repo.ProcedureToList<WarrantyClaimDTO>("spGetWarrantyClaims",
                    ["p_PhoneNumber", "p_Email", "p_ClaimNo", "p_FromDate", "p_ToDate", "p_Status"],
                    [phoneNumber, email, claimNo, fromDateStart, toDateEnd, status]);

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("WarrantyClaims");

                worksheet.Cell(1, 1).Value = "Claim No";
                worksheet.Cell(1, 2).Value = "Created Date";
                worksheet.Cell(1, 3).Value = "Customer Name";
                worksheet.Cell(1, 4).Value = "Phone Number";
                worksheet.Cell(1, 5).Value = "Email";
                worksheet.Cell(1, 6).Value = "Address";
                worksheet.Cell(1, 7).Value = "Product Name";
                worksheet.Cell(1, 8).Value = "Serial Number";
                worksheet.Cell(1, 9).Value = "Status";
                worksheet.Cell(1, 10).Value = "Note";

                int row = 2;
                foreach (var item in warrantyClaims)
                {
                    worksheet.Cell(row, 1).Value = item.ClaimNo;
                    worksheet.Cell(row, 2).Value = item.CreatedDate?.ToString("dd/MM/yyyy HH:mm:ss");
                    worksheet.Cell(row, 3).Value = item.CustomerName;
                    worksheet.Cell(row, 4).Value = item.CustomerPhoneNumber;
                    worksheet.Cell(row, 5).Value = item.CustomerEmail;
                    worksheet.Cell(row, 6).Value = item.CustomerAddress;
                    worksheet.Cell(row, 7).Value = item.ProductName;
                    worksheet.Cell(row, 8).Value = item.SerialNumber;
                    worksheet.Cell(row, 9).Value = GetStatusText(item.Status ?? 0);
                    worksheet.Cell(row, 10).Value = item.Note;
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"WarrantyClaims_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                );
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to export warranty claims to Excel."));
            }
        }

        private string GetStatusText(int status)
        {
            return status switch
            {
                1 => "Tiếp nhận thông tin",
                2 => "Xác minh thông tin",
                3 => "Chẩn đoán sơ bộ",
                4 => "Báo giá",
                5 => "Sửa chữa/bảo hành",
                6 => "Hoàn trả",
                _ => "Không xác định"
            };
        }
    }
}