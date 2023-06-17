using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using TechPetal_Lab.Models;

namespace TechPetal_Lab.Areas.Admin.Controllers
{
    [Area( "Admin" )]
    //[Authorize(Roles = "Admin")]
    [AllowAnonymous]
    public class ManageMessagesController : Controller
    {
        public IActionResult Index ()
        {

            WebRequest request = WebRequest.Create( "http://localhost:3000/" );

            HttpWebResponse response = ( HttpWebResponse ) request.GetResponse();
            string mystream = "";

            using( StreamReader stream = new StreamReader( response.GetResponseStream() ) )
            {
                mystream = stream.ReadToEnd();
            }
            List<Contact> contact = JsonConvert.DeserializeObject<List<Contact>>( mystream ).ToList();


            return View( contact );
        }

        public IActionResult ViewMessage ( string MessageId )
        {

            WebRequest request = WebRequest.Create( "http://localhost:3000/" + MessageId );

            HttpWebResponse response = ( HttpWebResponse ) request.GetResponse();
            string mystream = "";

            using( StreamReader stream = new StreamReader( response.GetResponseStream() ) )
            {
                mystream = stream.ReadToEnd();
            }
            Contact contact = JsonConvert.DeserializeObject<Contact>( mystream );

            return View( contact );

        }


        public IActionResult DeleteMessage ( string MessageId )
        {

            HttpClient client = new HttpClient();



            var test = client.DeleteAsync( "http://localhost:3000/" + MessageId );

            return RedirectToAction( nameof( Index ) );

        }

    }
}
