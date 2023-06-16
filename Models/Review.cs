using System;

namespace BloomTech.Models
{
    public class Review
    {
        public Guid Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        public Guid WebUserId { get; set; }
        public WebUser WebUser { get; set; }
    }
}
