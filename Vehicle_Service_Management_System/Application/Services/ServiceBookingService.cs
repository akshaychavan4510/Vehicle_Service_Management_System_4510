    using AutoMapper;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using Vehicle_Service_Management_System.Application.ViewModels.Common;
    using Vehicle_Service_Management_System.Application.ViewModels.ServiceBooking;
    using Vehicle_Service_Management_System.Domain.Entities;
    using Vehicle_Service_Management_System.Domain.Enums;
    using Vehicle_Service_Management_System.Infrastructure.Data;

    namespace Vehicle_Service_Management_System.Application.Services
    {
        public class ServiceBookingService
        {
            private readonly ApplicationDbContext _context;
            private readonly IMapper _mapper;

            public ServiceBookingService(ApplicationDbContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            // ─── PAGED LIST ──────────────────────────────────────────────────
            public async Task<PagedResult<ServiceBookingListViewModel>> GetPagedAsync(
                BookingStatus? status = null,
                string? searchTerm = null,
                int? customerId = null,
                int? vehicleId = null,
                DateTime? dateFrom = null,
                DateTime? dateTo = null,
                int pageNumber = 1,
                int pageSize = 10)
            {
                var query = _context.ServiceBookings
                    .Include(b => b.Customer)
                    .Include(b => b.Vehicle)
                    .Include(b => b.Mechanic)
                    .Include(b => b.ServiceBookingDetails)
                        .ThenInclude(d => d.ServiceType)
                    .Include(b => b.JobCard)
                    .Include(b => b.Invoice)
                    .Where(b => !b.IsDeleted)
                    .AsQueryable();

                if (status.HasValue)
                    query = query.Where(b => b.Status == status.Value);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(b =>
                        b.BookingNumber.Contains(searchTerm) ||
                        b.Customer.FullName.Contains(searchTerm) ||
                        b.Vehicle.RegistrationNumber.Contains(searchTerm) ||
                        (b.Complaint != null && b.Complaint.Contains(searchTerm)));
                }

                if (customerId.HasValue)
                    query = query.Where(b => b.CustomerId == customerId.Value);

                if (vehicleId.HasValue)
                    query = query.Where(b => b.VehicleId == vehicleId.Value);

                if (dateFrom.HasValue)
                    query = query.Where(b => b.BookingDate >= dateFrom.Value);

                if (dateTo.HasValue)
                {
                    var endDate = dateTo.Value.Date.AddDays(1);
                    query = query.Where(b => b.BookingDate < endDate);
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderBy(b => b.BookingNumber)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<ServiceBookingListViewModel>
                {
                    Items = _mapper.Map<List<ServiceBookingListViewModel>>(items),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            // ─── BUILD FORM (CREATE) ─────────────────────────────────────
            public async Task<ServiceBookingCreateViewModel> BuildFormAsync()
            {
                return new ServiceBookingCreateViewModel
                {
                    Customers = await _context.Customers
                        .Where(c => !c.IsDeleted && c.IsActive)
                        .OrderBy(c => c.FullName)
                        .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.FullName })
                        .ToListAsync(),

                    Vehicles = await _context.Vehicles
                        .Where(v => !v.IsDeleted && v.IsActive)
                        .OrderBy(v => v.RegistrationNumber)
                        .Select(v => new SelectListItem { Value = v.Id.ToString(), Text = $"{v.RegistrationNumber} - {v.VehicleName}" })
                        .ToListAsync(),

                    Mechanics = await _context.Mechanics
                        .Where(m => m.IsAvailable && !m.IsDeleted)
                        .OrderBy(m => m.FullName)
                        .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.FullName })
                        .ToListAsync(),

                    ServiceTypes = await _context.ServiceTypes
                        .Where(st => !st.IsDeleted && st.IsActive)
                        .OrderBy(st => st.ServiceName)
                        .Select(st => new SelectListItem { Value = st.Id.ToString(), Text = $"{st.ServiceName} (Rs. {st.LabourCharge})" })
                        .ToListAsync()
                };
            }

            // ─── GENERATE BOOKING NUMBER ────────────────────────────────
            public async Task<string> GenerateBookingNumberAsync()
            {
                var lastBooking = await _context.ServiceBookings
                    .OrderByDescending(b => b.Id)
                    .FirstOrDefaultAsync();

                int nextNumber = (lastBooking?.Id ?? 0) + 1;
                return $"SB{nextNumber:D4}";
            }

            // ─── CREATE ────────────────────────────────────────────────────
            public async Task<int> CreateAsync(ServiceBookingCreateViewModel model)
            {
                if (!await _context.Customers.AnyAsync(c => c.Id == model.CustomerId && !c.IsDeleted && c.IsActive))
                    throw new InvalidOperationException("Selected customer does not exist or is inactive.");

                if (!await _context.Vehicles.AnyAsync(v => v.Id == model.VehicleId && !v.IsDeleted && v.IsActive))
                    throw new InvalidOperationException("Selected vehicle does not exist or is inactive.");

                if (model.MechanicId.HasValue)
                {
                    var mechanic = await _context.Mechanics
                        .FirstOrDefaultAsync(m => m.Id == model.MechanicId.Value && m.IsAvailable && !m.IsDeleted);

                    if (mechanic is null)
                        throw new InvalidOperationException("Selected mechanic is not available.");
                }

                if (model.Services == null || !model.Services.Any())
                    throw new InvalidOperationException("At least one service must be selected.");

                var booking = _mapper.Map<ServiceBooking>(model);
                booking.BookingNumber = await GenerateBookingNumberAsync();
                booking.BookingDate = model.BookingDate;
                booking.Status = BookingStatus.Pending;
                booking.CreatedOn = DateTime.UtcNow;
                booking.IsDeleted = false;
                booking.IsActive = true;

                booking.TotalAmount = model.Services.Sum(s => s.Price * s.Quantity);

                foreach (var serviceInput in model.Services)
                {
                    var detail = _mapper.Map<ServiceBookingDetail>(serviceInput);
                    detail.CreatedOn = DateTime.UtcNow;
                    detail.IsDeleted = false;
                    detail.IsActive = true;
                    booking.ServiceBookingDetails.Add(detail);
                }

                _context.ServiceBookings.Add(booking);
                await _context.SaveChangesAsync();
                return booking.Id;
            }

            // ─── GET BY ID (DETAILS) ─────────────────────────────────────
            public async Task<ServiceBookingDetailsViewModel?> GetByIdAsync(int id)
            {
                var booking = await _context.ServiceBookings
                    .Include(b => b.Customer)
                    .Include(b => b.Vehicle)
                    .Include(b => b.Mechanic)
                    .Include(b => b.ServiceBookingDetails)
                        .ThenInclude(d => d.ServiceType)
                    .Include(b => b.JobCard)
                    .Include(b => b.Invoice)
                    .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

                return booking is null ? null : _mapper.Map<ServiceBookingDetailsViewModel>(booking);
            }

            // ─── GET FOR EDIT ─────────────────────────────────────────────
            public async Task<ServiceBookingUpdateViewModel?> GetForEditAsync(int id)
            {
                var booking = await _context.ServiceBookings
                    .Include(b => b.Customer)
                    .Include(b => b.Vehicle)
                    .Include(b => b.Mechanic)
                    .Include(b => b.ServiceBookingDetails)
                        .ThenInclude(d => d.ServiceType)
                    .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

                if (booking is null) return null;

                var viewModel = _mapper.Map<ServiceBookingUpdateViewModel>(booking);

                // Populate dropdowns
                viewModel.Customers = await _context.Customers
                    .Where(c => !c.IsDeleted && c.IsActive)
                    .OrderBy(c => c.FullName)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.FullName,
                        Selected = c.Id == booking.CustomerId
                    })
                    .ToListAsync();

                viewModel.Vehicles = await _context.Vehicles
                    .Where(v => !v.IsDeleted && v.IsActive)
                    .OrderBy(v => v.RegistrationNumber)
                    .Select(v => new SelectListItem
                    {
                        Value = v.Id.ToString(),
                        Text = $"{v.RegistrationNumber} - {v.VehicleName}",
                        Selected = v.Id == booking.VehicleId
                    })
                    .ToListAsync();

                viewModel.Mechanics = await _context.Mechanics
                    .Where(m => m.IsAvailable && !m.IsDeleted)
                    .OrderBy(m => m.FullName)
                    .Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.FullName,
                        Selected = m.Id == booking.MechanicId
                    })
                    .ToListAsync();

                viewModel.ServiceTypes = await _context.ServiceTypes
                    .Where(st => !st.IsDeleted && st.IsActive)
                    .OrderBy(st => st.ServiceName)
                    .Select(st => new SelectListItem
                    {
                        Value = st.Id.ToString(),
                        Text = $"{st.ServiceName} (Rs. {st.LabourCharge})"
                    })
                    .ToListAsync();

                viewModel.ServiceTypeOptions = await _context.ServiceTypes
                    .Where(st => !st.IsDeleted && st.IsActive)
                    .OrderBy(st => st.ServiceName)
                    .Select(st => new ServiceTypeOption
                    {
                        Id = st.Id,
                        Name = st.ServiceName,
                        LabourCharge = st.LabourCharge
                    })
                    .ToListAsync();

                if (viewModel.Services == null || !viewModel.Services.Any())
                {
                    viewModel.Services = booking.ServiceBookingDetails
                        .Where(d => !d.IsDeleted)
                        .Select(d => new ServiceBookingDetailInputViewModel
                        {
                            Id = d.Id,
                            ServiceTypeId = d.ServiceTypeId,
                            Price = d.Price,
                            Quantity = (int)d.Quantity,
                        })
                        .ToList();
                }

                viewModel.TotalAmount = booking.TotalAmount;

                return viewModel;
            }

            // ─── UPDATE ────────────────────────────────────────────────────
            public async Task UpdateAsync(ServiceBookingUpdateViewModel model)
            {
                var booking = await _context.ServiceBookings
                    .Include(b => b.ServiceBookingDetails)
                    .FirstOrDefaultAsync(b => b.Id == model.Id && !b.IsDeleted)
                    ?? throw new KeyNotFoundException("Booking not found.");

                if (!await _context.Customers.AnyAsync(c => c.Id == model.CustomerId && !c.IsDeleted && c.IsActive))
                    throw new InvalidOperationException("Selected customer does not exist or is inactive.");

                if (!await _context.Vehicles.AnyAsync(v => v.Id == model.VehicleId && !v.IsDeleted && v.IsActive))
                    throw new InvalidOperationException("Selected vehicle does not exist or is inactive.");

                if (model.MechanicId.HasValue && model.MechanicId != booking.MechanicId)
                {
                    var mechanic = await _context.Mechanics
                        .FirstOrDefaultAsync(m => m.Id == model.MechanicId.Value && m.IsAvailable && !m.IsDeleted);

                    if (mechanic is null)
                        throw new InvalidOperationException("Selected mechanic is not available.");
                }

                if (model.Services == null || !model.Services.Any())
                    throw new InvalidOperationException("At least one service must be selected.");

                _mapper.Map(model, booking);
                booking.TotalAmount = model.Services.Sum(s => s.Price * s.Quantity);
                booking.ModifiedOn = DateTime.UtcNow;

                var existingDetails = booking.ServiceBookingDetails.ToList();
                var submittedIds = model.Services.Where(s => s.Id > 0).Select(s => s.Id).ToList();

                // Soft-delete removed details
                foreach (var detail in existingDetails.Where(d => !submittedIds.Contains(d.Id)))
                {
                    detail.IsDeleted = true;
                    detail.ModifiedOn = DateTime.UtcNow;
                }

                // Update or add details
                foreach (var detailVm in model.Services)
                {
                    if (detailVm.Id > 0)
                    {
                        var detail = existingDetails.FirstOrDefault(d => d.Id == detailVm.Id);
                        if (detail != null)
                        {
                            detail.ServiceTypeId = detailVm.ServiceTypeId;
                            detail.Price = detailVm.Price;
                            detail.Quantity = detailVm.Quantity;
                            detail.TotalAmount = detailVm.TotalAmount;
                            detail.ModifiedOn = DateTime.UtcNow;
                            detail.IsDeleted = false;
                            detail.IsActive = true;
                        }
                    }
                    else
                    {
                        var newDetail = new ServiceBookingDetail
                        {
                            ServiceBookingId = booking.Id,
                            ServiceTypeId = detailVm.ServiceTypeId,
                            Price = detailVm.Price,
                            Quantity = detailVm.Quantity,
                            TotalAmount = detailVm.TotalAmount,
                            CreatedOn = DateTime.UtcNow,
                            IsDeleted = false,
                            IsActive = true
                        };
                        booking.ServiceBookingDetails.Add(newDetail);
                    }
                }

                // Recalculate total
                booking.TotalAmount = booking.ServiceBookingDetails
                    .Where(d => !d.IsDeleted)
                    .Sum(d => d.TotalAmount);

                await _context.SaveChangesAsync();
            }

            // ─── UPDATE STATUS ────────────────────────────────────────────
            public async Task<bool> UpdateStatusAsync(int bookingId, BookingStatus newStatus)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var booking = await _context.ServiceBookings
                        .Include(b => b.JobCard)
                        .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

                    if (booking == null)
                        return false;

                    var validTransitions = new Dictionary<BookingStatus, BookingStatus[]>
                    {
                        [BookingStatus.Pending] = new[] { BookingStatus.InProgress, BookingStatus.Cancelled },
                        [BookingStatus.InProgress] = new[] { BookingStatus.Completed, BookingStatus.Cancelled },
                        [BookingStatus.Completed] = Array.Empty<BookingStatus>(),
                        [BookingStatus.Cancelled] = Array.Empty<BookingStatus>()
                    };

                    if (!validTransitions[booking.Status].Contains(newStatus))
                        throw new InvalidOperationException($"Cannot move from {booking.Status} to {newStatus}.");

                    booking.Status = newStatus;
                    booking.ModifiedOn = DateTime.UtcNow;

                    if (booking.JobCard != null)
                    {
                        booking.JobCard.Status = newStatus.ToString();
                        booking.JobCard.ModifiedOn = DateTime.UtcNow;

                        if (newStatus == BookingStatus.Completed)
                        {
                            booking.JobCard.ActualCost = booking.TotalAmount;
                            booking.DeliveryDate = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        if (newStatus == BookingStatus.InProgress || newStatus == BookingStatus.Completed)
                        {
                            var lastJobCard = await _context.JobCards
                                .OrderByDescending(jc => jc.Id)
                                .FirstOrDefaultAsync();

                            int nextNumber = (lastJobCard?.Id ?? 0) + 1;
                            var jobCard = new JobCard
                            {
                                BookingId = booking.Id,
                                JobCardNumber = $"JC{nextNumber:D4}",
                                InspectionDate = DateTime.UtcNow,
                                Status = newStatus.ToString(),
                                EstimatedCost = booking.TotalAmount,
                                ActualCost = newStatus == BookingStatus.Completed ? booking.TotalAmount : 0,
                                CreatedOn = DateTime.UtcNow,
                                IsDeleted = false,
                                IsActive = true
                            };

                            _context.JobCards.Add(jobCard);
                            booking.JobCard = jobCard;
                        }
                    }

                    if (newStatus == BookingStatus.Cancelled)
                    {
                        booking.Remarks = string.IsNullOrEmpty(booking.Remarks)
                            ? "Booking cancelled"
                            : $"{booking.Remarks} - Cancelled";
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            // ─── ASSIGN MECHANIC ──────────────────────────────────────────
            public async Task<bool> AssignMechanicAsync(int bookingId, int mechanicId)
            {
                var booking = await _context.ServiceBookings
                    .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted)
                    ?? throw new KeyNotFoundException("Booking not found.");

                if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.InProgress)
                    throw new InvalidOperationException($"Cannot assign mechanic to a booking with status {booking.Status}.");

                var mechanic = await _context.Mechanics
                    .FirstOrDefaultAsync(m => m.Id == mechanicId && m.IsAvailable && !m.IsDeleted);

                if (mechanic is null)
                    throw new InvalidOperationException("Selected mechanic is not available.");

                booking.MechanicId = mechanicId;
                booking.ModifiedOn = DateTime.UtcNow;

                if (booking.Status == BookingStatus.Pending)
                    booking.Status = BookingStatus.InProgress;

                await _context.SaveChangesAsync();
                return true;
            }

            // ─── CANCEL BOOKING ───────────────────────────────────────────
            public async Task<bool> CancelBookingAsync(int id, string? cancellationReason = null)
            {
                var booking = await _context.ServiceBookings
                    .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted)
                    ?? throw new KeyNotFoundException("Booking not found.");

                if (booking.Status == BookingStatus.Completed)
                    throw new InvalidOperationException("Cannot cancel a completed booking.");

                booking.Status = BookingStatus.Cancelled;
                booking.Remarks = cancellationReason ?? "Booking cancelled by customer";
                booking.ModifiedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }

            // ─── DELETE (SOFT DELETE) ─────────────────────────────────────
            public async Task<bool> DeleteAsync(int id)
            {
                var booking = await _context.ServiceBookings
                    .Include(b => b.JobCard)
                    .Include(b => b.Invoice)
                    .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

                if (booking is null) return false;

                if (booking.JobCard != null && !booking.JobCard.IsDeleted)
                    throw new InvalidOperationException("Cannot delete booking with an existing Job Card.");

                if (booking.Invoice != null && !booking.Invoice.IsDeleted)
                    throw new InvalidOperationException("Cannot delete booking with an existing Invoice.");

                booking.IsDeleted = true;
                booking.IsActive = false;
                booking.ModifiedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }

            // ─── RESTORE ───────────────────────────────────────────────────
            public async Task<bool> RestoreAsync(int id)
            {
                var booking = await _context.ServiceBookings
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(b => b.Id == id && b.IsDeleted);

                if (booking is null) return false;

                booking.IsDeleted = false;
                booking.IsActive = true;
                booking.ModifiedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }

            // ─── STATISTICS ──────────────────────────────────────────────
            public async Task<ServiceBookingStatisticsViewModel> GetStatisticsAsync(
                DateTime? dateFrom = null,
                DateTime? dateTo = null)
            {
                var query = _context.ServiceBookings
                    .Include(b => b.ServiceBookingDetails)
                        .ThenInclude(d => d.ServiceType)
                    .Include(b => b.Mechanic)
                    .Where(b => !b.IsDeleted)
                    .AsQueryable();

                if (dateFrom.HasValue)
                    query = query.Where(b => b.BookingDate >= dateFrom.Value);

                if (dateTo.HasValue)
                {
                    var endDate = dateTo.Value.Date.AddDays(1);
                    query = query.Where(b => b.BookingDate < endDate);
                }

                var bookings = await query.ToListAsync();

                var stats = new ServiceBookingStatisticsViewModel
                {
                    TotalBookings = bookings.Count,
                    Pending = bookings.Count(b => b.Status == BookingStatus.Pending),
                    InProgress = bookings.Count(b => b.Status == BookingStatus.InProgress),
                    Completed = bookings.Count(b => b.Status == BookingStatus.Completed),
                    Cancelled = bookings.Count(b => b.Status == BookingStatus.Cancelled),
                    TotalRevenue = bookings.Where(b => b.Status == BookingStatus.Completed).Sum(b => b.TotalAmount),
                    TodaysBookings = bookings.Count(b => b.BookingDate.Date == DateTime.UtcNow.Date),
                    TodaysRevenue = bookings
                        .Where(b => b.Status == BookingStatus.Completed && b.BookingDate.Date == DateTime.UtcNow.Date)
                        .Sum(b => b.TotalAmount),
                    ServiceTypeDistribution = bookings
                        .SelectMany(b => b.ServiceBookingDetails)
                        .Where(d => !d.IsDeleted)
                        .GroupBy(d => d.ServiceType.ServiceName)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    MechanicWorkload = bookings
                        .Where(b => b.Mechanic != null && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.InProgress))
                        .GroupBy(b => b.Mechanic.FullName)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                stats.AverageBookingValue = stats.TotalBookings > 0
                    ? stats.TotalRevenue / stats.TotalBookings
                    : 0;

                return stats;
            }

            // ─── ADDITIONAL QUERIES ──────────────────────────────────────
            public async Task<List<ServiceBookingListViewModel>> GetBookingsByCustomerAsync(int customerId)
            {
                var bookings = await _context.ServiceBookings
                    .Include(b => b.Customer)
                    .Include(b => b.Vehicle)
                    .Include(b => b.Mechanic)
                    .Include(b => b.ServiceBookingDetails)
                        .ThenInclude(d => d.ServiceType)
                    .Include(b => b.JobCard)
                    .Include(b => b.Invoice)
                    .Where(b => b.CustomerId == customerId && !b.IsDeleted)
                    .OrderByDescending(b => b.BookingDate)
                    .ToListAsync();

                return _mapper.Map<List<ServiceBookingListViewModel>>(bookings);
            }

            public async Task<List<ServiceBookingListViewModel>> GetBookingsByVehicleAsync(int vehicleId)
            {
                var bookings = await _context.ServiceBookings
                    .Include(b => b.Customer)
                    .Include(b => b.Vehicle)
                    .Include(b => b.Mechanic)
                    .Include(b => b.ServiceBookingDetails)
                        .ThenInclude(d => d.ServiceType)
                    .Include(b => b.JobCard)
                    .Include(b => b.Invoice)
                    .Where(b => b.VehicleId == vehicleId && !b.IsDeleted)
                    .OrderByDescending(b => b.BookingDate)
                    .ToListAsync();

                return _mapper.Map<List<ServiceBookingListViewModel>>(bookings);
            }

            public async Task<Dictionary<BookingStatus, int>> GetStatusCountsAsync()
            {
                var counts = await _context.ServiceBookings
                    .Where(b => !b.IsDeleted)
                    .GroupBy(b => b.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(g => g.Status, g => g.Count);

                foreach (BookingStatus status in Enum.GetValues(typeof(BookingStatus)))
                {
                    if (!counts.ContainsKey(status))
                        counts[status] = 0;
                }

                return counts;
            }

            public async Task<bool> HasActiveBookingsAsync(int customerId)
            {
                return await _context.ServiceBookings
                    .AnyAsync(b => b.CustomerId == customerId
                        && !b.IsDeleted
                        && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.InProgress));
            }

            public async Task<bool> IsBookingNumberUniqueAsync(string bookingNumber, int? excludeId = null)
            {
                var query = _context.ServiceBookings
                    .Where(b => b.BookingNumber == bookingNumber && !b.IsDeleted);

                if (excludeId.HasValue)
                    query = query.Where(b => b.Id != excludeId.Value);

                return !await query.AnyAsync();
            }

            public async Task<List<ServiceBookingListViewModel>> GetBookingsWithoutInvoiceAsync()
            {
                var bookings = await _context.ServiceBookings
                    .Include(b => b.Customer)
                    .Include(b => b.Vehicle)
                    .Include(b => b.Mechanic)
                    .Include(b => b.ServiceBookingDetails)
                        .ThenInclude(d => d.ServiceType)
                    .Include(b => b.Invoice)
                    .Where(b => !b.IsDeleted && b.Invoice == null)
                    .OrderByDescending(b => b.BookingDate)
                    .ToListAsync();

                return _mapper.Map<List<ServiceBookingListViewModel>>(bookings);
            }

            public async Task<List<SelectListItem>> GetVehiclesByCustomerAsync(int customerId)
            {
                return await _context.Vehicles
                    .Where(v => v.CustomerId == customerId && !v.IsDeleted && v.IsActive)
                    .OrderBy(v => v.RegistrationNumber)
                    .Select(v => new SelectListItem
                    {
                        Value = v.Id.ToString(),
                        Text = $"{v.RegistrationNumber} - {v.VehicleName}"
                    })
                    .ToListAsync();
            }
        }
    }