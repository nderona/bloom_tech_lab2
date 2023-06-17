using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TechPetal_Lab.Data;

namespace TechPetal_Lab.Areas.Admin.Controllers
{
    [Area( "Admin" )]
    //[Authorize(Roles = "Admin")]
    [AllowAnonymous]
    public class AdminHomeController : Controller
    {
        private readonly ApplicationDbContext context;

        public AdminHomeController ( ApplicationDbContext _context )
        {
            context = _context;
        }
        public IActionResult AdminPanel ()
        {
            ViewBag.NumberOfUsers = context.WebUsers.ToList().Count();
            ViewBag.NumberOfProducts = context.Products.ToList().Count();
            ViewBag.NumberOfPurchases = context.Purchases.ToList().Count();
            ViewBag.NumberOfCoupons = context.UserCoupons.ToList().Count();
            return View();
        }

        public IActionResult ChatRoom ()
        {
            return View();
        }

    }
}
