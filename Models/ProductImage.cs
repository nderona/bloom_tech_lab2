﻿using System;

namespace TechPetal_Lab.Models
{
    public class ProductImage
    {
        public Guid Id { get; set; }
        public byte[] Photo { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
    }
}
