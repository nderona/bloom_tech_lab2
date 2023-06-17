using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TechPetal_Lab.Areas.Admin.Models;

namespace TechPetal_Lab.Areas.Admin.Controllers
{
    [Area( "Admin" )]
    //[Authorize(Roles = "Admin")]
    [AllowAnonymous]
    public class JiraController : Controller
    {
        public IActionResult Index ()
        {
            JiraModel model = new JiraModel();

            List<IssueTypeModel> types = new List<IssueTypeModel>
            {
                new IssueTypeModel{Type1 = "Performance", Type2 = "Performance"},
                new IssueTypeModel{Type1 = "Payment", Type2 = "Payment"},
                new IssueTypeModel{Type1 = "Account", Type2 = "Account"},
                new IssueTypeModel{Type1 = "Other", Type2 = "Other"}
            };

            List<IssuePriorityType> priorities = new List<IssuePriorityType>
            {
                new IssuePriorityType{id = "1", name = "Highest"},
                new IssuePriorityType{id = "2", name = "High"},
                new IssuePriorityType{id = "3", name = "Medium"},
                new IssuePriorityType{id = "4", name = "Low"},
                new IssuePriorityType{id = "5", name = "Lowest"}
            };
            ViewBag.typeList = types;
            ViewBag.priorityList = priorities;

            return View( model );
        }


        [HttpPost]
        public async Task<IActionResult> AddToJiraAsync ( JiraModel model )
        {
            model.fields.description.type = "doc";
            model.fields.description.version = 1;
            model.fields.description.content[ 0 ].type = "paragraph";
            model.fields.description.content[ 0 ].content[ 0 ].type = "text";
            Project project = new Project();
            project.key = "L2";
            model.fields.project = project;

            string base64Credentials = GetEncodedCredentials();

            HttpClient client = new HttpClient();

            var json = JsonConvert.SerializeObject( model );
            var content = new StringContent( json, Encoding.UTF8, "application/json" );

            client.DefaultRequestHeaders.Add( "Authorization", "Basic " + base64Credentials );

            var test = client.PostAsync( "https://besmir1.atlassian.net/rest/api/3/issue/", content );



            return RedirectToAction( "Index" );
            //return View("Index");
        }

        private string GetEncodedCredentials ()
        {
            string mergedCredentials = string.Format( "{0}:{1}", "bm44276@ubt-uni.net", "UEaN6EfhIJ6beFxLjUAMA2C2" );
            byte[] byteCredentials = UTF8Encoding.UTF8.GetBytes( mergedCredentials );
            return Convert.ToBase64String( byteCredentials );
        }
    }
}