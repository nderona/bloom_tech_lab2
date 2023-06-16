using System;

namespace BloomTech.Models
{
    public class Report
    {
        public Guid Id { get; set; }
        public Guid WebUserId { get; set; }

        public WebUser WebUser { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        public string ReportDetails { get; set; }
    }
}
