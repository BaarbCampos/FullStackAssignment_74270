using AutoMapper;
using SportsStore.OrderApi.Models;
using SportsStore.Shared.DTOs;

namespace SportsStore.OrderApi.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CheckoutRequestDto, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAtUtc, opt => opt.Ignore())
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        CreateMap<CheckoutRequestItemDto, OrderItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProductName, opt => opt.Ignore())
            .ForMember(dest => dest.UnitPrice, opt => opt.Ignore());

        CreateMap<Order, CheckoutResponseDto>()
            .ForMember(dest => dest.OrderId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Message, opt => opt.Ignore());
    }
}