using AutoMapper;
using BloomTech.ViewModels;

namespace BloomTech.Models
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
