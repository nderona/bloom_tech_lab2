using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechPetal_Lab.Data;
using TechPetal_Lab.Models;

namespace TechPetal_Lab.Areas.User.Controllers
{
    [Area( "User" )]
    [Authorize( Policy = "LogedInUser" )]
    public class UserHomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserHomeController ( ApplicationDbContext context, UserManager<IdentityUser> userManager )
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index ()
        {
            return View();
        }
        public async Task<IActionResult> Report ( Guid id )
        {
            if( id == null )
            {
                return NotFound();
            }
            var user = await _userManager.GetUserAsync( User );
            var userFind = _context.WebUsers.FirstOrDefault( p => user.Id == p.IdentityUserId );
            ViewBag.getProduct = _context.Products.Find( id );
            ViewBag.getWebUser = userFind;
            return View();
        }
        public async Task<IActionResult> Reports ( [Bind( "Id, ProductId, WebUserId, ReportDetails" )] Report report )
        {
            if( ModelState.IsValid )
            {
                await _context.Reports.AddAsync( report );
                await _context.SaveChangesAsync();
                return Redirect( "~/Home/Index" );
            }
            else
            {
                return NotFound();
            }

        }
        [Authorize]
        public async Task<IActionResult> AvailableCouponsAsync ()
        {
            IdentityUser identityUser = await _userManager.GetUserAsync( User );
            WebUser webUser = _context.WebUsers.Where( x => x.IdentityUserId.Equals( identityUser.Id ) ).FirstOrDefault();
            List<UserCoupon> userCoupons = _context.UserCoupons.Include( x => x.Coupon ).Where( x => x.WebUserId.Equals( webUser.Id ) ).ToList();
            return View( userCoupons );
        }

    }
}
