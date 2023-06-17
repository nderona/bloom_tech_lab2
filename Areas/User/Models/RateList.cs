using System.Collections.Generic;

namespace TechPetal_Lab.Areas.User.Models
{
    public class RateList
    {
        public IEnumerable<RateModel> Rates { get; set; }
        public int SelectedRate { get; set; }
    }
}
