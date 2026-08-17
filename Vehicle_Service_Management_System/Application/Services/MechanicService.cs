using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.ViewModels.Common;
using Vehicle_Service_Management_System.Application.ViewModels.Mechanic;
using Vehicle_Service_Management_System.Domain.Entities;
using Vehicle_Service_Management_System.Domain.Enums;
using Vehicle_Service_Management_System.Infrastructure.Data;

namespace Vehicle_Service_Management_System.Application.Services
{
    public class MechanicService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public MechanicService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<MechanicListViewModel>> GetAllAsync(bool? availableOnly = null)
        {
            var query = _context.Mechanics
                .Where(m => !m.IsDeleted)
                .AsQueryable();

            if (availableOnly == true)
                query = query.Where(m => m.IsAvailable);

            var items = await query
                .OrderBy(m => m.FullName)
                .ToListAsync();

            return _mapper.Map<List<MechanicListViewModel>>(items);
        }

        public async Task<PagedResult<MechanicListViewModel>> GetPagedAsync(
            string? searchTerm = null,
            string? specialization = null,
            bool? isAvailable = null,
            int? minExperience = null,
            int? maxExperience = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.Mechanics
                .Include(m => m.ServiceBookings)
                .Where(m => !m.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(m =>
                    m.FullName.Contains(searchTerm) ||
                    m.PhoneNumber.Contains(searchTerm) ||
                    (m.Email != null && m.Email.Contains(searchTerm)) ||
                    (m.Specialization != null && m.Specialization.Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(specialization))
            {
                query = query.Where(m => m.Specialization == specialization);
            }

            if (isAvailable.HasValue)
            {
                query = query.Where(m => m.IsAvailable == isAvailable.Value);
            }

            if (minExperience.HasValue)
            {
                query = query.Where(m => m.ExperienceYears >= minExperience.Value);
            }

            if (maxExperience.HasValue)
            {
                query = query.Where(m => m.ExperienceYears <= maxExperience.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(m => m.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<MechanicListViewModel>
            {
                Items = _mapper.Map<List<MechanicListViewModel>>(items),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        // ✅ FIXED: Removed invalid .ThenInclude(sb => sb.ServiceType)
        public async Task<MechanicDetailsViewModel?> GetByIdAsync(int id)
        {
            var entity = await _context.Mechanics
                .Include(m => m.ServiceBookings)
                    .ThenInclude(sb => sb.Customer)
                .Include(m => m.ServiceBookings)
                    .ThenInclude(sb => sb.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (entity is null)
                return null;

            var viewModel = _mapper.Map<MechanicDetailsViewModel>(entity);
            viewModel.TotalBookings = entity.ServiceBookings.Count;
            viewModel.ActiveBookings = entity.ServiceBookings
                .Count(sb => sb.Status == BookingStatus.InProgress || sb.Status == BookingStatus.Pending);

            return viewModel;
        }

        public async Task<MechanicFormViewModel?> GetForEditAsync(int id)
        {
            var entity = await _context.Mechanics
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            return entity is null ? null : _mapper.Map<MechanicFormViewModel>(entity);
        }

        public async Task<int> CreateAsync(MechanicFormViewModel model)
        {
            if (await _context.Mechanics.AnyAsync(m =>
                m.PhoneNumber == model.PhoneNumber && !m.IsDeleted))
            {
                throw new InvalidOperationException("A mechanic with this phone number already exists.");
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                if (await _context.Mechanics.AnyAsync(m =>
                    m.Email == model.Email && !m.IsDeleted))
                {
                    throw new InvalidOperationException("A mechanic with this email already exists.");
                }
            }

            var entity = _mapper.Map<Domain.Entities.Mechanic>(model);
            entity.CreatedOn = DateTime.UtcNow;
            entity.IsDeleted = false;

            _context.Mechanics.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task UpdateAsync(MechanicFormViewModel model)
        {
            var entity = await _context.Mechanics
                .FirstOrDefaultAsync(m => m.Id == model.Id && !m.IsDeleted)
                ?? throw new KeyNotFoundException("Mechanic not found.");

            if (await _context.Mechanics.AnyAsync(m =>
                m.PhoneNumber == model.PhoneNumber &&
                m.Id != model.Id &&
                !m.IsDeleted))
            {
                throw new InvalidOperationException("A mechanic with this phone number already exists.");
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                if (await _context.Mechanics.AnyAsync(m =>
                    m.Email == model.Email &&
                    m.Id != model.Id &&
                    !m.IsDeleted))
                {
                    throw new InvalidOperationException("A mechanic with this email already exists.");
                }
            }

            _mapper.Map(model, entity);
            entity.ModifiedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateAvailabilityAsync(int id, bool isAvailable)
        {
            var entity = await _context.Mechanics
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (entity is null)
                return false;

            entity.IsAvailable = isAvailable;
            entity.ModifiedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task ToggleAvailabilityAsync(int id)
        {
            var entity = await _context.Mechanics
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted)
                ?? throw new KeyNotFoundException("Mechanic not found.");

            entity.IsAvailable = !entity.IsAvailable;
            entity.ModifiedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await _context.Mechanics
                .Include(m => m.ServiceBookings)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (entity is null)
                return false;

            if (entity.ServiceBookings.Any(sb =>
                sb.Status == BookingStatus.Pending ||
                sb.Status == BookingStatus.InProgress))
            {
                throw new InvalidOperationException(
                    "Cannot delete a mechanic with active bookings. Please reassign or complete the bookings first.");
            }

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var entity = await _context.Mechanics
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id && m.IsDeleted);

            if (entity is null)
                return false;

            entity.IsDeleted = false;
            entity.ModifiedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HardDeleteAsync(int id)
        {
            var entity = await _context.Mechanics
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entity is null)
                return false;

            _context.Mechanics.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<MechanicAvailabilityViewModel>> GetAvailableMechanicsAsync(
            string? specialization = null,
            int? maxBookings = null)
        {
            var query = _context.Mechanics
                .Include(m => m.ServiceBookings)
                .Where(m => m.IsAvailable && !m.IsDeleted);

            if (!string.IsNullOrWhiteSpace(specialization))
            {
                query = query.Where(m => m.Specialization == specialization);
            }

            var mechanics = await query.ToListAsync();

            var result = mechanics.Select(m => new MechanicAvailabilityViewModel
            {
                Id = m.Id,
                FullName = m.FullName,
                Specialization = m.Specialization,
                IsAvailable = m.IsAvailable,
                CurrentBookings = m.ServiceBookings.Count(sb =>
                    sb.Status == BookingStatus.InProgress ||
                    sb.Status == BookingStatus.Pending)
            }).ToList();

            if (maxBookings.HasValue)
            {
                result = result.Where(m => m.CurrentBookings < maxBookings.Value).ToList();
            }

            return result;
        }

        public async Task<List<string>> GetSpecializationsAsync()
        {
            return await _context.Mechanics
                .Where(m => !m.IsDeleted && m.Specialization != null)
                .Select(m => m.Specialization!)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }

        public async Task<MechanicStatisticsViewModel> GetStatisticsAsync()
        {
            var mechanics = await _context.Mechanics
                .Include(m => m.ServiceBookings)
                .Where(m => !m.IsDeleted)
                .ToListAsync();

            var stats = new MechanicStatisticsViewModel
            {
                Total = mechanics.Count,
                Available = mechanics.Count(m => m.IsAvailable),
                Busy = mechanics.Count(m => !m.IsAvailable)
            };

            if (mechanics.Any())
            {
                stats.AverageExperience = mechanics.Average(m => m.ExperienceYears);
                stats.AverageSalary = mechanics.Average(m => m.Salary);
                stats.TotalSalaryCost = mechanics.Sum(m => m.Salary);

                stats.SpecializationDistribution = mechanics
                    .Where(m => m.Specialization != null)
                    .GroupBy(m => m.Specialization!)
                    .ToDictionary(g => g.Key, g => g.Count());
            }

            return stats;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Mechanics
                .AnyAsync(m => m.Id == id && !m.IsDeleted);
        }

        public async Task<bool> IsPhoneNumberUniqueAsync(string phoneNumber, int? excludeId = null)
        {
            var query = _context.Mechanics
                .Where(m => m.PhoneNumber == phoneNumber && !m.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(m => m.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            var query = _context.Mechanics
                .Where(m => m.Email == email && !m.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(m => m.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<List<MechanicListViewModel>> GetDeletedAsync()
        {
            var mechanics = await _context.Mechanics
                .IgnoreQueryFilters()
                .Where(x => x.IsDeleted)
                .OrderByDescending(x => x.ModifiedOn)
                .ToListAsync();

            return _mapper.Map<List<MechanicListViewModel>>(mechanics);
        }
    }
}