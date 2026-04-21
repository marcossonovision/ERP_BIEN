using ERP_BIEN.Common.Enums;
using ERP_BIEN.Models;
using ERP_BIEN.Models.ViewModels;
using ERP_BIEN.Services;
using Microsoft.AspNetCore.Mvc;

namespace ERP_BIEN.Controllers
{
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

            // Clamp para evitar UI inconsistente / páginas vacías
            if (totalPages > 0 && pageNumber > totalPages)
            {
                pageNumber = totalPages;
                (devices, totalPages) = await _service.GetDevicesAsync(pageNumber, pageSize, deviceTypeFilter, statusFilter, userIdFilter, hostnameFilter, modelFilter, snFilter, manufacturingFrom, manufacturingTo, useFrom, useTo);
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
        // DETAILS (para abrir modal al pulsar fila)
        // ============================================================
        public async Task<IActionResult> Details(int id)
        {
            var device = await _service.GetByIdAsync(id);
            if (device == null)
                return NotFound();

            return PartialView("_DeviceDetails", device);
        }

        // ============================================================
        // CREATE
        // ============================================================
        [HttpPost]
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
        // EDIT
        // ============================================================
        [HttpPost]
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
        // DELETE
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Delete(
            int id,
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
            await _service.DeleteAsync(id);
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
    }
}
