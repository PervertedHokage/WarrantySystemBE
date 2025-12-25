using Microsoft.AspNetCore.Mvc;
using WarrantySystem.Model.DTO;
using WarrantySystem.Model.Entities;
using WarrantySystem.Model.Param;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;

namespace WarrantySystem.API.Controllers.Issues
{
    [Route("api/[controller]")]
    [ApiController]
    public class IssuesController : ControllerBase
    {
        private IGenericRepo _repo;

        public IssuesController(IGenericRepo repo)
        {
            _repo = repo;
        }


        [HttpGet]
        public async Task<IActionResult> GetIssues()
        {
            try
            {
                var issues = (await _repo.FindByExpression<IssuesGroup>(x => x.IsDeleted == false));
                return Ok(ApiResponseFactory.Success(issues, "Lấy dữ liệu thành công"));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));

            }
        }

        [HttpPost("issues")]
        public async Task<IActionResult> GetListIssues([FromBody] IssuesParam request)
        {
            try
            {
                var issues = await _repo.ProcedureToList<dynamic>("spGetIssues",
                    new string[] { "@IssuesGroupId" },
                    new object[] { request.IssuesGroupId });
                return Ok(ApiResponseFactory.Success(issues, "Lấy dữ liệu thành công"));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }

        [HttpPost("save-data-issues")]
        public async Task<IActionResult> SaveData([FromBody] IssuesDTO dto)
        {
            try
            {
                var claims = User.Claims.ToDictionary(x => x.Type, x => x.Value);
                CurrentUser currentUser = ObjectMapper.GetCurrentUser(claims);

                if (dto == null || dto.IssuesGroup == null)
                {
                    return BadRequest(new { status = 0, message = "Dữ liệu không hợp lệ" });
                }

                int IssuesGroupID = 0;

                // Master
                if (dto.IssuesGroup.Id <= 0)

                {
                    await _repo.Insert(dto.IssuesGroup);
                    IssuesGroupID = dto.IssuesGroup.Id; 
                }
                else
                {
                    await _repo.Update(dto.IssuesGroup);
                    IssuesGroupID = dto.IssuesGroup.Id;
                }

                // issues
                if (dto.Issues != null && dto.Issues.Any())
                {
                    foreach (var itemIssues in dto.Issues)
                    {
                        itemIssues.IssuesGroupId = IssuesGroupID;
                        var existing = (await _repo.FindByExpression<Issue>(x => x.IssuesGroupId == IssuesGroupID && x.Id == itemIssues.Id));

                        if (existing == null || itemIssues.Id <= 0)
                        {
                            await _repo.Insert(itemIssues);
                        }

                        else
                            await _repo.Update(itemIssues);
                    }
                }
                if (dto.DeletedIssues?.Count > 0)
                {
                    foreach (var item in dto.DeletedIssues)
                    {
                        var issue = await _repo.GetById<Issue>(item);
                        if (issue == null) continue;
                        issue.IsDeleted = true;
                        await _repo.Update(issue);
                    }
                }
                return Ok(new
                {
                    status = 1,
                    message = "Lưu thành công",
                    id = IssuesGroupID,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, ex.Message));
            }
        }


    }
}
