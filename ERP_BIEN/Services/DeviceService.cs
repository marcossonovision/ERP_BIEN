using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            device.Hostname = hostname;
            device.SN = sn;
            device.Model = model;
            device.NumberOfDevice = numberOfDevice;
            device.ManufacturingDate = manufacturingDate;
            device.Status = status;
            device.Comment = comment;
            device.UseDate = useDate;
            device.UserId = userId;

            NormalizeDevice(device);

            _context.Devices.Add(device);

            // ✅ Si se crea asignado, abrimos histórico en la MISMA transacción
            if (userId.HasValue)
            {
                // aún no tenemos Id hasta guardar, así que primero SaveChanges
                await _context.SaveChangesAsync();

                await OpenHistoryAsync(device.Id, userId.Value);
                await _context.SaveChangesAsync();
                return;
            }

            await _context.SaveChangesAsync();
        }

        // ============================================================
        // EDIT (CORREGIDO: si cambia UserId => histórico)
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

            var previousUserId = device.UserId;

            // Datos normales
            device.Hostname = hostname;
            device.SN = sn;
            device.Model = model;
            device.NumberOfDevice = numberOfDevice;
            device.ManufacturingDate = manufacturingDate;
            device.Status = status;
            device.Comment = comment;
            device.UseDate = useDate;

            // ✅ Si cambia asignación => histórico (RF-025 / RF-071) 
            if (previousUserId != userId)
            {
                // Cerrar histórico activo si lo hubiera
                await CloseActiveHistoryAsync(id);

                // Asignar/Desasignar en entidad
                device.UserId = userId;

                // Abrir nuevo histórico si hay usuario
                if (userId.HasValue)
                {
                    await OpenHistoryAsync(id, userId.Value);
                }
            }
            else
            {
                // no cambió usuario
                device.UserId = userId;
            }

            NormalizeDevice(device);

            // ✅ operación atómica (RNF-024) 
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

        // ============================================================
        // ASIGNAR / QUITAR + HISTÓRICO (como Licenses)
        // ============================================================
        public async Task AssignAsync(int deviceId, int userId)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device == null) return;

            await CloseActiveHistoryAsync(deviceId);

            device.UserId = userId;

            await OpenHistoryAsync(deviceId, userId);

            // ✅ atómico (RNF-024) 
            await _context.SaveChangesAsync();
        }

        public async Task UnassignAsync(int deviceId)
        {
            var device = await _context.Devices.FindAsync(deviceId);
            if (device == null) return;

            await CloseActiveHistoryAsync(deviceId);

            device.UserId = null;

            // ✅ atómico (RNF-024) 
            await _context.SaveChangesAsync();
        }

        public async Task<List<DeviceHistoryRowDto>> GetDeviceHistoryAsync(int deviceId)
        {
            var rows = await _context.DeviceHistories
                .Include(h => h.User)
                .Where(h => h.DeviceId == deviceId)
                .OrderByDescending(h => h.StartDate)
                .ToListAsync();

            return rows.Select(h =>
            {
                var end = h.EndDate; // null => ACTUAL
                return new DeviceHistoryRowDto
                {
                    UserName = h.User != null ? (h.User.Name + " " + h.User.LastName) : "-",
                    StartDate = h.StartDate.ToString("dd/MM/yyyy HH:mm"),
                    EndDate = end.HasValue ? end.Value.ToString("dd/MM/yyyy HH:mm") : null,
                    Duration = end.HasValue ? FormatDuration(h.StartDate, end.Value) : "ACTUAL"
                };
            }).ToList();
        }

        private static string FormatDuration(DateTime start, DateTime end)
        {
            var ts = end - start;

            if (ts.TotalMinutes < 1) return "0 min";
            if (ts.TotalHours < 1) return $"{(int)ts.TotalMinutes} min";

            if (ts.TotalDays < 1)
                return $"{(int)ts.TotalHours}h {ts.Minutes}m";

            return $"{(int)ts.TotalDays}d {ts.Hours}h";
        }

        private async Task CloseActiveHistoryAsync(int deviceId)
        {
            var active = await _context.DeviceHistories
                .Where(h => h.DeviceId == deviceId && h.EndDate == null)
                .OrderByDescending(h => h.StartDate)
                .FirstOrDefaultAsync();

            if (active != null)
                active.EndDate = DateTime.Now;
        }

        private Task OpenHistoryAsync(int deviceId, int userId)
        {
            var h = new DeviceHistory
            {
                DeviceId = deviceId,
                UserId = userId,
                StartDate = DateTime.Now,
                EndDate = null
            };

            _context.DeviceHistories.Add(h);
            return Task.CompletedTask;
        }

        // ============================================================
        // NORMALIZACIÓN
        // ============================================================
        private void NormalizeDevice(Device device)
        {
            device.Comment = string.IsNullOrWhiteSpace(device.Comment)
                ? string.Empty
                : device.Comment.Trim();

            device.Hostname = device.Hostname?.Trim();
            device.Model = device.Model?.Trim();
            device.SN = device.SN?.Trim();
        }

        internal async Task UnassignAsync(object deviceId)
        {
            throw new NotImplementedException();
        }
    }

    public class DeviceHistoryRowDto
    {
        public string UserName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; } // null => ACTUAL
        public string Duration { get; set; }
    }
}