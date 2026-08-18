using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vehicle_Service_Management_System.Application.ViewModels.Customer;
using Vehicle_Service_Management_System.Application.ViewModels.Invoice;
using Vehicle_Service_Management_System.Application.ViewModels.JobCard;
using Vehicle_Service_Management_System.Application.ViewModels.Mechanic;
using Vehicle_Service_Management_System.Application.ViewModels.Payment;
using Vehicle_Service_Management_System.Application.ViewModels.ServiceBooking;
using Vehicle_Service_Management_System.Application.ViewModels.ServiceType;
using Vehicle_Service_Management_System.Application.ViewModels.SparePart;
using Vehicle_Service_Management_System.Application.ViewModels.SparePartCategory;
using Vehicle_Service_Management_System.Application.ViewModels.Vehicle;
using Vehicle_Service_Management_System.Application.ViewModels.VehicleBrand;
using Vehicle_Service_Management_System.Application.ViewModels.VehicleType;
using Vehicle_Service_Management_System.Domain.Entities;
using Vehicle_Service_Management_System.Domain.Enums;

// Aliases to avoid ambiguity
using VehicleBrandVM = Vehicle_Service_Management_System.Application.ViewModels.VehicleBrand;
using VehicleTypeVM = Vehicle_Service_Management_System.Application.ViewModels.VehicleType;
using CategorySummary = Vehicle_Service_Management_System.Application.ViewModels.SparePartCategory.SparePartSummaryViewModel;

namespace Vehicle_Service_Management_System.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // =========================================================
            //  CUSTOMER
            // =========================================================
            CreateMap<Customer, CustomerListViewModel>()
                .ForMember(dest => dest.VehicleCount,
                    opt => opt.MapFrom(src => src.Vehicles != null ? src.Vehicles.Count(v => !v.IsDeleted) : 0))
                .ForMember(dest => dest.IsDeleted,
                    opt => opt.MapFrom(src => src.IsDeleted))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.StatusBadge, opt => opt.Ignore());

            CreateMap<Customer, CustomerDetailsViewModel>()
                .ForMember(dest => dest.VehicleCount,
                    opt => opt.MapFrom(src => src.Vehicles != null ? src.Vehicles.Count(v => !v.IsDeleted) : 0))
                .ForMember(dest => dest.ActiveVehicleCount,
                    opt => opt.MapFrom(src => src.Vehicles != null ? src.Vehicles.Count(v => !v.IsDeleted) : 0))
                .ForMember(dest => dest.TotalBookings,
                    opt => opt.MapFrom(src => src.ServiceBookings != null ? src.ServiceBookings.Count(sb => !sb.IsDeleted) : 0))
                .ForMember(dest => dest.ActiveBookings,
                    opt => opt.MapFrom(src => src.ServiceBookings != null ?
                        src.ServiceBookings.Count(sb => !sb.IsDeleted && (sb.Status == BookingStatus.Pending || sb.Status == BookingStatus.InProgress)) : 0))
                .ForMember(dest => dest.IsDeleted,
                    opt => opt.MapFrom(src => src.IsDeleted));

            CreateMap<Customer, CustomerFormViewModel>();
            CreateMap<CustomerFormViewModel, Customer>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookings, opt => opt.Ignore());

            CreateMap<CustomerCreateViewModel, Customer>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookings, opt => opt.Ignore());

            CreateMap<CustomerUpdateViewModel, Customer>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookings, opt => opt.Ignore());

            CreateMap<Customer, CustomerUpdateViewModel>();

            // =========================================================
            //  VEHICLE TYPE
            // =========================================================
            CreateMap<VehicleType, VehicleTypeListViewModel>()
                .ForMember(dest => dest.VehicleCount,
                    opt => opt.MapFrom(src => src.Vehicles != null ? src.Vehicles.Count(v => !v.IsDeleted) : 0))
                .ForMember(dest => dest.StatusBadge,
                    opt => opt.MapFrom(src => src.IsDeleted ? "bg-danger" : "bg-success"))
                .ForMember(dest => dest.CreatedOn,
                    opt => opt.MapFrom(src => src.CreatedOn));

            CreateMap<VehicleType, VehicleTypeDetailsViewModel>()
                .ForMember(dest => dest.TotalVehicles,
                    opt => opt.MapFrom(src => src.Vehicles != null ? src.Vehicles.Count(v => !v.IsDeleted) : 0))
                .ForMember(dest => dest.ActiveVehicles,
                    opt => opt.MapFrom(src => src.Vehicles != null ? src.Vehicles.Count(v => !v.IsDeleted) : 0))
                .ForMember(dest => dest.Vehicles,
                    opt => opt.MapFrom(src => src.Vehicles != null ? src.Vehicles.Where(v => !v.IsDeleted).ToList() : new List<Vehicle>()))
                .ForMember(dest => dest.StatusBadge,
                    opt => opt.MapFrom(src => src.IsDeleted ? "bg-danger" : "bg-success"));

            CreateMap<Vehicle, VehicleTypeVM.VehicleSummaryViewModel>()
                .ForMember(dest => dest.BrandName,
                    opt => opt.MapFrom(src => src.VehicleBrand.BrandName))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.StatusBadge, opt => opt.Ignore());

            CreateMap<VehicleType, VehicleTypeFormViewModel>();
            CreateMap<VehicleTypeFormViewModel, VehicleType>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore());

            CreateMap<VehicleTypeCreateViewModel, VehicleType>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore());

            CreateMap<VehicleTypeUpdateViewModel, VehicleType>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore());

            CreateMap<VehicleType, VehicleTypeUpdateViewModel>();

            CreateMap<VehicleType, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.TypeName));

            // =========================================================
            //  VEHICLE BRAND
            // =========================================================
            CreateMap<VehicleBrand, VehicleBrandListViewModel>()
                .ForMember(dest => dest.VehicleCount,
                    opt => opt.MapFrom(src => src.Vehicles != null ? src.Vehicles.Count(v => !v.IsDeleted) : 0))
                .ForMember(dest => dest.StatusBadge,
                    opt => opt.MapFrom(src => src.IsDeleted ? "bg-danger" : "bg-success"));

            CreateMap<VehicleBrand, VehicleBrandDetailsViewModel>()
                .ForMember(dest => dest.TotalVehicles,
                    opt => opt.MapFrom(src => src.Vehicles != null ? src.Vehicles.Count(v => !v.IsDeleted) : 0))
                .ForMember(dest => dest.ActiveVehicles,
                    opt => opt.MapFrom(src => src.Vehicles != null ? src.Vehicles.Count(v => !v.IsDeleted) : 0))
                .ForMember(dest => dest.Vehicles,
                    opt => opt.MapFrom(src => src.Vehicles != null ? src.Vehicles.Where(v => !v.IsDeleted).ToList() : new List<Vehicle>()))
                .ForMember(dest => dest.StatusBadge,
                    opt => opt.MapFrom(src => src.IsDeleted ? "bg-danger" : "bg-success"));

            CreateMap<VehicleBrand, VehicleBrandFormViewModel>();
            CreateMap<VehicleBrandFormViewModel, VehicleBrand>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore());

            CreateMap<VehicleBrandCreateViewModel, VehicleBrand>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore());

            CreateMap<VehicleBrandUpdateViewModel, VehicleBrand>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore());

            CreateMap<VehicleBrand, VehicleBrandUpdateViewModel>();

            CreateMap<VehicleBrand, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.BrandName));

            // =========================================================
            //  VEHICLE
            // =========================================================
            CreateMap<Vehicle, VehicleBrandVM.VehicleSummaryViewModel>()
                .ForMember(dest => dest.BrandName,
                    opt => opt.MapFrom(src => src.VehicleBrand.BrandName))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.StatusBadge, opt => opt.Ignore());

            CreateMap<Vehicle, VehicleListViewModel>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer.FullName))
                .ForMember(dest => dest.VehicleTypeName,
                    opt => opt.MapFrom(src => src.VehicleType.TypeName))
                .ForMember(dest => dest.VehicleBrandName,
                    opt => opt.MapFrom(src => src.VehicleBrand.BrandName))
                .ForMember(dest => dest.FuelType,
                    opt => opt.MapFrom(src => src.FuelType.ToString()))
                .ForMember(dest => dest.TotalBookings,
                    opt => opt.MapFrom(src => src.ServiceBookings != null ? src.ServiceBookings.Count : 0))
                .ForMember(dest => dest.StatusBadge,
                    opt => opt.MapFrom(src => src.IsDeleted ? "bg-danger" : "bg-success"))
                .ForMember(dest => dest.CreatedOn,
                    opt => opt.MapFrom(src => src.CreatedOn));

            CreateMap<Vehicle, VehicleDetailsViewModel>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer.FullName))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.Customer.PhoneNumber))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.Customer.Email))
                .ForMember(dest => dest.VehicleTypeName,
                    opt => opt.MapFrom(src => src.VehicleType.TypeName))
                .ForMember(dest => dest.VehicleBrandName,
                    opt => opt.MapFrom(src => src.VehicleBrand.BrandName))
                .ForMember(dest => dest.FuelType,
                    opt => opt.MapFrom(src => src.FuelType.ToString()))
                .ForMember(dest => dest.TotalBookings,
                    opt => opt.MapFrom(src => src.ServiceBookings != null ? src.ServiceBookings.Count : 0))
                .ForMember(dest => dest.ActiveBookings,
                    opt => opt.MapFrom(src => src.ServiceBookings != null ?
                        src.ServiceBookings.Count(sb => sb.Status == BookingStatus.Pending || sb.Status == BookingStatus.InProgress) : 0))
                .ForMember(dest => dest.TotalServiceAmount,
                    opt => opt.MapFrom(src => src.ServiceBookings != null ?
                        src.ServiceBookings.Where(sb => sb.Status == BookingStatus.Completed).Sum(sb => sb.TotalAmount) : 0))
                .ForMember(dest => dest.StatusBadge,
                    opt => opt.MapFrom(src => src.IsDeleted ? "bg-danger" : "bg-success"))
                .ForMember(dest => dest.CreatedOn,
                    opt => opt.MapFrom(src => src.CreatedOn))
                .ForMember(dest => dest.ModifiedOn,
                    opt => opt.MapFrom(src => src.ModifiedOn));

            CreateMap<Vehicle, VehicleFormViewModel>()
                .ForMember(dest => dest.Customers, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleTypes, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleBrands, opt => opt.Ignore());

            CreateMap<VehicleFormViewModel, Vehicle>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleType, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleBrand, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookings, opt => opt.Ignore());

            CreateMap<VehicleCreateViewModel, Vehicle>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleType, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleBrand, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookings, opt => opt.Ignore());

            CreateMap<Vehicle, VehicleUpdateViewModel>();

            CreateMap<VehicleUpdateViewModel, Vehicle>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleType, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleBrand, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookings, opt => opt.Ignore());

            // =========================================================
            //  MECHANIC
            // =========================================================
            CreateMap<Mechanic, MechanicListViewModel>()
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.PhoneNumber));

            CreateMap<MechanicFormViewModel, Mechanic>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookings, opt => opt.Ignore());

            CreateMap<Mechanic, MechanicFormViewModel>();

            CreateMap<Mechanic, MechanicDetailsViewModel>()
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.TotalBookings, opt => opt.Ignore())
                .ForMember(dest => dest.ActiveBookings, opt => opt.Ignore());

            CreateMap<MechanicCreateViewModel, Mechanic>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookings, opt => opt.Ignore());

            CreateMap<MechanicUpdateViewModel, Mechanic>()
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookings, opt => opt.Ignore());

            CreateMap<Mechanic, MechanicUpdateViewModel>();

            // =========================================================
            //  SERVICE TYPE
            // =========================================================
            CreateMap<ServiceType, ServiceTypeListViewModel>()
                .ForMember(dest => dest.TotalBookings,
                    opt => opt.MapFrom(src => src.ServiceBookingDetails != null ? src.ServiceBookingDetails.Count() : 0));

            CreateMap<ServiceType, ServiceTypeDetailsViewModel>()
                .ForMember(dest => dest.TotalBookings,
                    opt => opt.MapFrom(src => src.ServiceBookingDetails != null ? src.ServiceBookingDetails.Count() : 0))
                .ForMember(dest => dest.ActiveBookings,
                    opt => opt.MapFrom(src => src.ServiceBookingDetails != null ?
                        src.ServiceBookingDetails.Count(sbd => sbd.ServiceBooking != null &&
                            (sbd.ServiceBooking.Status == BookingStatus.Pending ||
                             sbd.ServiceBooking.Status == BookingStatus.InProgress)) : 0))
                .ForMember(dest => dest.TotalRevenue,
                    opt => opt.MapFrom(src => src.ServiceBookingDetails != null ?
                        src.ServiceBookingDetails
                            .Where(sbd => sbd.ServiceBooking != null && sbd.ServiceBooking.Status == BookingStatus.Completed)
                            .Sum(sbd => sbd.ServiceBooking.TotalAmount) : 0));

            CreateMap<ServiceTypeFormViewModel, ServiceType>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookingDetails, opt => opt.Ignore());

            CreateMap<ServiceType, ServiceTypeFormViewModel>();

            CreateMap<ServiceTypeCreateViewModel, ServiceType>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookingDetails, opt => opt.Ignore());

            CreateMap<ServiceTypeUpdateViewModel, ServiceType>()
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookingDetails, opt => opt.Ignore());

            CreateMap<ServiceType, ServiceTypeUpdateViewModel>();

            // =========================================================
            //  SPARE PART CATEGORY
            // =========================================================
            CreateMap<SparePartCategory, SparePartCategoryListViewModel>()
                .ForMember(dest => dest.SparePartCount,
                    opt => opt.MapFrom(src => src.SpareParts != null ? src.SpareParts.Count(sp => !sp.IsDeleted) : 0))
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.StatusBadge, opt => opt.Ignore());

            CreateMap<SparePartCategory, SparePartCategoryDetailsViewModel>()
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.TotalParts,
                    opt => opt.MapFrom(src => src.SpareParts != null ? src.SpareParts.Count(sp => !sp.IsDeleted) : 0))
                .ForMember(dest => dest.TotalStockValue,
                    opt => opt.MapFrom(src => src.SpareParts != null ?
                        src.SpareParts.Where(sp => !sp.IsDeleted).Sum(sp => sp.StockQuantity * sp.UnitPrice) : 0))
                .ForMember(dest => dest.AveragePartPrice,
                    opt => opt.MapFrom(src =>
                        (src.SpareParts != null && src.SpareParts.Any(sp => !sp.IsDeleted))
                            ? src.SpareParts.Where(sp => !sp.IsDeleted).Average(sp => sp.UnitPrice)
                            : 0))
                .ForMember(dest => dest.SpareParts,
                    opt => opt.MapFrom(src => src.SpareParts != null
                        ? src.SpareParts.Where(sp => !sp.IsDeleted)
                        : Enumerable.Empty<SparePart>()));

            CreateMap<SparePartCategoryFormViewModel, SparePartCategory>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.SpareParts, opt => opt.Ignore());

            CreateMap<SparePartCategory, SparePartCategoryFormViewModel>();

            CreateMap<SparePartCategoryCreateViewModel, SparePartCategory>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.SpareParts, opt => opt.Ignore());

            CreateMap<SparePartCategoryUpdateViewModel, SparePartCategory>()
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.SpareParts, opt => opt.Ignore());

            CreateMap<SparePartCategory, SparePartCategoryUpdateViewModel>();

            // =========================================================
            //  SPARE PART
            // =========================================================
            CreateMap<SparePart, CategorySummary>()
                .ForMember(dest => dest.IsActive,
                    opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Unit,
                    opt => opt.MapFrom(src => src.Unit))
                .ForMember(dest => dest.PartName,
                    opt => opt.MapFrom(src => src.PartName))
                .ForMember(dest => dest.PartCode,
                    opt => opt.MapFrom(src => src.PartCode))
                .ForMember(dest => dest.Brand,
                    opt => opt.MapFrom(src => src.Brand))
                .ForMember(dest => dest.UnitPrice,
                    opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.StockQuantity,
                    opt => opt.MapFrom(src => src.StockQuantity));

            CreateMap<SparePart, SparePartListViewModel>()
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.SparePartCategory.CategoryName))
                .ForMember(dest => dest.StockStatus, opt => opt.Ignore());

            CreateMap<SparePart, SparePartDetailsViewModel>()
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.SparePartCategory.CategoryName))
                .ForMember(dest => dest.StockStatus, opt => opt.Ignore())
                .ForMember(dest => dest.TotalUsed,
                    opt => opt.MapFrom(src => src.InvoiceItems != null ? src.InvoiceItems.Count : 0))
                .ForMember(dest => dest.TotalRevenue,
                    opt => opt.MapFrom(src => src.InvoiceItems != null ? src.InvoiceItems.Sum(ii => ii.TotalAmount) : 0));

            CreateMap<SparePartFormViewModel, SparePart>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceItems, opt => opt.Ignore())
                .ForMember(dest => dest.SparePartCategory, opt => opt.Ignore());

            CreateMap<SparePart, SparePartFormViewModel>()
                .ForMember(dest => dest.Categories, opt => opt.Ignore());

            CreateMap<SparePartCreateViewModel, SparePart>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceItems, opt => opt.Ignore())
                .ForMember(dest => dest.SparePartCategory, opt => opt.Ignore());

            CreateMap<SparePartUpdateViewModel, SparePart>()
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceItems, opt => opt.Ignore())
                .ForMember(dest => dest.SparePartCategory, opt => opt.Ignore());

            CreateMap<SparePart, SparePartUpdateViewModel>();

            CreateMap<SparePart, SparePartLowStockViewModel>()
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.SparePartCategory.CategoryName))
                .ForMember(dest => dest.Shortage, opt => opt.Ignore());

            // =========================================================
            //  SERVICE BOOKING
            // =========================================================
            CreateMap<ServiceBookingCreateViewModel, ServiceBooking>()
                .ForMember(dest => dest.BookingNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => BookingStatus.Pending))
                .ForMember(dest => dest.DeliveryDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
                .ForMember(dest => dest.Mechanic, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookingDetails, opt => opt.Ignore())
                .ForMember(dest => dest.JobCard, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());

            CreateMap<ServiceBooking, ServiceBookingListViewModel>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer.FullName))
                .ForMember(dest => dest.VehicleRegistrationNumber,
                    opt => opt.MapFrom(src => src.Vehicle.RegistrationNumber))
                .ForMember(dest => dest.MechanicName,
                    opt => opt.MapFrom(src => src.Mechanic != null ? src.Mechanic.FullName : "Not Assigned"))
                .ForMember(dest => dest.ServicesSummary,
                    opt => opt.MapFrom(src => string.Join(", ", src.ServiceBookingDetails
                        .Where(sbd => !sbd.IsDeleted)
                        .Select(sbd => sbd.ServiceType.ServiceName))))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.HasJobCard,
                    opt => opt.MapFrom(src => src.JobCard != null))
                .ForMember(dest => dest.HasInvoice,
                    opt => opt.MapFrom(src => src.Invoice != null))
                .ForMember(dest => dest.TotalAmount,
                    opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.ExpectedDeliveryDate,
                    opt => opt.MapFrom(src => src.ExpectedDeliveryDate))
                .ForMember(dest => dest.CreatedOn,
                    opt => opt.MapFrom(src => src.CreatedOn));

            CreateMap<ServiceBookingDetail, ServiceBookingDetailLineViewModel>()
                .ForMember(dest => dest.ServiceName,
                    opt => opt.MapFrom(src => src.ServiceType.ServiceName))
                .ForMember(dest => dest.Quantity,
                    opt => opt.MapFrom(src => (int)src.Quantity))
                .ForMember(dest => dest.Price,
                    opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.TotalAmount,
                    opt => opt.MapFrom(src => src.TotalAmount));

            CreateMap<ServiceBooking, ServiceBookingDetailsViewModel>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer.FullName))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.Customer.PhoneNumber))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.Customer.Email))
                .ForMember(dest => dest.VehicleRegistrationNumber,
                    opt => opt.MapFrom(src => src.Vehicle.RegistrationNumber))
                .ForMember(dest => dest.VehicleModel,
                    opt => opt.MapFrom(src => src.Vehicle.VehicleName))
                .ForMember(dest => dest.Services,
                    opt => opt.MapFrom(src => src.ServiceBookingDetails
                        .Where(sbd => !sbd.IsDeleted)
                        .Select(sbd => sbd)))
                .ForMember(dest => dest.MechanicName,
                    opt => opt.MapFrom(src => src.Mechanic != null ? src.Mechanic.FullName : "Not Assigned"))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.HasJobCard,
                    opt => opt.MapFrom(src => src.JobCard != null))
                .ForMember(dest => dest.HasInvoice,
                    opt => opt.MapFrom(src => src.Invoice != null))
                .ForMember(dest => dest.CreatedOn,
                    opt => opt.MapFrom(src => src.CreatedOn))
                .ForMember(dest => dest.ModifiedOn,
                    opt => opt.MapFrom(src => src.ModifiedOn));

            CreateMap<ServiceBookingDetail, ServiceBookingDetailInputViewModel>()
                .ForMember(dest => dest.ServiceTypeId,
                    opt => opt.MapFrom(src => src.ServiceTypeId))
                .ForMember(dest => dest.ServiceName,
                    opt => opt.MapFrom(src => src.ServiceType.ServiceName))
                .ForMember(dest => dest.Price,
                    opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Quantity,
                    opt => opt.MapFrom(src => (int)src.Quantity))
                .ForMember(dest => dest.TotalAmount,
                    opt => opt.MapFrom(src => src.TotalAmount));

            CreateMap<ServiceBookingUpdateViewModel, ServiceBooking>()
                .ForMember(dest => dest.BookingNumber, opt => opt.Ignore())
                .ForMember(dest => dest.BookingDate, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryDate, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookingDetails, opt => opt.Ignore())
                .ForMember(dest => dest.JobCard, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());

            CreateMap<ServiceBooking, ServiceBookingUpdateViewModel>()
                .ForMember(dest => dest.Customers, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore())
                .ForMember(dest => dest.Mechanics, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceTypes, opt => opt.Ignore())
                .ForMember(dest => dest.Services,
                    opt => opt.MapFrom(src => src.ServiceBookingDetails
                        .Where(sbd => !sbd.IsDeleted)
                        .Select(sbd => sbd)));

            CreateMap<ServiceBookingStatusUpdateViewModel, ServiceBooking>()
                .ForMember(dest => dest.DeliveryDate, opt => opt.MapFrom(src =>
                    src.Status == BookingStatus.Completed
                        ? (DateTime?)(src.DeliveryDate ?? DateTime.UtcNow)
                        : null))
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore())
                .ForMember(dest => dest.Mechanic, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookingDetails, opt => opt.Ignore())
                .ForMember(dest => dest.JobCard, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());

            CreateMap<ServiceBookingDetailInputViewModel, ServiceBookingDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBookingId, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBooking, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceType, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore());

            // =========================================================
            //  JOB CARD
            // =========================================================
            CreateMap<JobCard, JobCardListViewModel>()
                .ForMember(dest => dest.BookingNumber,
                    opt => opt.MapFrom(src => src.ServiceBooking.BookingNumber))
                .ForMember(dest => dest.BookingId,
                    opt => opt.MapFrom(src => src.BookingId))
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.ServiceBooking.Customer.FullName))
                .ForMember(dest => dest.VehicleNumber,
                    opt => opt.MapFrom(src => src.ServiceBooking.Vehicle.RegistrationNumber));

            CreateMap<JobCard, JobCardDetailsViewModel>()
                .ForMember(dest => dest.BookingNumber,
                    opt => opt.MapFrom(src => src.ServiceBooking.BookingNumber))
                .ForMember(dest => dest.BookingId,
                    opt => opt.MapFrom(src => src.BookingId))
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.ServiceBooking.Customer.FullName))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.ServiceBooking.Customer.PhoneNumber))
                .ForMember(dest => dest.VehicleRegistrationNumber,
                    opt => opt.MapFrom(src => src.ServiceBooking.Vehicle.RegistrationNumber))
                .ForMember(dest => dest.MechanicName,
                    opt => opt.MapFrom(src => src.ServiceBooking.Mechanic != null ? src.ServiceBooking.Mechanic.FullName : "Not Assigned"))
                .ForMember(dest => dest.ServicesSummary,
                    opt => opt.MapFrom(src => string.Join(", ", src.ServiceBooking.ServiceBookingDetails
                        .Where(d => !d.IsDeleted)
                        .Select(d => d.ServiceType.ServiceName))));

            CreateMap<JobCard, JobCardPrintViewModel>()
                .ForMember(dest => dest.BookingNumber,
                    opt => opt.MapFrom(src => src.ServiceBooking.BookingNumber))
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.ServiceBooking.Customer.FullName))
                .ForMember(dest => dest.CustomerAddress,
                    opt => opt.MapFrom(src => src.ServiceBooking.Customer.Address))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.ServiceBooking.Customer.PhoneNumber))
                .ForMember(dest => dest.VehicleRegistrationNumber,
                    opt => opt.MapFrom(src => src.ServiceBooking.Vehicle.RegistrationNumber))
                .ForMember(dest => dest.VehicleModel,
                    opt => opt.MapFrom(src => src.ServiceBooking.Vehicle.VehicleName))
                .ForMember(dest => dest.MechanicName,
                    opt => opt.MapFrom(src => src.ServiceBooking.Mechanic != null ? src.ServiceBooking.Mechanic.FullName : "Not Assigned"))
                .ForMember(dest => dest.ServicesSummary,
                    opt => opt.MapFrom(src => string.Join(", ", src.ServiceBooking.ServiceBookingDetails
                        .Where(d => !d.IsDeleted)
                        .Select(d => d.ServiceType.ServiceName))));

            CreateMap<JobCardCreateViewModel, JobCard>()
                .ForMember(dest => dest.JobCardNumber, opt => opt.Ignore())
                .ForMember(dest => dest.ActualCost, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "InProgress"))
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBooking, opt => opt.Ignore());

            CreateMap<JobCardUpdateViewModel, JobCard>()
                .ForMember(dest => dest.BookingId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBooking, opt => opt.Ignore());

            // =========================================================
            //  INVOICE
            // =========================================================
            CreateMap<Invoice, InvoiceListViewModel>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.ServiceBooking != null ? src.ServiceBooking.Customer.FullName : "N/A"))
                .ForMember(dest => dest.VehicleRegistrationNumber,
                    opt => opt.MapFrom(src => src.ServiceBooking != null ? src.ServiceBooking.Vehicle.RegistrationNumber : "N/A"))
                .ForMember(dest => dest.AmountPaid,
                    opt => opt.MapFrom(src => src.Payments != null ?
                        src.Payments.Where(p => !p.IsDeleted).Sum(p => p.AmountPaid) : 0));

            CreateMap<InvoiceItem, InvoiceItemLineViewModel>()
                .ForMember(dest => dest.SparePartName,
                    opt => opt.MapFrom(src => src.SparePart.PartName));

            CreateMap<Invoice, InvoiceDetailsViewModel>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.ServiceBooking.Customer.FullName))
                .ForMember(dest => dest.VehicleRegistrationNumber,
                    opt => opt.MapFrom(src => src.ServiceBooking.Vehicle.RegistrationNumber))
                .ForMember(dest => dest.ServicesSummary,
                    opt => opt.MapFrom(src => string.Join(", ", src.ServiceBooking.ServiceBookingDetails
                        .Where(d => !d.IsDeleted)
                        .Select(d => d.ServiceType.ServiceName))))
                .ForMember(dest => dest.Items,
                    opt => opt.MapFrom(src => src.InvoiceItems))
                .ForMember(dest => dest.Payments,
                    opt => opt.MapFrom(src => src.Payments));

            CreateMap<InvoiceItem, InvoiceItemPrintViewModel>()
                .ForMember(dest => dest.SrNo, opt => opt.Ignore())
                .ForMember(dest => dest.Description,
                    opt => opt.MapFrom(src => src.SparePart.PartName));

            CreateMap<Invoice, InvoicePrintViewModel>()
                .ForMember(dest => dest.InvoiceDate, opt => opt.MapFrom(src => src.CreatedOn))
                .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.BookingId))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.ServiceBooking.Customer.FullName))
                .ForMember(dest => dest.CustomerAddress, opt => opt.MapFrom(src => src.ServiceBooking.Customer.Address))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.ServiceBooking.Customer.PhoneNumber))
                .ForMember(dest => dest.VehicleRegistrationNumber, opt => opt.MapFrom(src => src.ServiceBooking.Vehicle.RegistrationNumber))
                .ForMember(dest => dest.ServicesSummary, opt => opt.MapFrom(src => string.Join(", ", src.ServiceBooking.ServiceBookingDetails
                    .Where(d => !d.IsDeleted)
                    .Select(d => d.ServiceType.ServiceName))))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.InvoiceItems))
                .ForMember(dest => dest.AmountInWords, opt => opt.Ignore());

            CreateMap<InvoiceCreateViewModel, Invoice>()
                .ForMember(dest => dest.InvoiceNumber, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBooking, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceItems, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore())
                .ForMember(dest => dest.SparePartsTotal, opt => opt.Ignore())
                .ForMember(dest => dest.GSTAmount, opt => opt.Ignore())
                .ForMember(dest => dest.GrandTotal, opt => opt.Ignore());

            CreateMap<InvoiceItemInputViewModel, InvoiceItem>()
                .ForMember(dest => dest.Quantity,
                    opt => opt.MapFrom(src => src.QuantityUsed))
                .ForMember(dest => dest.UnitPrice,
                    opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.TotalAmount,
                    opt => opt.MapFrom(src => src.UnitPrice * src.QuantityUsed))
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.SparePart, opt => opt.Ignore());

            CreateMap<Invoice, InvoiceUpdateViewModel>().ReverseMap();

            // =========================================================
            //  PAYMENT
            // =========================================================
            CreateMap<Payment, PaymentListViewModel>()
                .ForMember(dest => dest.PaymentMode,
                    opt => opt.MapFrom(src => src.PaymentMode.ToString()))
                .ForMember(dest => dest.InvoiceNumber,
                    opt => opt.MapFrom(src => src.Invoice.InvoiceNumber))
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Invoice.ServiceBooking.Customer.FullName));

            CreateMap<Payment, PaymentDetailsViewModel>()
                .ForMember(dest => dest.PaymentMode,
                    opt => opt.MapFrom(src => src.PaymentMode.ToString()))
                .ForMember(dest => dest.InvoiceNumber,
                    opt => opt.MapFrom(src => src.Invoice.InvoiceNumber))
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Invoice.ServiceBooking.Customer.FullName))
                .ForMember(dest => dest.VehicleNumber,
                    opt => opt.MapFrom(src => src.Invoice.ServiceBooking.Vehicle.RegistrationNumber))
                .ForMember(dest => dest.InvoiceGrandTotal,
                    opt => opt.MapFrom(src => src.Invoice.GrandTotal))
                .ForMember(dest => dest.TotalPaid, opt => opt.Ignore())
                .ForMember(dest => dest.Balance, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore());

            CreateMap<Payment, PaymentLineViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PaymentMode,
                    opt => opt.MapFrom(src => src.PaymentMode.ToString()))
                .ForMember(dest => dest.AmountPaid,
                    opt => opt.MapFrom(src => src.AmountPaid))
                .ForMember(dest => dest.PaymentDate,
                    opt => opt.MapFrom(src => src.PaymentDate));

            CreateMap<PaymentFormViewModel, Payment>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());

            CreateMap<Payment, PaymentFormViewModel>()
                .ForMember(dest => dest.InvoiceNumber,
                    opt => opt.MapFrom(src => src.Invoice.InvoiceNumber))
                .ForMember(dest => dest.InvoiceGrandTotal,
                    opt => opt.MapFrom(src => src.Invoice.GrandTotal))
                .ForMember(dest => dest.AmountPaidSoFar, opt => opt.Ignore())
                .ForMember(dest => dest.RemainingBalance, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentStatus, opt => opt.Ignore());

            CreateMap<PaymentCreateViewModel, Payment>()
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());

            CreateMap<PaymentUpdateViewModel, Payment>()
                .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore());
        }
    }
}