using ClosedXML;
using ClosedXML.Excel;
using ERP_BIEN.Common.Enums;
using ERP_BIEN.Models;
using ERP_BIEN.Models.ViewModels;
using ERP_BIEN.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
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

        //====================================
        //exportar excel
        //====================================
        [Authorize(Policy = "DEVICES")]
        [HttpGet]
        public async Task<IActionResult> ExportExcel(
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
            // OJO: sacas TODO sin paginación
            var (devices, _) = await _service.GetDevicesAsync(
                1,
                int.MaxValue,
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

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Devices");

            // Headers
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Tipo";
            ws.Cell(1, 3).Value = "Hostname";
            ws.Cell(1, 4).Value = "Modelo";
            ws.Cell(1, 5).Value = "SN";
            ws.Cell(1, 6).Value = "Estado";
            ws.Cell(1, 7).Value = "Usuario";
            ws.Cell(1, 8).Value = "Fecha fabricación";

            int row = 2;

            foreach (var d in devices)
            {
                string tipo = d switch
                {
                    Computer => "Computer",
                    Phone => "Phone",
                    Screen => "Screen",
                    DockStation => "DockStation",
                    _ => "Device"
                };

                ws.Cell(row, 1).Value = d.Id;
                ws.Cell(row, 2).Value = tipo;
                ws.Cell(row, 3).Value = d.Hostname;
                ws.Cell(row, 4).Value = d.Model;
                ws.Cell(row, 5).Value = d.SN;
                ws.Cell(row, 6).Value = d.Status.ToString();
                ws.Cell(row, 7).Value = d.User != null ? $"{d.User.Name} {d.User.LastName}" : "";
                ws.Cell(row, 8).Value = d.ManufacturingDate?.ToString("yyyy-MM-dd");

                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"devices_{DateTime.Now:yyyyMMdd_HHmm}.xlsx"
            );
        }
    }
}