using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TechPetal_Lab.Data;
using TechPetal_Lab.Models;

namespace TechPetal_Lab.Areas.Admin.Controllers
{
    [Area( "Admin" )]
    //[Authorize(Roles = "Admin")]
    [AllowAnonymous]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext context;

        public OrdersController ( ApplicationDbContext context )
        {
            this.context = context;
        }

        public async Task<IActionResult> Index ( int? pageNumber, string currentFilter, string searchString )
        {
            if( searchString != null )
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData[ "CurrentFilter" ] = searchString;


            var getPurchases = from purchase in context.Purchases.Include( p => p.Product ).Include( u => u.WebUser ) select purchase;

            getPurchases = getPurchases.OrderBy( p => p.Received );

            if( !String.IsNullOrEmpty( searchString ) )
            {
                getPurchases = getPurchases.Where( p => p.Product.Name.Contains( searchString ) || p.WebUser.Name.Equals( searchString ) || p.WebUser.Surname.Equals( searchString ) );

            }
            int pageSize = 10;

            return View( await PaginatedList<Purchase>.CreateAsync( getPurchases.AsNoTracking(), pageNumber ?? 1, pageSize ) );
        }

        public IActionResult ConfirmArrival ( Guid purchaseId )
        {
            context.Purchases.Where( p => p.Id.Equals( purchaseId ) ).First().Received = true;
            context.SaveChanges();

            return RedirectToAction( nameof( Index ) );
        }

        public IActionResult OrderDetails ( Guid purchaseId )
        {
            var purchase = context.Purchases.Where( purchase => purchase.Id.Equals( purchaseId ) ).Include( u => u.WebUser ).Include( p => p.Product ).FirstOrDefault();
            return View( purchase );
        }

    }
}
