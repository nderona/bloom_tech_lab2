using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Composition;
using BloomTech.Models;

namespace BloomTech.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext ( DbContextOptions<ApplicationDbContext> options )
            : base( options )
        {
        }
        public DbSet<WebUser> WebUsers { get; set; }
        //public DbSet<Category> Categories { get; set; }
        //// public DbSet<Contact> Contacts { get; set; }
        //public DbSet<Coupon> Coupons { get; set; }
        //public DbSet<UserCoupon> UserCoupons { get; set; }
        //public DbSet<Product> Products { get; set; }
        //public DbSet<ProductImage> ProductImages { get; set; }
        //public DbSet<Review> Reviews { get; set; }
        //public DbSet<Purchase> Purchases { get; set; }
        //public DbSet<ShoppingCart> ShoppingCart { get; set; }
        //public DbSet<Report> Reports { get; set; }
    }
}
