using Fanex.Affiliate.Link.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.Affiliate.Link.Controllers
{
    public class UnderMaintenanceController : Controller
    {
        private const string MsgFormat = "<div style='text-align:center;'>System is currently unavailable due to scheduled maintenance." +
            "<br /> This downtime will last from <b>{0}</b> &rarr; <b>{1}</b> (HK Time GMT +8). Thanks for your patience!</div>";

        private readonly IUnderMaintenanceService _underMaintenanceService;

        public UnderMaintenanceController(IUnderMaintenanceService underMaintenanceService)
        {
            _underMaintenanceService = underMaintenanceService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var maintenanceInfo = _underMaintenanceService.GetUnderMaintenanceTime();

            if (!maintenanceInfo.IsUnderMaintenance)
            {
                return RedirectToAction("PageNotFound", "Home");
            }

            var fromTime = maintenanceInfo.StartTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown";
            var toTime = maintenanceInfo.EndTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown";
            ViewBag.Result = string.Format(MsgFormat, fromTime, toTime);

            return View();
        }
    }
}