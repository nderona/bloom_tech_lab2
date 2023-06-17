using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace TechPetal_Lab.Areas.Admin.ViewModels
{
    public class EditRoleViewModel
    {
        public Guid UserId { get; set; }

        public string RoleId { get; set; }

        public SelectListItem Roles { get; set; }

        public List<string> UserRoles { get; set; }
    }
}
