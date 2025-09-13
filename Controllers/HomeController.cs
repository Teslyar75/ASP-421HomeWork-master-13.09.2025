using System.Diagnostics;
using ASP_421.Models;
using ASP_421.Services.Kdf;
using ASP_421.Services.Random;
using ASP_421.Services.Confirmation;
using ASP_421.Data;
using ASP_421.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASP_421.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRandomService _randomService;
        private readonly IKdfService _kdfService;
        private readonly IConfirmationCodeService _confirmationService;
        private readonly DataContext _dataContext;

        public HomeController(
            ILogger<HomeController> logger, 
            IRandomService randomService, 
            IKdfService kdfService,
            IConfirmationCodeService confirmationService,
            DataContext dataContext)
        {
            _logger = logger;
            _randomService = randomService;
            _kdfService = kdfService;
            _confirmationService = confirmationService;
            _dataContext = dataContext;
        }

        public async Task<IActionResult> Index()
        {
            await TrackVisit();
            return View();
        }

        public async Task<IActionResult> IoC()
        {
            await TrackVisit();
            ViewData["otp"] = _kdfService.Dk("Admin", "4FA5D20B-E546-4818-9381-B4BD9F327F4E"); // _randomService.Otp(6);
            return View();
        }

        public async Task<IActionResult> Razor()
        {
            await TrackVisit();
            return View();
        }

        public async Task<IActionResult> Privacy()
        {
            await TrackVisit();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Отслеживает заход пользователя на страницу
        /// </summary>
        private async Task TrackVisit()
        {
            try
            {
                var visit = new Visit
                {
                    Id = Guid.NewGuid(),
                    VisitTime = DateTime.UtcNow,
                    RequestPath = HttpContext.Request.Path,
                    UserLogin = HttpContext.Session.GetString("UserLogin"), // null если не авторизован
                    ConfirmationCode = _confirmationService.GenerateCode(),
                    IsConfirmed = false,
                    UserAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault(),
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                _dataContext.Visits.Add(visit);
                await _dataContext.SaveChangesAsync();

                // Сохраняем код подтверждения в сессии для отображения пользователю
                HttpContext.Session.SetString("ConfirmationCode", visit.ConfirmationCode);
                HttpContext.Session.SetString("VisitId", visit.Id.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отслеживании захода");
            }
        }

        /// <summary>
        /// Подтверждает заход по коду подтверждения
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ConfirmVisit(string confirmationCode)
        {
            try
            {
                if (string.IsNullOrEmpty(confirmationCode))
                {
                    TempData["Error"] = "Код подтверждения не может быть пустым";
                    return RedirectToAction("Index");
                }

                var visitId = HttpContext.Session.GetString("VisitId");
                if (string.IsNullOrEmpty(visitId))
                {
                    TempData["Error"] = "Сессия истекла. Пожалуйста, обновите страницу";
                    return RedirectToAction("Index");
                }

                var visit = await _dataContext.Visits
                    .FirstOrDefaultAsync(v => v.Id == Guid.Parse(visitId) && v.ConfirmationCode == confirmationCode);

                if (visit == null)
                {
                    TempData["Error"] = "Неверный код подтверждения";
                    return RedirectToAction("Index");
                }

                if (visit.IsConfirmed)
                {
                    TempData["Info"] = "Этот заход уже был подтвержден ранее";
                    return RedirectToAction("Index");
                }

                visit.IsConfirmed = true;
                visit.ConfirmedAt = DateTime.UtcNow;
                await _dataContext.SaveChangesAsync();

                TempData["Success"] = "Заход успешно подтвержден!";
                HttpContext.Session.Remove("ConfirmationCode");
                HttpContext.Session.Remove("VisitId");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при подтверждении захода");
                TempData["Error"] = "Произошла ошибка при подтверждении захода";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Показывает статистику заходов
        /// </summary>
        public async Task<IActionResult> VisitStats()
        {
            try
            {
                var stats = await _dataContext.Visits
                    .GroupBy(v => v.RequestPath)
                    .Select(g => new
                    {
                        Path = g.Key,
                        TotalVisits = g.Count(),
                        ConfirmedVisits = g.Count(v => v.IsConfirmed),
                        LastVisit = g.Max(v => v.VisitTime)
                    })
                    .OrderByDescending(s => s.TotalVisits)
                    .ToListAsync();

                ViewData["Stats"] = stats;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики заходов");
                TempData["Error"] = "Ошибка при загрузке статистики";
                return RedirectToAction("Index");
            }
        }
    }
}
