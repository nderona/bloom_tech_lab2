using AutoMapper;
using TechPetal_Lab.ViewModels;

namespace TechPetal_Lab.Models
{
    public class ProductMapper : Profile
    {
        public ProductMapper ()
        {
            CreateMap<Product, ProductViewModel>();
            CreateMap<ProductViewModel, Product>();
        }

    }
}
