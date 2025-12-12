using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarrantySystem.Model.Entities;
using WarrantySystem.Repository.IRepositories;
using WarrantySystem.Shared.Common;
namespace WarrantySystem.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        IGenericRepo _repo;
        public OrganizationController(IGenericRepo repo)
        {
            _repo = repo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var organizations = await _repo.GetAll<Organization>();
                return Ok(ApiResponseFactory.Success(organizations));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve organizations."));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var organization = await _repo.GetById<Organization>(id);
                if (organization == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "Organization not found."));
                }
                return Ok(ApiResponseFactory.Success(organization));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve organization."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Organization organization)
        {
            try
            {
                var createdOrganization = await _repo.Insert(organization);
                return Ok(ApiResponseFactory.Success(createdOrganization, "Organization created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to create organization."));
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Organization organization)
        {
            try
            {
                var existingOrganization = await _repo.GetById<Organization>(id);
                if (existingOrganization == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "Organization not found."));
                }
                organization.Id = id; // Ensure the ID is set for update
                var updatedOrganization = await _repo.Update(organization);
                return Ok(ApiResponseFactory.Success(updatedOrganization, "Organization updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to update organization."));
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingOrganization = await _repo.GetById<Organization>(id);
                if (existingOrganization == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "Organization not found."));
                }
                await _repo.DeleteById<Organization>(id);
                return Ok(ApiResponseFactory.Success(null, "Organization deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to delete organization."));
            }
        }
    }
}
