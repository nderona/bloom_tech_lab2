using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TechPetal_Lab.Data;
using TechPetal_Lab.Models;
using TechPetal_Lab.ViewModels;

namespace TechPetal_Lab.Areas.Admin.Controllers
{
    [Area( "Admin" )]
    //[Authorize(Roles = "Admin")]
    [AllowAnonymous]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public ProductController ( ApplicationDbContext _context, IMapper _mapper )
        {
            context = _context;
            mapper = _mapper;
        }
        public async Task<IActionResult> Index ( int? pageNumber )
        {
            var products = from p in context.Products select p;
            int pageSize = 8;


            //return View(context.Products.ToList());
            return View( await PaginatedList<Product>.CreateAsync( products.AsNoTracking(), pageNumber ?? 1, pageSize ) );
        }

        public async Task<IActionResult> CreateProduct ()
        {

            List<SelectListItem> categories = await context
                .Categories
                .Select( x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name } )
                .ToListAsync();

            ViewBag.Categories = categories;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct ( ProductViewModel product )
        {

            if( ModelState.IsValid && product.DefaultPhoto != null && product.Images != null )
            {
                product.Id = Guid.NewGuid();

                var newProduct = mapper.Map<Product>( product );
                newProduct.CategoryId = product.SelectedCategory;

                byte[] defaultImageBytes = null;

                using( var stream = new MemoryStream() )
                {
                    await product.DefaultPhoto.CopyToAsync( stream );
                    defaultImageBytes = stream.ToArray();

                    newProduct.DefaultImage = defaultImageBytes;
                }

                context.Add( newProduct );

                foreach( var image in product.Images )
                {
                    ProductImage productImages = new ProductImage();
                    productImages.Id = Guid.NewGuid();

                    byte[] ImagesBytes = null;

                    using( var imageStream = new MemoryStream() )
                    {
                        await image.CopyToAsync( imageStream );
                        ImagesBytes = imageStream.ToArray();
                    }

                    productImages.Photo = ImagesBytes;
                    productImages.ProductId = product.Id;

                    context.Add( productImages );
                    await context.SaveChangesAsync();
                }

                await context.SaveChangesAsync();
            }

            return RedirectToAction( nameof( Index ) );
        }

        public IActionResult DisplayCategories ()
        {
            List<Category> Categories = context.Categories.ToList();

            return View( Categories );
        }


        [HttpPost]
        public IActionResult CreateNewCategory ( string categoryName )
        {

            int countCategory = context.Categories.Where( category => category.Name.Equals( categoryName ) ).Count();

            if( countCategory == 0 )
            {
                Category newCategory = new Category();

                newCategory.Id = Guid.NewGuid();
                newCategory.Name = categoryName;

                context.Add( newCategory );
                context.SaveChanges();
            }


            return RedirectToAction( nameof( DisplayCategories ) );

        }


        public async Task<IActionResult> RemoveCategoryAsync ( Guid categoryId )
        {

            var category = await context.Categories.FindAsync( categoryId );
            context.Categories.Remove( category );
            await context.SaveChangesAsync();

            return RedirectToAction( nameof( DisplayCategories ) );
        }

        public async Task<IActionResult> ProductDetails ( Guid productId )
        {
            if( ProductExists( productId ) )
            {
                Product product = await context.Products.FindAsync( productId );
                List<ProductImage> productImages = await context
                    .ProductImages
                    .Where( x => x.ProductId.Equals( productId ) )
                    .Include( c => c.Product.Category )
                    .ToListAsync();

                ViewBag.ProductImages = productImages;

                List<Review> productReviews = await context.Reviews.Where( x => x.ProductId.Equals( product.Id ) ).Include( u => u.WebUser ).ToListAsync();
                List<CommentReviews> commentReviews = new List<CommentReviews>();
                var sum = 0;

                if( productReviews.Count() == 0 )
                {
                    ViewBag.ProductReviewsAverage = 0;
                    ViewBag.ProductReviewsComments = "";
                    ViewBag.productRevCount = 0;
                }
                else
                {

                    foreach( var review in productReviews )
                    {
                        sum += review.Rating;

                        CommentReviews tempCommentReview = new CommentReviews();
                        tempCommentReview.Name = review.WebUser.Name;
                        tempCommentReview.Surname = review.WebUser.Surname;
                        tempCommentReview.Comment = review.Comment;

                        commentReviews.Add( tempCommentReview );

                    }

                    sum = sum / productReviews.Count();

                    ViewBag.ProductReviewsAverage = sum;
                    ViewBag.ProductReviewsComments = commentReviews;

                    var revCount = productReviews.Count();
                    ViewBag.productRevCount = revCount;

                }

                if( product == null )
                {
                    return NotFound();
                }

                return View( product );
            }
            else
            {
                return RedirectToAction( nameof( Index ) );
            }
        }

        public bool ProductExists ( Guid productId )
        {
            var productCount = context.Products.Where( x => x.Id.Equals( productId ) ).Count();

            if( productCount > 0 )
            {
                return true;
            }

            return false;
        }

        public async Task<IActionResult> DeleteProduct ( Guid productId )
        {

            if( ProductExists( productId ) )
            {
                var product = await context.Products.FindAsync( productId );
                context.Products.Remove( product );
                await context.SaveChangesAsync();
            }

            return RedirectToAction( nameof( Index ) );
        }

        public async Task<IActionResult> EditProduct ( Guid productId )
        {

            if( ProductExists( productId ) )
            {
                Product product = await context.Products.FindAsync( productId );
                var editProduct = mapper.Map<ProductViewModel>( product );
                editProduct.SelectedCategory = product.CategoryId;

                List<SelectListItem> categories = await context.Categories.Select( x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name } ).ToListAsync();
                ViewBag.Categories = categories;
                ViewBag.DefaultImage = product.DefaultImage;

                return View( editProduct );
            }
            else
            {
                return RedirectToAction( nameof( Index ) );
            }

        }



        [HttpPost]
        public async Task<IActionResult> EditProduct ( Guid productId, ProductViewModel product )
        {

            if( ModelState.IsValid )
            {
                var oldProductData = await context.Products.AsNoTracking().Where( x => x.Id.Equals( productId ) ).FirstAsync();


                var editProduct = mapper.Map<Product>( product );
                editProduct.Id = productId;
                editProduct.CategoryId = product.SelectedCategory;
                editProduct.DefaultImage = oldProductData.DefaultImage;

                context.Products.Update( editProduct );

                await context.SaveChangesAsync();
                return RedirectToAction( "EditProduct", new { productId = productId } );
            }


            return RedirectToAction( "EditProduct", new { productId = productId } );
        }

        public async Task<IActionResult> SearchProducts ( string search, int? pageNumber )
        {

            if( search != null && !search.Trim().Equals( "" ) )
            {
                var products = from p in context.Products select p;
                var productsResults = products.Where( p => p.Name.ToLower().Contains( search.ToLower() ) );
                int pageSize = 8;
                ViewBag.Search = search;

                return View( await PaginatedList<Product>.CreateAsync( productsResults.AsNoTracking(), pageNumber ?? 1, pageSize ) );
            }

            return RedirectToAction( nameof( Index ) );
        }
    }
}
