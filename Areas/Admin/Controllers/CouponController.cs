using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechPetal_Lab.Data;
using TechPetal_Lab.Models;

namespace TechPetal_Lab.Areas.Admin.Controllers
{
    [Area( "Admin" )]
    //[Authorize(Roles = "Admin")]
    [AllowAnonymous]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CouponController ( ApplicationDbContext context )
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> ListCoupons ()
        {


            List<Coupon> coupons = await _context.Coupons.Where( coupon => coupon.Public == true ).ToListAsync();


            //return View(context.Products.ToList());
            return View( coupons );
        }

        [HttpGet]
        public IActionResult CreateCoupon ()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCoupon (
            [Bind("Id,Name,Discount,Public,CreatedDate,ExpireDate")]
            Coupon coupon )
        {

            if( ModelState.IsValid )
            {
                coupon.Id = Guid.NewGuid();

                _context.Coupons.Add( coupon );
                await _context.SaveChangesAsync();

                return RedirectToAction( nameof( ListCoupons ) );
            }
            return View( coupon );
        }


        [HttpGet]
        public async Task<IActionResult> EditCoupon ( Guid id )
        {
            if( id == null )
            {
                return NotFound();
            }

            var getCoupon = await _context.Coupons.FindAsync( id );
            if( getCoupon == null )
            {
                return NotFound();
            }
            return View( getCoupon );
        }
        [HttpPost]
        public async Task<IActionResult> EditCoupon ( Guid id, [Bind("Id,Name,Discount,Public,CreatedDate,ExpireDate")]
            Coupon coupon )
        {
            if( id != coupon.Id )
            {
                return NotFound();
            }

            if( ModelState.IsValid )
            {
                _context.Coupons.Update( coupon );
                await _context.SaveChangesAsync();

                return RedirectToAction( nameof( ListCoupons ) );
            }
            return View( coupon );
        }
    }

}
