using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.ViewModels.Common; // ← ADD THIS
using Vehicle_Service_Management_System.Application.ViewModels.JobCard;
using Vehicle_Service_Management_System.Domain.Entities;
using Vehicle_Service_Management_System.Domain.Enums;
using Vehicle_Service_Management_System.Infrastructure.Data;

namespace Vehicle_Service_Management_System.Application.Services
{
    public class JobCardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public JobCardService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // ─── GET PAGED (ADD THIS METHOD) ──────────────────────────────
        public async Task<PagedResult<JobCardListViewModel>> GetPagedAsync(
            string? search = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.JobCards
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Customer)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Vehicle)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Mechanic)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.ServiceBookingDetails)
                        .ThenInclude(d => d.ServiceType)
                .Where(j => !j.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(j =>
                    j.JobCardNumber.Contains(search) ||
                    j.ServiceBooking.Customer.FullName.Contains(search) ||
                    j.ServiceBooking.Vehicle.RegistrationNumber.Contains(search) ||
                    j.ServiceBooking.BookingNumber.Contains(search) ||
                    (j.ServiceBooking.Vehicle.VehicleName != null && j.ServiceBooking.Vehicle.VehicleName.Contains(search))
                );
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(j => j.JobCardNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<JobCardListViewModel>
            {
                Items = _mapper.Map<List<JobCardListViewModel>>(items),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        // ─── GET ALL ───
        public async Task<List<JobCardListViewModel>> GetAllAsync()
        {
            var items = await _context.JobCards
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Customer)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Vehicle)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Mechanic)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.ServiceBookingDetails)
                        .ThenInclude(d => d.ServiceType)
                .Where(j => !j.IsDeleted)
                .OrderBy(j => j.JobCardNumber)
                .ToListAsync();

            return _mapper.Map<List<JobCardListViewModel>>(items);
        }

        // ─── GET DELETED ───
        public async Task<List<JobCardListViewModel>> GetDeletedAsync()
        {
            var items = await _context.JobCards
                .IgnoreQueryFilters()
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Customer)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Vehicle)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.ServiceBookingDetails)
                        .ThenInclude(d => d.ServiceType)
                .Where(j => j.IsDeleted)
                .OrderBy(j => j.JobCardNumber)
                .ToListAsync();

            return _mapper.Map<List<JobCardListViewModel>>(items);
        }

        // ─── GET BY ID ───
        public async Task<JobCardDetailsViewModel?> GetByIdAsync(int id)
        {
            var entity = await _context.JobCards
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Customer)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Vehicle)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Mechanic)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.ServiceBookingDetails)
                        .ThenInclude(d => d.ServiceType)
                .FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted);

            return entity is null ? null : _mapper.Map<JobCardDetailsViewModel>(entity);
        }

        // ─── GET BY BOOKING ID ───
        public async Task<JobCardDetailsViewModel?> GetByBookingIdAsync(int bookingId)
        {
            var entity = await _context.JobCards
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Customer)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Vehicle)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Mechanic)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.ServiceBookingDetails)
                        .ThenInclude(d => d.ServiceType)
                .FirstOrDefaultAsync(j => j.BookingId == bookingId && !j.IsDeleted);

            return entity is null ? null : _mapper.Map<JobCardDetailsViewModel>(entity);
        }

        // ─── CREATE ───
        public async Task<int> CreateAsync(JobCardCreateViewModel model)
        {
            if (await _context.JobCards.AnyAsync(j => j.BookingId == model.BookingId && !j.IsDeleted))
                throw new InvalidOperationException("This booking already has a Job Card.");

            var jobCard = _mapper.Map<JobCard>(model);

            var lastJobCard = await _context.JobCards
                .OrderByDescending(j => j.Id)
                .FirstOrDefaultAsync();

            int nextNumber = (lastJobCard?.Id ?? 0) + 1;
            jobCard.JobCardNumber = $"JC{nextNumber:D4}";
            jobCard.ActualCost = model.ActualCost ?? 0;
            jobCard.Status = "InProgress";
            jobCard.CreatedOn = DateTime.UtcNow;
            jobCard.IsDeleted = false;

            _context.JobCards.Add(jobCard);
            await _context.SaveChangesAsync();

            var booking = await _context.ServiceBookings.FindAsync(model.BookingId);
            if (booking != null)
            {
                booking.Status = BookingStatus.InProgress;
                await _context.SaveChangesAsync();
            }

            return jobCard.Id;
        }

        // ─── UPDATE ───
        public async Task UpdateAsync(JobCardUpdateViewModel model)
        {
            var entity = await _context.JobCards
                .FirstOrDefaultAsync(j => j.Id == model.Id && !j.IsDeleted)
                ?? throw new KeyNotFoundException("Job card not found.");

            if (entity.BookingId != model.BookingId)
                throw new InvalidOperationException("Cannot change the associated booking.");

            _mapper.Map(model, entity);
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ─── UPDATE STATUS ───
        public async Task UpdateStatusAsync(JobCardStatusUpdateViewModel model)
        {
            var entity = await _context.JobCards
                .FirstOrDefaultAsync(j => j.Id == model.Id && !j.IsDeleted)
                ?? throw new KeyNotFoundException("Job card not found.");

            entity.Status = model.Status;

            if (!string.IsNullOrWhiteSpace(model.WorkPerformed))
                entity.WorkPerformed = model.WorkPerformed;

            if (model.ActualCost.HasValue)
                entity.ActualCost = model.ActualCost.Value;

            if (model.Status == "Completed" && !model.ActualCost.HasValue)
                entity.ActualCost = entity.EstimatedCost;

            entity.ModifiedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // ─── SOFT DELETE ───
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await _context.JobCards.FindAsync(id);
            if (entity is null) return false;

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // ─── RESTORE ───
        public async Task<bool> RestoreAsync(int id)
        {
            var entity = await _context.JobCards
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(j => j.Id == id && j.IsDeleted);

            if (entity is null) return false;

            entity.IsDeleted = false;
            entity.ModifiedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // ─── HARD DELETE ───
        public async Task<bool> HardDeleteAsync(int id)
        {
            var entity = await _context.JobCards
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(j => j.Id == id);

            if (entity is null) return false;

            _context.JobCards.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // ─── STATUS COUNTS ───
        public async Task<Dictionary<string, int>> GetStatusCountsAsync()
        {
            return await _context.JobCards
                .Where(j => !j.IsDeleted)
                .GroupBy(j => j.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Status, g => g.Count);
        }

        // ─── GET PRINT MODEL ───
        public async Task<JobCardPrintViewModel?> GetPrintAsync(int id)
        {
            var entity = await _context.JobCards
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Customer)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Vehicle)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.Mechanic)
                .Include(j => j.ServiceBooking)
                    .ThenInclude(sb => sb.ServiceBookingDetails)
                        .ThenInclude(d => d.ServiceType)
                .FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted);

            return entity == null ? null : _mapper.Map<JobCardPrintViewModel>(entity);
        }

        // ─── BUILD FORM FOR CREATE ───
        public async Task<JobCardCreateViewModel> BuildCreateFormAsync()
        {
            var model = new JobCardCreateViewModel();

            var bookedIds = await _context.JobCards
                .Where(j => !j.IsDeleted)
                .Select(j => j.BookingId)
                .ToListAsync();

            model.AvailableBookings = await _context.ServiceBookings
                .Include(sb => sb.Customer)
                .Include(sb => sb.Vehicle)
                .Where(sb => !sb.IsDeleted && !bookedIds.Contains(sb.Id))
                .OrderBy(sb => sb.BookingNumber)
                .Select(sb => new SelectListItem
                {
                    Value = sb.Id.ToString(),
                    Text = $"{sb.BookingNumber} - {sb.Customer.FullName} ({sb.Vehicle.RegistrationNumber})"
                })
                .ToListAsync();

            return model;
        }
    }
}