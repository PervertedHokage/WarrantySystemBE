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
    public class UserController : Controller
    {
        private IGenericRepo _repo;

        public UserController(IGenericRepo repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var users = await _repo.GetAll<User>();
                return Ok(ApiResponseFactory.Success(users));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve users."));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = await _repo.GetById<User>(id);
                if (user == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "User not found."));
                }
                return Ok(ApiResponseFactory.Success(user));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to retrieve user."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] User user)
        {
            try
            {
                var createdUser = await _repo.Insert(user);
                return Ok(ApiResponseFactory.Success(createdUser, "User created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to create user."));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] User user)
        {
            try
            {
                var existingUser = await _repo.GetById<User>(id);
                if (existingUser == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "User not found."));
                }
                user.Id = id; // Ensure the ID is set for update
                var updatedUser = await _repo.Update(user);
                return Ok(ApiResponseFactory.Success(updatedUser, "User updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to update user."));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingUser = await _repo.GetById<User>(id);
                if (existingUser == null)
                {
                    return NotFound(ApiResponseFactory.Fail(null, "User not found."));
                }
                await _repo.DeleteById<User>(id);
                return Ok(ApiResponseFactory.Success(null, "User deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseFactory.Fail(ex, "Failed to delete user."));
            }
        }
    }
}