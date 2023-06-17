using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TechPetal_Lab.Data;

namespace TechPetal_Lab.Areas.Admin.Controllers
{
    [Area( "Admin" )]
    //[Authorize(Roles = "Admin")]
    [AllowAnonymous]
    public class AuthorizationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public AuthorizationController ( RoleManager<IdentityRole> _roleManager, ApplicationDbContext _context, UserManager<IdentityUser> userManager )
        {
            roleManager = _roleManager;
            context = _context;
            this.userManager = userManager;
        }
        public IActionResult Index ()
        {
            return View();
        }
        public IActionResult CreateRole ()
        {
            ViewBag.roles = roleManager.Roles.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole ( string roleName )
        {

            if( !await roleManager.RoleExistsAsync( roleName ) )
            {
                if( !String.IsNullOrEmpty( roleName ) )
                {
                    await roleManager.CreateAsync( new IdentityRole( roleName ) );
                }
            }
            return RedirectToAction( nameof( CreateRole ) );
        }
        [HttpGet]
        public IActionResult RoleList ()
        {
            ViewBag.roles = roleManager.Roles.ToList();
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> AddClaimToRole ( string roleName )
        {

            var role = roleManager.FindByNameAsync( roleName ).Result;
            if( role != null )
            {
                ViewBag.RoleClaims = await roleManager.GetClaimsAsync( role );
                ViewBag.Role = roleName;
            }
            else
            {
                return NotFound();
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddClaimToRole ( string roleName, string type, string value )
        {
            if( await roleManager.RoleExistsAsync( roleName ) && !String.IsNullOrEmpty( type ) && !String.IsNullOrEmpty( value ) )
            {
                var role = roleManager.FindByNameAsync( roleName ).Result;
                Claim claim = new Claim( type, value );
                await roleManager.AddClaimAsync( role, claim );
            }
            else
            {
                return NotFound();
            }

            return RedirectToAction( "AddClaimToRole", new { roleName = roleName } );
        }

        public async Task<IActionResult> DeleteClaimFromRole ( string roleName, string type, string value )
        {
            if( await roleManager.RoleExistsAsync( roleName ) && !String.IsNullOrEmpty( type ) && !String.IsNullOrEmpty( value ) )
            {
                var role = roleManager.FindByNameAsync( roleName ).Result;
                var roleClaims = await roleManager.GetClaimsAsync( role );
                foreach( var claim in roleClaims )
                {
                    if( claim.Type.Equals( type ) && claim.Value.Equals( value ) )
                    {
                        await roleManager.RemoveClaimAsync( role, claim );
                    }
                }
                return RedirectToAction( "AddClaimToRole", new { roleName = roleName } );
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> DeleteRole ( string roleName )
        {

            if( !String.IsNullOrEmpty( roleName ) )
            {
                if( await roleManager.RoleExistsAsync( roleName ) )
                {
                    var role = await roleManager.FindByNameAsync( roleName );
                    await roleManager.DeleteAsync( role );
                }
            }
            return RedirectToAction( nameof( CreateRole ) );
        }
    }
}
