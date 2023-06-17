using System.Collections.Generic;

namespace TechPetal_Lab.Areas.Admin.Models
{
    public class Project
    {
        public string key { get; set; }
    }

    public class Content
    {
        public string type { get; set; }
        public List<Content2> content { get; set; }
    }

    public class Content2
    {

        public string text { get; set; }
        public string type { get; set; }
    }


    public class Description
    {
        public string type { get; set; }
        public int version { get; set; }
        public List<Content> content { get; set; }
    }

    public class Issuetype
    {

        public string name { get; set; }


    }

    public class Fields
    {
        public Project project { get; set; }
        public string summary { get; set; }
        public Description description { get; set; }
        public Issuetype issuetype { get; set; }

        public Priority priority { get; set; }
    }

    public class Priority
    {
        public string id { get; set; }
    }


    public class JiraModel
    {
        public Fields fields { get; set; }
    }
}
