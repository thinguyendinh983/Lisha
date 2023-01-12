using Lisha.Application.Common.Models;

namespace Lisha.Application.Identity.Users
{
    public class UserListFilter : PaginationFilter
    {
        public bool? IsActive { get; set; }
    }
}
