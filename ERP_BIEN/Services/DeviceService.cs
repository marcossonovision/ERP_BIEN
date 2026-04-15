using ERP_BIEN.Data;
using ERP_BIEN.Models;
using ERP_BIEN.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_BIEN.Services
{
    public class DeviceService
    {
        private readonly AppDbContext _context;

        public DeviceService(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // LISTADO + FILTROS + PAGINACIÓN
        // ============================================================
        public async Task<(List<Device> Devices, int TotalPages)> GetDevicesAsync(
            int pageNumber,
            int pageSize,
            string deviceType,
            StatusDevice? status,
            int? userId,
            string hostname,
            string model,
            string sn,
            DateTime? manufacturingFrom,
            DateTime? manufacturingTo,
            DateTime? useFrom,
            DateTime? useTo)
        {
            var query = _context.Devices
                .Include(d => d.User)
                .AsQueryable();

            // TIPO
            if (!string.IsNullOrWhiteSpace(deviceType))
            {
                query = deviceType switch
                {
                    "Computer" => query.Where(d => d is Computer),
                    "Phone" => query.Where(d => d is Phone),
                    "Screen" => query.Where(d => d is Screen),
                    "DockStation" => query.Where(d => d is DockStation),
                    _ => query
                };
            }

            // STATUS
            if (status.HasValue)
                query = query.Where(d => d.Status == status.Value);

            // USUARIO
            if (userId.HasValue)
                query = query.Where(d => d.UserId == userId.Value);

            // TEXTO
            if (!string.IsNullOrWhiteSpace(hostname))
                query = query.Where(d => d.Hostname.Contains(hostname));

            if (!string.IsNullOrWhiteSpace(model))
                query = query.Where(d => d.Model.Contains(model));

            if (!string.IsNullOrWhiteSpace(sn))
                query = query.Where(d => d.SN.Contains(sn));

            // FECHAS
            if (manufacturingFrom.HasValue)
                query = query.Where(d => d.ManufacturingDate >= manufacturingFrom.Value);

            if (manufacturingTo.HasValue)
                query = query.Where(d => d.ManufacturingDate <= manufacturingTo.Value);

            if (useFrom.HasValue)
                query = query.Where(d => d.UseDate >= useFrom.Value);

            if (useTo.HasValue)
                query = query.Where(d => d.UseDate <= useTo.Value);

            // PAGINACIÓN
            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            if (pageNumber < 1) pageNumber = 1;
            if (totalPages == 0) pageNumber = 1;
            else if (pageNumber > totalPages) pageNumber = totalPages;

            var devices = await query
                .OrderBy(d => d.Hostname)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (devices, totalPages);
        }

        // ============================================================
        // CREATE
        // ============================================================
        public async Task CreateAsync(
            string deviceType,
            string hostname,
            string sn,
            string model,
            int numberOfDevice,
            DateTime? manufacturingDate,
            StatusDevice status,
            string comment,
            DateTime? useDate,
            int? userId)
        {
            Device device = deviceType switch
            {
                "Computer" => new Computer(),
                "Phone" => new Phone(),
                "Screen" => new Screen(),
                "DockStation" => new DockStation(),
                _ => null
            };

            if (device == null)
                return;

            device.Hostname = hostname?.Trim();
            device.SN = sn?.Trim();
            device.Model = model?.Trim();
            device.NumberOfDevice = numberOfDevice;
            device.ManufacturingDate = manufacturingDate;
            device.Status = status;
            device.Comment = comment?.Trim();
            device.UseDate = useDate;
            device.UserId = userId;

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();
        }

        // ============================================================
        // EDIT
        // ============================================================
        public async Task EditAsync(
            int id,
            string hostname,
            string sn,
            string model,
            int numberOfDevice,
            DateTime? manufacturingDate,
            StatusDevice status,
            string comment,
            DateTime? useDate,
            int? userId)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
                return;

            device.Hostname = hostname?.Trim();
            device.SN = sn?.Trim();
            device.Model = model?.Trim();
            device.NumberOfDevice = numberOfDevice;
            device.ManufacturingDate = manufacturingDate;
            device.Status = status;
            device.Comment = comment?.Trim();
            device.UseDate = useDate;
            device.UserId = userId;

            _context.Devices.Update(device);
            await _context.SaveChangesAsync();
        }

        // ============================================================
        // DELETE
        // ============================================================
        public async Task DeleteAsync(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device != null)
            {
                _context.Devices.Remove(device);
                await _context.SaveChangesAsync();
            }
        }

        // ============================================================
        // USERS PARA LOS SELECTS
        // ============================================================
        public async Task<List<User>> GetUsersAsync()
        {
            return await _context.Users
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<Device> GetByIdAsync(int id)
        {
            return await _context.Devices
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

    }
}
