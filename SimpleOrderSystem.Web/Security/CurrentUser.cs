using System.Security.Claims;
using SimpleOrderSystem.Data;
using Microsoft.AspNetCore.Http;

namespace SimpleOrderSystem.Web.Security
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor accessor;

        public CurrentUser(IHttpContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        public int UserId
        {
            get
            {
                var value = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(value, out var id) ? id : 0;
            }
        }
    }
}
