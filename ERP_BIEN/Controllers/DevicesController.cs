using ERP_BIEN.Common.Enums;
using ERP_BIEN.Models;
using ERP_BIEN.Models.ViewModels;
using ERP_BIEN.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_BIEN.Controllers
{
    [Authorize(Policy = "DEVICES")]
    public class DevicesController : Controller
    {
        private readonly DeviceService _service;

        public DevicesController(DeviceService service)
        {
            _service = service;
        }

        // ============================================================
        // INDEX
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            string deviceTypeFilter = null,
            StatusDevice? statusFilter = null,
            int? userIdFilter = null,
            string hostnameFilter = null,
            string modelFilter = null,
            string snFilter = null,
            DateTime? manufacturingFrom = null,
            DateTime? manufacturingTo = null,
            DateTime? useFrom = null,
            DateTime? useTo = null)
        {
            int pageSize = 10;
            if (pageNumber < 1) pageNumber = 1;

            var (devices, totalPages) = await _service.GetDevicesAsync(
                pageNumber,
                pageSize,
                deviceTypeFilter,
                statusFilter,
                userIdFilter,
                hostnameFilter,
                modelFilter,
                snFilter,
                manufacturingFrom,
                manufacturingTo,
                useFrom,
                useTo
            );

            var users = await _service.GetUsersAsync();

            if (totalPages > 0 && pageNumber > totalPages)
            {
                pageNumber = totalPages;
                (devices, totalPages) = await _service.GetDevicesAsync(
                    pageNumber,
                    pageSize,
                    deviceTypeFilter,
                    statusFilter,
                    userIdFilter,
                    hostnameFilter,
                    modelFilter,
                    snFilter,
                    manufacturingFrom,
                    manufacturingTo,
                    useFrom,
                    useTo
                );
            }

            var vm = new DevicesViewModel
            {
                Devices = devices,
                Users = users,

                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,

                DeviceTypeFilter = deviceTypeFilter,
                StatusFilter = statusFilter,
                UserIdFilter = userIdFilter,
                HostnameFilter = hostnameFilter,
                ModelFilter = modelFilter,
                SNFilter = snFilter,
                ManufacturingFrom = manufacturingFrom,
                ManufacturingTo = manufacturingTo,
                UseFrom = useFrom,
                UseTo = useTo
            };

            return View(vm);
        }

        // ============================================================
        // DETAILS
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var device = await _service.GetByIdAsync(id);
            if (device == null) return NotFound();

            return PartialView("_DeviceDetails", device);
        }

        // ============================================================
        // DETAILS JSON
        // ============================================================
        [HttpGet]
        public async Task<JsonResult> DetailsJson(int id)
        {
            var d = await _service.GetByIdAsync(id);
            if (d == null) return Json(null);

            string tipo = d switch
            {
                Computer => "Computer",
                Phone => "Phone",
                Screen => "Screen",
                DockStation => "DockStation",
                _ => "Device"
            };

            var dto = new
            {
                id = d.Id,
                tipo,
                hostname = d.Hostname,
                model = d.Model,
                sn = d.SN,
                numberOfDevice = d.NumberOfDevice,
                manufacturingDate = d.ManufacturingDate?.ToString("yyyy-MM-dd"),
                status = d.Status.ToString(),
                comment = d.Comment,
                useDate = d.UseDate?.ToString("yyyy-MM-dd"),
                userId = d.UserId,
                userName = d.User != null ? $"{d.User.Name} {d.User.LastName}" : null,
                asignado = d.UserId != null
            };

            return Json(dto);
        }

        // ============================================================
        // ✅ ASSIGN CORREGIDO
        // ============================================================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(
            int deviceId,   // ✅ CAMBIO CLAVE
            int userId,
            int pageNumber,
            string deviceTypeFilter = null,
            StatusDevice? statusFilter = null,
            int? userIdFilter = null,
            string hostnameFilter = null,
            string modelFilter = null,
            string snFilter = null,
            DateTime? manufacturingFrom = null,
            DateTime? manufacturingTo = null,
            DateTime? useFrom = null,
            DateTime? useTo = null)
        {
            await _service.AssignAsync(deviceId, userId); // ✅ CAMBIO CLAVE

            return RedirectToAction(nameof(Index), new
            {
                assigned = "ok",
                pageNumber,
                DeviceTypeFilter = deviceTypeFilter,
                StatusFilter = statusFilter,
                UserIdFilter = userIdFilter,
                HostnameFilter = hostnameFilter,
                ModelFilter = modelFilter,
                SNFilter = snFilter,
                ManufacturingFrom = manufacturingFrom,
                ManufacturingTo = manufacturingTo,
                UseFrom = useFrom,
                UseTo = useTo
            });

        }

        // ============================================================
        // ✅ UNASSIGN CORREGIDO
        // ============================================================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unassign(
            int deviceId,   // ✅ CAMBIO CLAVE
            int pageNumber,
            string deviceTypeFilter = null,
            StatusDevice? statusFilter = null,
            int? userIdFilter = null,
            string hostnameFilter = null,
            string modelFilter = null,
            string snFilter = null,
            DateTime? manufacturingFrom = null,
            DateTime? manufacturingTo = null,
            DateTime? useFrom = null,
            DateTime? useTo = null)
        {
            await _service.UnassignAsync(deviceId); // ✅ CAMBIO CLAVE

            return RedirectToAction(nameof(Index), new
            {
                pageNumber,
                DeviceTypeFilter = deviceTypeFilter,
                StatusFilter = statusFilter,
                UserIdFilter = userIdFilter,
                HostnameFilter = hostnameFilter,
                ModelFilter = modelFilter,
                SNFilter = snFilter,
                ManufacturingFrom = manufacturingFrom,
                ManufacturingTo = manufacturingTo,
                UseFrom = useFrom,
                UseTo = useTo
            });
        }

        // ============================================================
        // HISTORY
        // ============================================================
        [HttpGet]
        public async Task<JsonResult> HistoryJson(int deviceId)
        {
            var rows = await _service.GetDeviceHistoryAsync(deviceId);
            return Json(rows);
        }

        // ============================================================
        // CREATE
        // ============================================================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string deviceType,
            string hostname,
            string sn,
            string model,
            int numberOfDevice,
            DateTime? manufacturingDate,
            StatusDevice status,
            string comment,
            DateTime? useDate,
            int? userId,
            int pageNumber)
        {
            await _service.CreateAsync(
                deviceType,
                hostname,
                sn,
                model,
                numberOfDevice,
                manufacturingDate,
                status,
                comment,
                useDate,
                userId
            );

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // EDIT
        // ============================================================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string hostname,
            string sn,
            string model,
            int numberOfDevice,
            DateTime? manufacturingDate,
            StatusDevice status,
            string comment,
            DateTime? useDate,
            int? userId,
            int pageNumber)
        {
            await _service.EditAsync(
                id,
                hostname,
                sn,
                model,
                numberOfDevice,
                manufacturingDate,
                status,
                comment,
                useDate,
                userId
            );

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // DELETE
        // ============================================================
        [Authorize(Policy = "WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            int pageNumber)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}