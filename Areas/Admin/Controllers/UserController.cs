using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TechPetal_Lab.Areas.Admin.ViewModels;
using TechPetal_Lab.Data;
using TechPetal_Lab.Models;

namespace TechPetal_Lab.Areas.Admin.Controllers
{
    [Area( "Admin" )]
    //[Authorize(Roles = "Admin")]
    [AllowAnonymous]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public UserController ( ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager )
        {
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }
        public IActionResult Index ()
        {

            List<TechPetal_Lab.Models.WebUser> webUsers = context.WebUsers.Include( i => i.IdentityUser ).ToList();
            return View( webUsers );
        }

        [HttpGet]
        public async Task<IActionResult> EditRole ( Guid UserId )
        {
            if( UserExists( UserId ) )
            {
                List<SelectListItem> roles = context.Roles.Select( x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name } ).ToList();

                var user = await userManager.FindByIdAsync( UserId.ToString() );
                var userRoles = ( List<string> ) await userManager.GetRolesAsync( user );

                ViewBag.roles = roles;

                EditRoleViewModel model = new EditRoleViewModel
                {
                    UserId = UserId,
                    UserRoles = userRoles
                };

                return View( model );
            }
            else
            {
                return NotFound( "User not found" );
            }

        }
        public bool UserExists ( Guid UserId )
        {
            if( UserId != null )
            {
                var user = context.Users.Where( x => x.Id.Equals( UserId.ToString() ) ).FirstOrDefault();
                if( user != null )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public async Task<IActionResult> AddClaimToUser ( Guid UserId )
        {
            if( UserExists( UserId ) )
            {
                ViewBag.UserClaims = await userManager.GetClaimsAsync( await userManager.FindByIdAsync( UserId.ToString() ) );
                ViewBag.UserId = UserId;
                return View();

            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddClaimToUser ( Guid UserId, string type, string value )
        {
            if( UserExists( UserId ) )
            {
                IdentityUser temp = context.Users.Where( x => x.Id == UserId.ToString() ).FirstOrDefault();

                Claim claim = new Claim( type, value );

                await userManager.AddClaimAsync( temp, claim );

                return RedirectToAction( "AddClaimToUser", new { UserId = UserId } );
            }
            else
            {
                return NotFound();
            }

        }


        public async Task<IActionResult> DeleteClaimFromUser ( string type, string value, Guid UserId )
        {
            if( UserExists( UserId ) )
            {
                IdentityUser user = context.Users.Where( x => x.Id == UserId.ToString() ).FirstOrDefault();
                var userClaims = await userManager.GetClaimsAsync( user );
                foreach( var claim in userClaims )
                {
                    if( claim.Type.Equals( type ) && claim.Value.Equals( value ) )
                    {
                        await userManager.RemoveClaimAsync( user, claim );
                    }
                }
                return RedirectToAction( "AddClaimToUser", new { UserId = UserId } );
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditRoleAsync ( EditRoleViewModel model )
        {
            IdentityUser user = await userManager.FindByIdAsync( model.UserId.ToString() );

            var role = await roleManager.FindByIdAsync( model.RoleId );

            var roleResult = await userManager.AddToRoleAsync( user, role.Name );

            return RedirectToAction( "EditRole", new { model.UserId } );
        }

        public async Task<IActionResult> RemoveRole ( string RoleName, string UserId )
        {
            if( !String.IsNullOrEmpty( RoleName ) || !String.IsNullOrEmpty( UserId ) )
            {
                IdentityUser user = await userManager.FindByIdAsync( UserId );
                if( user != null )
                {
                    await userManager.RemoveFromRoleAsync( user, RoleName );
                    Guid guidUserId = Guid.Parse( UserId );
                    return RedirectToAction( "EditRole", new { UserId = guidUserId } );
                }
                return RedirectToAction( "Index" );

            }
            else
            {
                return RedirectToAction( "Index" );
            }

        }

        public IActionResult Details ( Guid WebUserId )
        {

            if( !String.IsNullOrEmpty( WebUserId.ToString() ) )
            {
                WebUser webUser = context.WebUsers.Include( x => x.IdentityUser ).Include( x => x.Purchases ).Where( x => x.Id.Equals( WebUserId ) ).FirstOrDefault();
                if( webUser != null )
                {
                    return View( webUser );

                }
                return NotFound();
            }
            else
            {
                return NotFound();
            }

        }

        public IActionResult LockUser ( Guid WebUserId )
        {
            if( !String.IsNullOrEmpty( WebUserId.ToString() ) )
            {
                WebUser webUser = context.WebUsers.Where( x => x.Id.Equals( WebUserId ) ).SingleOrDefault();
                if( webUser != null )
                {
                    webUser.Locked = true;
                    context.WebUsers.Update( webUser );
                    context.SaveChanges();
                    return RedirectToAction( nameof( Index ) );
                }
                return NotFound();
            }
            return NotFound();
        }

        public IActionResult UnlockUser ( Guid WebUserId )
        {
            if( !String.IsNullOrEmpty( WebUserId.ToString() ) )
            {
                WebUser webUser = context.WebUsers.Where( x => x.Id.Equals( WebUserId ) ).FirstOrDefault();
                if( webUser != null )
                {
                    webUser.Locked = false;
                    context.WebUsers.Update( webUser );
                    context.SaveChanges();
                    return RedirectToAction( nameof( Index ) );
                }
                return NotFound();
            }
            return NotFound();
        }



        public IActionResult Delete ()
        {
            return RedirectToAction( nameof( Index ) );
        }
    }
}
