using Microsoft.AspNetCore.Http;
using WarrantySystem.Model.Entities;
using WarrantySystem.Repository.IRepositories;

namespace WarrantySystem.Repository.Services
{
    public class UserPermissionService : IUserPermissionService
    {
        public Dictionary<string, string> Claims { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IGenericRepo _repo;
        public UserPermissionService(IHttpContextAccessor httpContextAccessor, IGenericRepo repo)
        {
            _httpContextAccessor = httpContextAccessor;
            _repo = repo;
        }

        public Dictionary<string, string> GetClaims()
        {
            var claims = _httpContextAccessor.HttpContext?.User?.Claims.ToDictionary(x => x.Type, x => x.Value);
            return claims;
        }

        public async Task<bool> HasPermissionAsync(string userId, string permission)
        {
            if (!int.TryParse(userId, out var id)) return false;

            var permissions = permission.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries); //NTA B update 051125
            var lstUserPermissions = await _repo.ProcedureToList<FormAndFunction>("spPermissionAndShortcutKey", ["UserID"], [userId]);
            foreach (var perm in permissions)
            {
                var userPermissions = lstUserPermissions.FirstOrDefault(p => p.Code == perm);
                if (userPermissions != null && userPermissions.Id > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
