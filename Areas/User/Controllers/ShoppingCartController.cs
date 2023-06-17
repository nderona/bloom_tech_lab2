using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechPetal_Lab.Areas.User.ViewModels;
using TechPetal_Lab.Data;
using TechPetal_Lab.Models;

namespace TechPetal_Lab.Areas.User.Controllers
{
    [Area( "User" )]
    public class ShoppingCartController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public ShoppingCartController ( ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> _signInManager )
        {
            _context = context;
            this.userManager = userManager;
            signInManager = _signInManager;
        }
        [HttpGet]
        public async Task<IActionResult> Index ( string? message )
        {


            var currentUser = await userManager.GetUserAsync( HttpContext.User );
            var webUser = await _context.WebUsers.Where( user => user.IdentityUserId.Equals( currentUser.Id ) ).FirstAsync();
            var getShoppingCart = _context.ShoppingCart.Include( x => x.Product ).Include( x => x.Coupon ).Where( u => webUser.Id == u.WebUserId ).ToList();


            ShoppingCartViewModel model = new ShoppingCartViewModel
            {
                shoppingCart = getShoppingCart,
                price = calculatePrice( getShoppingCart )
            };
            foreach( var item in getShoppingCart )
            {
                if( item.CouponApplied )
                {
                    model.coupon = item.Coupon;
                }
            }
            if( !String.IsNullOrEmpty( message ) )
            {
                model.message = message;
            }
            else
            {
                model.message = "";
            }
            ViewBag.shoppCart = getShoppingCart;
            ViewBag.user = webUser;
            return View( model );
        }
        public async Task<string> AddToCart ( string productId )
        {
            if( !String.IsNullOrEmpty( productId ) )
            {
                Guid guidProductId = Guid.Parse( productId );
                Product product = _context.Products.Find( guidProductId );
                if( product != null )
                {
                    if( signInManager.IsSignedIn( User ) )
                    {
                        IdentityUser user = await userManager.GetUserAsync( User );

                        WebUser webUser = _context.WebUsers.Where( x => x.IdentityUserId.Equals( user.Id ) ).FirstOrDefault();

                        ShoppingCart shoppingCart = _context.ShoppingCart.Where( x => x.ProductId.Equals( guidProductId ) && x.WebUserId.Equals( webUser.Id ) ).FirstOrDefault();

                        if( shoppingCart == null )
                        {
                            if( product.Quantity > 0 )
                            {
                                ShoppingCart item = new ShoppingCart
                                {
                                    ProductId = guidProductId,
                                    WebUserId = webUser.Id,
                                    Quantity = 1,
                                    UserCouponId = null,
                                    CouponId = null,
                                    CouponApplied = false
                                };
                                await _context.ShoppingCart.AddAsync( item );
                                await _context.SaveChangesAsync();

                                return "true";
                            }
                            else
                            {
                                return "outOfStock";
                            }
                        }
                        else if( ( shoppingCart.Quantity + 1 ) <= product.Quantity )
                        {
                            ShoppingCart shoppingCart1 = await _context.ShoppingCart.FindAsync( shoppingCart.Id );
                            shoppingCart1.Quantity = shoppingCart.Quantity + 1;
                            _context.ShoppingCart.Update( shoppingCart1 );
                            await _context.SaveChangesAsync();

                            return "true";
                        }
                        else
                        {
                            return "max";
                        }
                    }
                    else
                    {
                        return "login";
                    }
                }
                else
                {
                    return "notFound";
                }
            }

            return "notFound";
        }

        public async Task<IActionResult> RemoveFromCart ( Guid id )
        {
            if( id == null )
            {
                return RedirectToAction( "Index", new { message = "Empty guid." } );
            }

            var getShoppingCart = _context.ShoppingCart.Include( x => x.Product ).FirstOrDefault( s => id == s.Id );
            var getProduct = _context.Products.FirstOrDefault( p => getShoppingCart.Product.Id == p.Id );
            if( getShoppingCart.Product.Id == getProduct.Id )
            {

                _context.ShoppingCart.Remove( getShoppingCart );
                await _context.SaveChangesAsync();
                return RedirectToAction( "Index" );
            }
            else
            {
                return RedirectToAction( "Index" );
            }
        }
        public async Task<string> IncreaseQuantity ( Guid productId )
        {
            if( productId == null )
            {
                return "false";
            }

            ShoppingCart getUserShoppingCart = _context.ShoppingCart.Include( x => x.Product ).Where( s => productId == s.Id ).FirstOrDefault();
            var getProduct = _context.Products.FirstOrDefault( p => getUserShoppingCart.Product.Id == p.Id );
            if( ( getUserShoppingCart.Quantity + 1 ) <= getProduct.Quantity )
            {
                ShoppingCart shoppingCart1 = await _context.ShoppingCart.FindAsync( getUserShoppingCart.Id );
                shoppingCart1.Quantity = getUserShoppingCart.Quantity + 1;
                _context.ShoppingCart.Update( shoppingCart1 );
                await _context.SaveChangesAsync();

                List<ShoppingCart> shoppingCarts = await getShoppingCart();
                var totalPrice = calculatePrice( shoppingCarts );
                return totalPrice.ToString();
            }
            return "false";
        }
        public async Task<string> DecreaseQuantity ( Guid productId )
        {
            if( productId == null )
            {
                return "false";
            }

            ShoppingCart getUserShoppingCart = _context.ShoppingCart.Include( x => x.Product ).Where( s => productId == s.Id ).FirstOrDefault();
            var getProduct = _context.Products.FirstOrDefault( p => getUserShoppingCart.Product.Id == p.Id );
            if( ( getUserShoppingCart.Quantity - 1 ) > 0 )
            {
                ShoppingCart shoppingCart1 = await _context.ShoppingCart.FindAsync( getUserShoppingCart.Id );
                shoppingCart1.Quantity = getUserShoppingCart.Quantity - 1;
                _context.ShoppingCart.Update( shoppingCart1 );
                await _context.SaveChangesAsync();



                List<ShoppingCart> shoppingCarts = await getShoppingCart();
                var totalPrice = calculatePrice( shoppingCarts );
                return totalPrice.ToString();

            }
            return "false";
        }

        public decimal calculatePrice ( List<ShoppingCart> shoppingCart )
        {
            decimal overallPrice = 0;
            foreach( var item in shoppingCart )
            {
                if( item.Coupon == null )
                {
                    overallPrice += ( item.Product.Price - ( item.Product.Price * ( item.Product.Sale * ( decimal ) 0.01 ) ) ) * item.Quantity;
                }
                else
                {
                    overallPrice += ( item.Product.Price - ( item.Product.Price * ( item.Coupon.Discount * ( decimal ) 0.01 ) ) ) * item.Quantity;
                }
            }
            return overallPrice;
        }
        public async Task<List<ShoppingCart>> getShoppingCart ()
        {
            IdentityUser identityUser = await userManager.GetUserAsync( User );

            WebUser webUser = _context.WebUsers.Where( x => x.IdentityUserId.Equals( identityUser.Id ) ).FirstOrDefault();

            List<ShoppingCart> shoppingCarts = _context.ShoppingCart.Include( x => x.Product ).Include( x => x.Coupon ).Where( x => x.WebUserId.Equals( webUser.Id ) ).ToList();
            return shoppingCarts;
        }
        public async Task<IActionResult> ApplyCoupon ( string couponName )
        {
            if( !String.IsNullOrEmpty( couponName ) && signInManager.IsSignedIn( User ) )
            {

                Coupon coupon = _context.Coupons.Where( x => x.Name.Equals( couponName ) ).FirstOrDefault();

                IdentityUser identityUser = await userManager.GetUserAsync( User );
                WebUser webUser = _context.WebUsers.Where( x => x.IdentityUserId.Equals( identityUser.Id ) ).FirstOrDefault();
                if( coupon != null )
                {
                    var couponDateIsValid = DateTime.Compare( coupon.ExpireDate, DateTime.Now );
                    if( couponDateIsValid < 0 )
                    {
                        return RedirectToAction( "Index", new { message = "Coupon has expired" } );
                    }

                    List<ShoppingCart> shoppingCart = await getShoppingCart();

                    if( coupon.Public )
                    {
                        bool couponApplied = false;
                        foreach( var item in shoppingCart )
                        {
                            if( item.CouponApplied )
                            {
                                couponApplied = true;
                                return RedirectToAction( "Index", new { message = "Coupon already applied. Cannot have 2 active coupons." } );
                            }
                        }
                        if( !couponApplied )
                        {
                            foreach( var item in shoppingCart )
                            {
                                if( item.Product.Sale == 0 )
                                {
                                    item.CouponId = coupon.Id;
                                    item.CouponApplied = true;

                                    _context.ShoppingCart.Update( item );
                                    await _context.SaveChangesAsync();
                                }
                            }
                            return RedirectToAction( "Index" );

                        }
                    }
                    else
                    {
                        List<UserCoupon> userCoupons = _context.UserCoupons.Where( x => x.WebUserId.Equals( webUser.Id ) && x.Used == false ).Include( x => x.Coupon ).ToList();
                        UserCoupon userCoupon = null;

                        foreach( var currentUserCoupon in userCoupons )
                        {

                            var userCouponDateIsValid = DateTime.Compare( currentUserCoupon.Coupon.ExpireDate, DateTime.Now );
                            if( userCouponDateIsValid >= 0 )
                            {
                                userCoupon = currentUserCoupon;
                                break;
                            }
                        }




                        if( userCoupon != null && userCoupon.Used != true )
                        {
                            bool couponApplied = false;
                            foreach( var item in shoppingCart )
                            {
                                if( item.CouponApplied )
                                {
                                    couponApplied = true;
                                    return RedirectToAction( "Index", new { message = "Coupon already applied. Cannot have 2 active coupons." } );
                                }
                            }
                            if( !couponApplied )
                            {
                                foreach( var item in shoppingCart )
                                {
                                    item.CouponId = userCoupon.Coupon.Id;
                                    item.CouponApplied = true;
                                    _context.ShoppingCart.Update( item );
                                    await _context.SaveChangesAsync();
                                }
                                return RedirectToAction( "Index" );
                            }
                        }
                    }
                }
                else
                {
                    return RedirectToAction( "Index", new { message = "Couldn't find coupon to apply." } );
                }
            }
            return RedirectToAction( "Index", new { message = "Please fill the form correctly." } );
        }
        public async Task<IActionResult> RemoveCoupon ( Coupon coupon )
        {
            if( coupon != null )
            {
                List<ShoppingCart> shoppingCart = await getShoppingCart();
                foreach( var item in shoppingCart )
                {
                    if( item.Coupon != null )
                    {
                        ShoppingCart shoppingCartItem = await _context.ShoppingCart.FindAsync( item.Id );

                        shoppingCartItem.Coupon = null;
                        shoppingCartItem.CouponApplied = false;
                        _context.ShoppingCart.Update( shoppingCartItem );
                        await _context.SaveChangesAsync();
                    }
                }
            }
            return RedirectToAction( "Index" );
        }
    }
}
