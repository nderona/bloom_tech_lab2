using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace BloomTech.Models
{
    public class WebUser
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string City { get; set; }
        public string IdentityUserId { get; set; }
        public IdentityUser IdentityUser { get; set; }
        public bool Locked { get; set; }
        public int Awarded { get; set; }

        public List<Purchase> Purchases { get; set; }
    }
}
