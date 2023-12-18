using EPiServer.Cms.UI.AspNetIdentity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace HeadlessCms
{
    public class CreateAdminUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        //NOTE: it's more appropriate using UIUserProvider _uIUserProvider, UIRoleProvider _uIRoleProvider instead likes the commented CreateUser
        public CreateAdminUserMiddleware(RequestDelegate next, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _next = next;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await CreateAdminUser();

            await _next(httpContext);
        }

        public async Task<IdentityResult> CreateAdminUser(string email = "admin@example.local")
        {            
            var admUser = await _userManager.FindByNameAsync(email);
            if (admUser == null)
            {
                var user = new ApplicationUser()
                {
                    Email = email,
                    UserName = email,
                    IsApproved = true
                };

                await _userManager.CreateAsync(user, "Episerver123!");
                admUser = await _userManager.FindByNameAsync(email);
            }

            if (!await _roleManager.RoleExistsAsync("Administrators"))
                await _roleManager.CreateAsync(new IdentityRole("Administrators"));

            if (!await _roleManager.RoleExistsAsync("WebAdmins"))
                await _roleManager.CreateAsync(new IdentityRole("WebAdmins"));

            return await _userManager.AddToRolesAsync(admUser, new List<string> { "Administrators", "WebAdmins" });
        }

        //private async Task CreateUser(string username, string email, IEnumerable<string> roles)
        //{
        //    var result = await _uIUserProvider.CreateUserAsync(username, "Episerver123!", email, null, null, true);
        //    if (result.Status == UIUserCreateStatus.Success)
        //    {
        //        foreach (var role in roles)
        //        {
        //            var exists = await _uIRoleProvider.RoleExistsAsync(role);
        //            if (!exists)
        //            {
        //                await _uIRoleProvider.CreateRoleAsync(role);
        //            }
        //        }

        //        await _uIRoleProvider.AddUserToRolesAsync(result.User.Username, roles);
        //    }
        //}

    }

    public static class MyMiddlewareExtensions
    {
        public static IApplicationBuilder UseMyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CreateAdminUserMiddleware>();
        }
    }
}
