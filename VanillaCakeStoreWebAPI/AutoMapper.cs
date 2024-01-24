using AutoMapper;
using BusinessObject.Models;
using VanillaCakeStoreWebAPI.DTO.Category;
using VanillaCakeStoreWebAPI.DTO.Customer;
using VanillaCakeStoreWebAPI.DTO.Order;
using VanillaCakeStoreWebAPI.DTO.Product;

namespace eBookStoreWebAPI
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            CreateMap<Category, CategoryDTO>().ReverseMap();

            CreateMap<Customer, CustomerEditDTO>().ReverseMap();
            CreateMap<Customer, CustomerDTO>()
               .ForMember(second => second.Email,
               map => map.MapFrom(
                   first => first.Accounts != null ? first.Accounts.FirstOrDefault().Email : null
                   ));

            CreateMap<Product, ProductAddDTO>().ReverseMap();
            CreateMap<Product, ProductEditDTO>().ReverseMap();
            CreateMap<Product, ProductDTO>()
                .ForMember(second => second.CategoryName,
                map => map.MapFrom(
                    first => first.Category != null ? first.Category.CategoryName : null
                    ));

            CreateMap<Order, OrderDTO>()
                .ForMember(second => second.OrderDetails, map => map.MapFrom(
                                        first => first.OrderDetails
                                        ));
            CreateMap<Order, OrderAdminDTO>()
              .ForMember(second => second.EmployeeName,
              map => map.MapFrom(
                  first => first.Employee.LastName
                  ))
              .ForMember(second => second.CustomerName,
              map => map.MapFrom(
                  first => first.Customer.ContactName
                  ));

            CreateMap<OrderDetail, CartItemDTO>()
               .ForMember(second => second.ProductID,
               map => map.MapFrom(
                   first => first.ProductId
                   ))
               .ForMember(second => second.ProductName,
               map => map.MapFrom(
                   first => first.Product.ProductName
                   ));
        }
    }
}
