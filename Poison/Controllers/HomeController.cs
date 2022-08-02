using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Poison.Extensions;
using Poison.Models;
using Poison.Models.AmChartData;
using Poison.Models.Enums;
using Poison.Models.ViewModels;
using Poison.Services.Interfaces;
using System.Diagnostics;

namespace Poison.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPoisonProjectService _projectService;
        private readonly IPoisonTicketService _ticketService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IPoisonCompanyInfoService _companyInfoService;

        public HomeController(ILogger<HomeController> logger, IPoisonProjectService projectService, IPoisonTicketService ticketService, IPoisonCompanyInfoService companyInfoService)
        {
            _logger = logger;
            _projectService = projectService;
            _ticketService = ticketService;
            _companyInfoService = companyInfoService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LandingPage()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            DashboardViewModel model = new();

            int companyId = User.Identity!.GetCompanyId();

            model.Company = await _companyInfoService.GetCompanyInfoById(companyId);
            
            model.Projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);
            model.Tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);

            model.Members = await _companyInfoService.GetAllMembersAsync(companyId);   

            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> AmCharts()
        {

            AmChartData amChartData = new();
            List<AmItem> amItems = new();

            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = (await _companyInfoService.GetAllProjectsAsync(companyId)).Where(p => p.Archived == false).ToList();

            foreach (Project project in projects)
            {
                AmItem item = new();

                item.Project = project.Name;
                item.Tickets = project.Tickets.Count;
                item.Developers = (await _projectService.GetProjectMemberByRoleAsync(project.Id, nameof(PoisonRoles.Developer))).Count();

                amItems.Add(item);
            }

            amChartData.Data = amItems.ToArray();


            return Json(amChartData.Data);
        }

        [HttpPost]
        public async Task<JsonResult> GglProjectTickets()
        {
            int companyId = User.Identity.GetCompanyId();

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            List<object> chartData = new();
            chartData.Add(new object[] { "ProjectName", "TicketCount" });

            foreach (Project prj in projects)
            {
                chartData.Add(new object[] { prj.Name, prj.Tickets.Count() });
            }

            return Json(chartData);
        }

        [HttpPost]
        public async Task<JsonResult> GglProjectPriority()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            List<object> chartData = new();
            chartData.Add(new object[] { "Priority", "Count" });


            foreach (string priority in Enum.GetNames(typeof(PoisonProjectPriorities)))
            {
                int priorityCount = (await _projectService.GetAllProjectsByPriorityAsync(companyId, priority)).Count();
                chartData.Add(new object[] { priority, priorityCount });
            }

            return Json(chartData);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}