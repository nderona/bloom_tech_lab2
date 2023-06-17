using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechPetal_Lab.Areas.User.ViewModels;
using TechPetal_Lab.Controllers;
using TechPetal_Lab.Data;
using TechPetal_Lab.Models;
using Coupon = TechPetal_Lab.Models.Coupon;

namespace TechPetal_Lab.Areas.User.Controllers
{
    [Area( "User" )]
    public class PaymentController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ApplicationDbContext context;

        public PaymentController ( ILogger<HomeController> logger, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ApplicationDbContext _context )
        {
            _logger = logger;
            _userManager = userManager;
            this.signInManager = signInManager;
            context = _context;
        }
        [Authorize]
        public bool InStock ( Guid id )
        {
            if( id != null )
            {
                TechPetal_Lab.Models.Product product = context.Products.Where( x => x.Id.Equals( id ) ).FirstOrDefault();
                if( product != null && product.Quantity > 0 )
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
        [Authorize]
        public async Task<bool> InStockMultiple ()
        {

            IdentityUser user = await _userManager.GetUserAsync( User );

            WebUser webUser = context.WebUsers.Where( x => x.IdentityUserId.Equals( user.Id ) ).FirstOrDefault();

            List<ShoppingCart> list = context.ShoppingCart.Include( x => x.Product ).Where( x => x.WebUserId.Equals( webUser.Id ) ).ToList();

            if( user == null || webUser == null || list.Count() == 0 )
            {
                return false;
            }

            foreach( var item in list )
            {

                if( item == null || item.Product.Quantity < item.Quantity )
                {
                    return false;
                }
            }
            return true;
        }

        [Authorize]
        public async Task<bool> MultiplePostPurchase ( PaymentIntent intent, Guid? couponId )
        {
            StripeConfiguration.ApiKey = "sk_test_51J1sdwAhs74rsCWnxp4ezgYrvMkn6DuQix3CqLWJausw4IaJQXNCKmvmSBR0OeWxNoAZPHdK6Hus6UOi3tD9k383006GUJRd0U";
            if( couponId != null )
            {
                TechPetal_Lab.Models.Coupon coupon = context.Coupons.Where( x => x.Id.Equals( couponId ) ).FirstOrDefault();
                if( coupon != null )
                {
                    if( !coupon.Public )
                    {
                        UserCoupon userCoupon = await context.UserCoupons.FindAsync( couponId );
                        context.UserCoupons.Remove( userCoupon );
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    UserCoupon userCoupon = await context.UserCoupons.FindAsync( couponId );
                    context.UserCoupons.Remove( userCoupon );
                    await context.SaveChangesAsync();
                }
            }
            await generateCoupons( intent.Amount );

            var service = new PaymentIntentService();
            PaymentIntent intentWithPaymentMethod = service.Get( intent.Id );

            var service2 = new PaymentMethodService();
            PaymentMethod paymentMethod = service2.Get( intentWithPaymentMethod.PaymentMethodId );

            PaymentMethodModel myDeserializedClass = JsonConvert.DeserializeObject<PaymentMethodModel>( paymentMethod.RawJObject.ToString() );

            IdentityUser user = await _userManager.GetUserAsync( User );

            WebUser webUser = context.WebUsers.Where( x => x.IdentityUserId.Equals( user.Id ) ).FirstOrDefault();

            List<ShoppingCart> list = context.ShoppingCart.Include( x => x.Product ).Where( x => x.WebUserId.Equals( webUser.Id ) ).ToList();

            foreach( var item in list )
            {
                TechPetal_Lab.Models.Product productCheck = context.Products.Find( item.ProductId );

                if( productCheck == null || productCheck.Quantity < item.Quantity && item.Quantity < 1 )
                {
                    return false;
                }
                Purchase purchase = new Purchase
                {
                    ProductId = item.Product.Id,
                    WebUserId = webUser.Id,
                    DateOfPurchase = DateTime.Now,
                    Quantity = item.Quantity,
                    PaymentMethod = Enums.PaymentMethod.CreditCard,
                    PhoneNumber = myDeserializedClass.billing_details.phone,
                    Received = false,
                    Address1 = myDeserializedClass.billing_details.address.line1,
                    Address2 = myDeserializedClass.billing_details.address.line2,
                    City = myDeserializedClass.billing_details.address.city
                };

                item.Product.Quantity -= item.Quantity;
                item.Product.Bought += item.Quantity;

                await context.Purchases.AddAsync( purchase );
                context.Products.Update( item.Product );
                context.ShoppingCart.Remove( item );

            }
            await context.SaveChangesAsync();
            return true;
        }


        [Authorize]
        public async Task<bool> PostPurchase ( Guid productId, PaymentIntent intent )
        {
            StripeConfiguration.ApiKey = "sk_test_51J1sdwAhs74rsCWnxp4ezgYrvMkn6DuQix3CqLWJausw4IaJQXNCKmvmSBR0OeWxNoAZPHdK6Hus6UOi3tD9k383006GUJRd0U";
            var service = new PaymentIntentService();
            PaymentIntent intentWithPaymentMethod = service.Get( intent.Id );

            var service2 = new PaymentMethodService();
            PaymentMethod paymentMethod = service2.Get( intentWithPaymentMethod.PaymentMethodId );

            PaymentMethodModel myDeserializedClass = JsonConvert.DeserializeObject<PaymentMethodModel>( paymentMethod.RawJObject.ToString() );

            IdentityUser user = await _userManager.GetUserAsync( User );

            WebUser webUser = context.WebUsers.Where( x => x.IdentityUserId.Equals( user.Id ) ).FirstOrDefault();

            TechPetal_Lab.Models.Product product = await context.Products.FindAsync( productId );

            Purchase purchase = new Purchase
            {
                ProductId = product.Id,
                WebUserId = webUser.Id,
                DateOfPurchase = DateTime.Now,
                Quantity = 1,
                PaymentMethod = Enums.PaymentMethod.CreditCard,
                PhoneNumber = myDeserializedClass.billing_details.phone,
                Received = false,
                Address1 = myDeserializedClass.billing_details.address.line1,
                Address2 = myDeserializedClass.billing_details.address.line2,
                City = myDeserializedClass.billing_details.address.city
            };

            context.Purchases.Add( purchase );

            product.Quantity--;
            product.Bought++;

            context.Products.Update( product );
            await generateCoupons( intent.Amount );
            await context.SaveChangesAsync();

            return true;

            //Return person to successful order page.
        }

        [Authorize]
        public async Task<IActionResult> SinglePaymentStripe ( string productId )
        {
            if( signInManager.IsSignedIn( User ) )
            {
                if( String.IsNullOrEmpty( productId ) )
                {
                    return NotFound();
                }
                StripeConfiguration.ApiKey = "sk_test_51J1sdwAhs74rsCWnxp4ezgYrvMkn6DuQix3CqLWJausw4IaJQXNCKmvmSBR0OeWxNoAZPHdK6Hus6UOi3tD9k383006GUJRd0U";
                IdentityUser identityUser = await _userManager.GetUserAsync( User );
                Guid guidId = Guid.Parse( productId );
                TechPetal_Lab.Models.Product product = context.Products.Find( guidId );

                if( product == null )
                {
                    return NotFound();
                }

                var customer = await GetStripeCustomerAsync();

                decimal productPrice = ( product.Price - ( decimal ) ( ( double ) product.Price * ( product.Sale * 0.01 ) ) ) * 100;

                var options = new PaymentIntentCreateOptions
                {
                    Amount = ( long ) productPrice,
                    Currency = "eur",
                    // Verify your integration in this guide by including this parameter
                    Metadata = new Dictionary<string, string>
                                {
                                    { "integration_check", "accept_a_payment" },

                                },
                    Customer = customer.Id
                };

                var intentService = new PaymentIntentService();
                var paymentIntent = intentService.Create( options );
                ViewData[ "ClientSecret" ] = paymentIntent.ClientSecret;
                ViewData[ "ProductId" ] = product.Id;
                List<string> test = new List<string>();
                test.Add( "hello1" );
                test.Add( "hello2" );

                ViewData[ "List" ] = test;

                return View();

            }
            return LocalRedirect( "/Identity/Account/Login" );
        }
        [Authorize]
        public async Task<List<ShoppingCart>> getShoppingCart ()
        {
            IdentityUser identityUser = await _userManager.GetUserAsync( User );

            WebUser webUser = context.WebUsers.Where( x => x.IdentityUserId.Equals( identityUser.Id ) ).FirstOrDefault();

            List<ShoppingCart> shoppingCarts = context.ShoppingCart.Include( x => x.Product ).Include( x => x.Coupon ).Where( x => x.WebUserId.Equals( webUser.Id ) ).ToList();
            return shoppingCarts;
        }
        [Authorize]
        public async Task<IActionResult> MultiplePaymentStripe ()
        {
            List<ShoppingCart> shoppingCart = await getShoppingCart();
            if( signInManager.IsSignedIn( User ) && shoppingCart.Count() > 0 )
            {
                StripeConfiguration.ApiKey = "sk_test_51J1sdwAhs74rsCWnxp4ezgYrvMkn6DuQix3CqLWJausw4IaJQXNCKmvmSBR0OeWxNoAZPHdK6Hus6UOi3tD9k383006GUJRd0U";
                IdentityUser identityUser = await _userManager.GetUserAsync( User );
                WebUser webUser = context.WebUsers.Where( x => x.IdentityUserId.Equals( identityUser.Id ) ).FirstOrDefault();
                var customer = await GetStripeCustomerAsync();

                decimal overallPrice = 0;
                foreach( var item in shoppingCart )
                {
                    if( item.Coupon == null )
                    {
                        overallPrice += ( ( item.Product.Price - ( item.Product.Price * ( item.Product.Sale * ( decimal ) 0.01 ) ) ) ) * item.Quantity;
                    }
                    else
                    {
                        overallPrice += ( ( item.Product.Price - ( item.Product.Price * ( item.Coupon.Discount * ( decimal ) 0.01 ) ) ) ) * item.Quantity;
                        if( item.Coupon.Public )
                        {
                            ViewData[ "CouponId" ] = item.Coupon.Id;
                        }
                        else
                        {
                            UserCoupon userCoupon = context.UserCoupons.Where( x => x.CouponId.Equals( item.Coupon.Id ) && x.WebUserId.Equals( webUser.Id ) ).FirstOrDefault();
                            ViewData[ "CouponId" ] = userCoupon.Id;
                        }
                    }
                }

                overallPrice = overallPrice * 100;
                var options = new PaymentIntentCreateOptions
                {
                    Amount = ( long ) overallPrice,
                    Currency = "eur",
                    // Verify your integration in this guide by including this parameter
                    Metadata = new Dictionary<string, string>
                                {
                                    { "integration_check", "accept_a_payment" },

                                },
                    Customer = customer.Id
                };

                var intentService = new PaymentIntentService();
                var paymentIntent = intentService.Create( options );
                ViewData[ "ClientSecret" ] = paymentIntent.ClientSecret;

                return View();
            }
            return LocalRedirect( "/Identity/Account/Login" );
        }
        [Authorize]
        public async Task<Customer> GetStripeCustomerAsync ()
        {
            IdentityUser identityUser = await _userManager.GetUserAsync( User );


            var options1 = new CustomerListOptions
            {
                Email = identityUser.Email
            };

            var service = new CustomerService();
            StripeList<Customer> customers = service.List(
              options1

            );
            if( customers.Data.Count() == 0 )
            {
                var createCustomerOptions = new CustomerCreateOptions
                {
                    Description = "TechPetal customer",
                    Email = identityUser.Email,
                    Balance = 10000000

                };
                var createCustomerService = new CustomerService();
                createCustomerService.Create( createCustomerOptions );
                customers = service.List(
                    options1
                    );
            }

            var customer = customers.Data[ 0 ];
            return customer;
        }
        [Authorize]
        public async Task generateCoupons ( decimal amount )
        {
            IdentityUser identityUser = await _userManager.GetUserAsync( User );
            WebUser webUser = context.WebUsers.Where( x => x.IdentityUserId.Equals( identityUser.Id ) ).FirstOrDefault();

            int normalAmount = ( int ) ( ( int ) amount * ( decimal ) 0.01 );
            webUser.Awarded += normalAmount;
            var timesGenerated = webUser.Awarded / 200;

            for( int i = 0; i < timesGenerated; i++ )
            {

                UserCoupon coupon = new UserCoupon
                {
                    WebUserId = webUser.Id,
                    Used = false
                };

                Coupon tempCoupon = new Coupon();
                tempCoupon.Id = Guid.NewGuid();
                tempCoupon.Public = false;
                tempCoupon.Name = "PetalTron";
                tempCoupon.CreatedDate = DateTime.Now;
                tempCoupon.ExpireDate = DateTime.Now.AddDays( 30 );
                tempCoupon.Discount = 5;
                context.Coupons.Add( tempCoupon );

                coupon.CouponId = tempCoupon.Id;

                context.UserCoupons.Add( coupon );
                webUser.Awarded -= 200;

            }

            context.WebUsers.Update( webUser );
            await context.SaveChangesAsync();
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> SinglePaymentHand ( Guid productId )
        {
            if( productId == Guid.Empty || productId == null )
            {
                return NotFound();
            }
            UserInformationViewModel model = new UserInformationViewModel
            {
                ProductId = productId
            };
            return View( model );
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SinglePaymentHand ( UserInformationViewModel model )
        {
            if( ModelState.IsValid )
            {
                IdentityUser user = await _userManager.GetUserAsync( User );
                WebUser webUser = context.WebUsers.Where( x => x.IdentityUserId.Equals( user.Id ) ).FirstOrDefault();
                Purchase purchase = new Purchase
                {
                    ProductId = model.ProductId,
                    WebUserId = webUser.Id,
                    DateOfPurchase = DateTime.Now,
                    Quantity = 1,
                    PaymentMethod = Enums.PaymentMethod.Cash,
                    PhoneNumber = model.PhoneNumber,
                    Received = false,
                    Address1 = model.Address1,
                    Address2 = model.Address2,
                    City = model.City
                };
                context.Purchases.Add( purchase );
                TechPetal_Lab.Models.Product product = context.Products.Where( x => x.Id.Equals( model.ProductId ) ).FirstOrDefault();

                product.Quantity--;
                product.Bought++;

                context.Products.Update( product );
                decimal overallPrice = ( product.Price - ( product.Price * ( product.Sale * ( decimal ) 0.01 ) ) ) * 100;
                await generateCoupons( overallPrice );
                await context.SaveChangesAsync();
                //return View();
                return RedirectToAction( "SuccessfulOrder", new { orderId = "" } );
            }
            else
            {
                return View( model );
            }
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MultiplePaymentHandAsync ()
        {
            UserInformationViewModel model = new UserInformationViewModel { };
            List<ShoppingCart> shoppingCart = await getShoppingCart();
            foreach( var item in shoppingCart )
            {
                if( item.CouponApplied )
                {
                    model.CouponId = item.Coupon.Id;
                }
            }
            return View( model );
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MultiplePaymentHand ( UserInformationViewModel model )
        {
            if( ModelState.IsValid )
            {
                IdentityUser user = await _userManager.GetUserAsync( User );
                WebUser webUser = context.WebUsers.Where( x => x.IdentityUserId.Equals( user.Id ) ).FirstOrDefault();
                if( model.CouponId != Guid.Empty )
                {
                    TechPetal_Lab.Models.Coupon coupon = context.Coupons.Where( x => x.Id.Equals( model.CouponId ) ).FirstOrDefault();
                    if( coupon != null )
                    {
                        if( !coupon.Public )
                        {
                            UserCoupon userCoupon = context.UserCoupons.Where( x => x.CouponId.Equals( coupon.Id ) && x.WebUserId.Equals( webUser.Id ) ).FirstOrDefault();
                            context.UserCoupons.Remove( userCoupon );
                            await context.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        UserCoupon userCoupon = context.UserCoupons.Where( x => x.CouponId.Equals( coupon.Id ) && x.WebUserId.Equals( webUser.Id ) ).FirstOrDefault();
                        context.UserCoupons.Remove( userCoupon );
                        await context.SaveChangesAsync();
                    }
                }




                List<ShoppingCart> shoppingCart = await getShoppingCart();
                foreach( var item in shoppingCart )
                {
                    Purchase purchase = new Purchase
                    {
                        ProductId = item.Product.Id,
                        WebUserId = webUser.Id,
                        DateOfPurchase = DateTime.Now,
                        Quantity = 1,
                        PaymentMethod = Enums.PaymentMethod.Cash,
                        PhoneNumber = model.PhoneNumber,
                        Received = false,
                        Address1 = model.Address1,
                        Address2 = model.Address2,
                        City = model.City
                    };
                    item.Product.Quantity -= item.Quantity;
                    item.Product.Bought += item.Quantity;

                    await context.Purchases.AddAsync( purchase );
                    context.Products.Update( item.Product );
                    context.ShoppingCart.Remove( item );
                }
                await context.SaveChangesAsync();
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
                await generateCoupons( overallPrice * 100 );
                return RedirectToAction( "SuccessfulOrder", new { orderId = "" } );
            }
            else
            {
                return View( model );
            }
        }
        [Authorize]
        public IActionResult SuccessfulOrder ( string orderId )
        {
            return View( orderId );
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ChoosePaymentMethod ( Guid productId )
        {
            if( productId == Guid.Empty )
            {
                return NotFound();
            }
            TechPetal_Lab.Models.Product product = await context.Products.FindAsync( productId );

            if( product == null )
            {
                return NotFound();
            }
            return View( product );
        }
    }
}
