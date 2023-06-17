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
using TechPetal_Lab.ViewModels;

namespace TechPetal_Lab.Areas.User.Controllers
{
    [Area( "User" )]
    [Authorize( Policy = "LogedInUser" )]
    public class PurchaseHistoryController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly ApplicationDbContext context;

        public PurchaseHistoryController ( ApplicationDbContext context, UserManager<IdentityUser> userManager )
        {
            this.context = context;
            this.userManager = userManager;
        }
        public async Task<IActionResult> Index ()
        {
            {

                var currentUser = await userManager.GetUserAsync( HttpContext.User );
                var webUser = await context.WebUsers.Where( user => user.IdentityUserId.Equals( currentUser.Id ) ).FirstAsync();

                var purchasedItems = context.Purchases.Where( user => user.WebUserId.Equals( webUser.Id ) ).Include( p => p.Product ).ToList();


                List<PurchaseHistoryProduct> boughtProducts = new List<PurchaseHistoryProduct>();

                foreach( var item in purchasedItems )
                {
                    PurchaseHistoryProduct temp = new PurchaseHistoryProduct();
                    temp.Name = item.Product.Name;
                    temp.ProductId = item.ProductId;
                    temp.DefaultImage = item.Product.DefaultImage;
                    temp.Reviewed = false;
                    temp.AmountBought = item.Quantity;
                    var reviewedBefore = context.Reviews.Where( r => r.ProductId.Equals( item.ProductId ) && r.WebUserId.Equals( webUser.Id ) ).Count();

                    if( reviewedBefore != 0 )
                    {
                        temp.Reviewed = true;
                        var reviewedBeforeId = context.Reviews.Where( r => r.ProductId.Equals( item.ProductId ) && r.WebUserId.Equals( webUser.Id ) ).First();
                        temp.ReviewedId = reviewedBeforeId.Id;
                    }

                    var amountBought = boughtProducts.Where( p => p.ProductId.Equals( temp.ProductId ) ).Count();

                    if( amountBought == 0 )
                    {
                        boughtProducts.Add( temp );
                    }
                    else
                    {
                        boughtProducts.Where( p => p.ProductId.Equals( temp.ProductId ) ).First().AmountBought += item.Quantity;


                    }


                }

                return View( boughtProducts );
            }
        }

        public async Task<IActionResult> AddReview ( Guid ProductId )
        {
            var currentUser = await userManager.GetUserAsync( HttpContext.User );
            var webUser = await context.WebUsers.Where( user => user.IdentityUserId.Equals( currentUser.Id ) ).FirstAsync();
            byte[] productImage = context.Products.Where( p => p.Id.Equals( ProductId ) ).Select( p => p.DefaultImage ).First();


            Review temp = new Review();
            temp.ProductId = ProductId;
            temp.WebUserId = webUser.Id;
            ViewBag.ProductImage = productImage;

            return View( temp );
        }
        [HttpPost]
        public async Task<IActionResult> AddReview ( Review review )
        {

            var currentUser = await userManager.GetUserAsync( HttpContext.User );
            var webUser = await context.WebUsers.Where( user => user.IdentityUserId.Equals( currentUser.Id ) ).FirstAsync();

            var confirmPurchasedItem = context.Purchases.Where( p => p.WebUserId.Equals( webUser.Id ) && p.ProductId.Equals( review.ProductId ) ).Count();
            var confirmPurchaseReview = context.Reviews.Where( r => r.WebUserId.Equals( webUser.Id ) && r.ProductId.Equals( review.ProductId ) ).Count();

            if( confirmPurchasedItem == 1 && confirmPurchaseReview == 0 )
            {
                await context.Reviews.AddAsync( review );
                await context.SaveChangesAsync();
            }


            return RedirectToAction( nameof( Index ) );
        }

        public async Task<IActionResult> EditReview ( Guid ReviewId )
        {
            var currentUser = await userManager.GetUserAsync( HttpContext.User );
            var webUser = await context.WebUsers.Where( user => user.IdentityUserId.Equals( currentUser.Id ) ).FirstAsync();

            var reviewedBefore = context.Reviews.Where( r => r.Id.Equals( ReviewId ) && r.WebUserId.Equals( webUser.Id ) ).Count();
            if( reviewedBefore == 0 )
            {
                return RedirectToAction( nameof( Index ) );
            }
            Review review = await context.Reviews.Where( r => r.Id.Equals( ReviewId ) && r.WebUserId.Equals( webUser.Id ) ).FirstAsync();

            byte[] productImage = context.Products.Where( p => p.Id.Equals( review.ProductId ) ).Select( p => p.DefaultImage ).First();
            ViewBag.ProductImage = productImage;

            return View( review );
        }
        [HttpPost]
        public async Task<IActionResult> EditReview ( Guid reviewedId, Review review )
        {

            var currentUser = await userManager.GetUserAsync( HttpContext.User );
            var webUser = await context.WebUsers.Where( user => user.IdentityUserId.Equals( currentUser.Id ) ).FirstAsync();

            var reviewedBefore = context.Reviews.Where( r => r.Id.Equals( reviewedId ) && r.WebUserId.Equals( webUser.Id ) ).Count();

            if( reviewedBefore > 0 )
            {
                review.Id = reviewedId;
                context.Reviews.Update( review );
                await context.SaveChangesAsync();
            }

            return RedirectToAction( nameof( Index ) );
        }


    }
}
