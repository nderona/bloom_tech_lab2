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
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController ( ApplicationDbContext context )
        {
            _context = context;
        }

        // GET reports from database and send it to the showReports.cshtml view page for display
        public async Task<IActionResult> ShowReports ( int? pageNumber, string currentFilter, string searchString )
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


            var getReports = from r in _context.Reports.Include( p => p.Product ).Include( w => w.WebUser ) select r;

            if( !String.IsNullOrEmpty( searchString ) )
            {
                getReports = getReports.Where( p => p.Product.Name.Contains( searchString )
                                                   || p.Product.Brand.Contains( searchString ) );
            }
            int pageSize = 5;
            //return View(context.Products.ToList());
            return View( await PaginatedList<Report>.CreateAsync( getReports.AsNoTracking(), pageNumber ?? 1, pageSize ) );
        }

        public async Task<IActionResult> ReportDetails ( Guid? id )
        {
            if( id == null )
            {
                return NotFound();
            }
            var reportdetail = await _context.Reports
                .Include( p => p.Product )
                .Include( w => w.WebUser )
                .FirstOrDefaultAsync( r => r.Id == id );
            if( reportdetail == null )
            {
                return NotFound();
            }

            return View( reportdetail );
        }
        public async Task<IActionResult> DeleteReport ( Guid id )
        {
            if( id == null )
            {
                return NotFound();
            }

            var findReport = await _context.Reports.FindAsync( id );
            _context.Reports.Remove( findReport );
            await _context.SaveChangesAsync();

            return RedirectToAction( nameof( ShowReports ) );
        }
    }
}
