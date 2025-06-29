using AutoMapper;
using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Tenant mappings
        CreateMap<Tenant, TenantDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        CreateMap<TenantBranding, TenantBrandingDto>();
        CreateMap<BrandingColors, BrandingColorsDto>();
        CreateMap<BrandingLogo, BrandingLogoDto>();
        CreateMap<BrandingFonts, BrandingFontsDto>();
        CreateMap<TenantSettings, TenantSettingsDto>();
        CreateMap<BusinessHourSettings, BusinessHourDto>();
        CreateMap<BookingSettings, BookingSettingsDto>();

        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
            .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId.HasValue ? src.TenantId.ToString() : null));

        // Service mappings
        CreateMap<Service, ServiceDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.DurationMinutes));

        CreateMap<ServiceCategory, ServiceCategoryDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        // Booking mappings
        CreateMap<Booking, BookingDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId.ToString()))
            .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.ServiceId.ToString()))
            .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.Name))
            .ForMember(dest => dest.ServiceDuration, opt => opt.MapFrom(src => src.Service.DurationMinutes))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.BookingDate.ToString("yyyy-MM-dd")))
            .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.BookingTime.ToString(@"hh\:mm")))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Customer mappings
        CreateMap<Customer, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "Customer"))
            .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId.ToString()))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.LastLoginAt, opt => opt.MapFrom(src => (DateTime?)null));
    }
} 