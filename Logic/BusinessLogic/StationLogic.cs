using Database.AppDBContext;
using Database.Entities;
using Logic.BusinessLogicInterface;
using Logic.DTO;
using Logic.Model;
using Microsoft.EntityFrameworkCore;
using static Logic.Enums.Enums;

namespace Logic.BusinessLogic
{
    public class StationLogic : IStationLogic
    {
        private readonly ApplicationDbContext _context;

        public StationLogic(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResultModel<Station>> CreateAsync(StationDTO dto)
        {
            try
            {
                var existing = await _context.Stations.FirstOrDefaultAsync(s => s.StationName.ToLower() == dto.StationName.ToLower());
                if (existing != null)
                {
                    return new ResultModel<Station> { ResultId = EResultValue.Fail, Message = $"A station named '{dto.StationName}' already exists." };
                }

                if (dto.Latitude == null || dto.Longitude == null)
                {
                    return new ResultModel<Station> { ResultId = EResultValue.Fail, Message = "Please pick the station's location on the map." };
                }

                var station = new Station
                {
                    StationName = dto.StationName,
                    Province = dto.Province,
                    Address = dto.Address,
                    ContactNumber = dto.ContactNumber,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude
                };

                _context.Stations.Add(station);
                await _context.SaveChangesAsync();

                return new ResultModel<Station> { ResultId = EResultValue.Success, Message = "Station created successfully.", Data = station };
            }
            catch (Exception ex)
            {
                return new ResultModel<Station> { ResultId = EResultValue.Fail, Message = $"An error occurred while creating the station: {ex.Message}" };
            }
        }

        public async Task<IEnumerable<Station>> GetAllAsync()
        {
            return await _context.Stations.OrderBy(s => s.StationName).ToListAsync();
        }

        public async Task<PagedResult<Station>> GetAllPagedAsync(int page, int pageSize, string? search = null, CancellationToken ct = default)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var query = _context.Stations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(s => s.StationName.Contains(term) || s.Province.Contains(term) || s.Address.Contains(term));
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderBy(s => s.StationName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Station> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
        }

        public async Task<Station?> GetByIdAsync(long id)
        {
            return await _context.Stations.FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
