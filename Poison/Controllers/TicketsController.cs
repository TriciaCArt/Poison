using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Poison.Data;
using Poison.Extensions;
using Poison.Models;
using Poison.Models.Enums;
using Poison.Models.ViewModels;
using Poison.Services;
using Poison.Services.Interfaces;

namespace Poison.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IPoisonTicketService _ticketService;
        private readonly IPoisonProjectService _projectService;
        private readonly IPoisonTicketHistoryService _ticketHistory;
        private readonly IPoisonNotificationService _notification;
        private readonly IPoisonLookupService _lookupService;
        private readonly IPoisonRolesService _roleService;
        private readonly IPoisonFileService _fileService;

        public TicketsController(ApplicationDbContext context, UserManager<BTUser> userManager, IPoisonTicketService ticketService, IPoisonProjectService projectService, IPoisonTicketHistoryService ticketHistory, IPoisonNotificationService notification, IPoisonLookupService lookupService, IPoisonRolesService roleService, IPoisonFileService fileService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _projectService = projectService;
            _ticketHistory = ticketHistory;
            _notification = notification;
            _lookupService = lookupService;
            _roleService = roleService;
            _fileService = fileService;
        }

        // GET: Tickets

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileContentType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DateTime.UtcNow;
                ticketAttachment.UserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        public async Task<IActionResult> AllTickets()
        {
            BTUser bTUser = await _userManager.GetUserAsync(User);
            var userId = bTUser.CompanyId;
            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(userId);

            return View(tickets);

        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string? fileName = ticketAttachment.FileName;
            byte[]? fileData = ticketAttachment.FileData;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");

            return File(fileData!, $"application/{ext}");
        }

        public async Task<IActionResult> ArchivedTickets()
        {
            BTUser bTUser = await _userManager.GetUserAsync(User);
            var userId = bTUser.CompanyId;
            List<Ticket> tickets = await _ticketService.GetArchivedTicketsAsync(userId);

            return View(tickets);

        }

        [HttpGet]
        public async Task<IActionResult> AssignDeveloper(int? ticketId)
        {
            if (ticketId == null)
            {
                return NotFound();
            }

            AssignDevToTicketViewModel model = new();
            int companyId = User.Identity!.GetCompanyId();

            model.Ticket = await _ticketService.GetTicketByIdAsync(ticketId.Value);
            model.Developers = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(PoisonRoles.Developer), companyId), "Id", "FullName");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(AssignDevToTicketViewModel model)
        {
            if (model.Ticket == null)
            {
                return NotFound();
            }


            if (model.DeveloperId != null)
            {

                BTUser btUser = await _userManager.GetUserAsync(User);
                //oldTicket
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket!.Id);
                Ticket ticket = await _ticketService.GetTicketByIdAsync(model.Ticket!.Id);

                try
                {
                    ticket.Created = DateTime.SpecifyKind(ticket.Created, DateTimeKind.Utc);
                    ticket.Updated = DateTime.UtcNow;
                    ticket.DeveloperUserId = model.DeveloperId;

                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (Exception)
                {
                    throw;
                }
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket!.Id);
                await _ticketHistory.AddHistoryAsync(oldTicket, newTicket, btUser.Id);

                Notification devNotification = new()
                {
                    TicketId = ticket.Id,
                    NotificationTypeId = (await _lookupService.LookupNotificationTypeIdAsync(nameof(PoisonNotificationTypes.Ticket))).Value,
                    Title = "Ticket Updated",
                    Message = $"Ticket: {model.Ticket.Title}, was updated by {btUser.FullName}",
                    Created = DateTime.UtcNow,
                    SenderId = btUser.Id,
                    RecipientId = ticket.DeveloperUserId
                };
                await _notification.AddNotificationAsync(devNotification);
                await _notification.SendEmailNotificationAsync(devNotification, "Ticket Updated");

                return RedirectToAction(nameof(Details), new { id = model.Ticket?.Id });
            }

            int companyId = User.Identity!.GetCompanyId();

            model.Ticket = await _ticketService.GetTicketByIdAsync(model.Ticket.Id);
            model.Developers = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(PoisonRoles.Developer), companyId), "Id", "FullName");

            return View(model);

        }

        public async Task<IActionResult> MyTickets()
        {

            var compId = User.Identity.GetCompanyId();
            string userId = _userManager.GetUserId(User);
            List<Ticket> tickets = await _ticketService.GetTicketByUserIdAync(userId, compId);

            return View(tickets);
        }

        [Authorize(Roles="Admin,ProjectManager")]
        public async Task<IActionResult> UnassignedTickets(int companyId)
        {
            int userId = User.Identity!.GetCompanyId();

            //if (companyId == null)
            //{
            //    return NotFound();
            //}


            List<Ticket> tickets = await _ticketService.GetUnassignedTicketsAsync(userId);

            return View(tickets);


        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId();
            string userId = _userManager.GetUserId(User);

            if (User.IsInRole(nameof(PoisonRoles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyIdAsync(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(userId), "Id", "Name");
            }


            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name");

            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name");

            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId,")] Ticket ticket)
        {
            ModelState.Remove("SubmitterUserId");


            if (ModelState.IsValid)
            {
                ticket.Created = DateTime.UtcNow;
                ticket.SubmitterUserId = _userManager.GetUserId(User);

                ticket.TicketStatusId = (await _context.TicketStatuses.FirstOrDefaultAsync(t => t.Name == "New"))!.Id;

                await _ticketService.AddNewTicketAsync(ticket);

                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                await _ticketHistory.AddHistoryAsync(null!, newTicket!, ticket.SubmitterUserId);

                //TODO: Add something else, I don't know what.


                return RedirectToAction(nameof(AllTickets));
            }

            int companyId = User.Identity!.GetCompanyId();
            string userId = _userManager.GetUserId(User);

            if (User.IsInRole(nameof(PoisonRoles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyIdAsync(companyId), "Id", "Name");
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetUserProjectsAsync(userId), "Id", "Name");
            }


            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");

            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);
            if (ticket == null)
            {
                return NotFound();
            }

            ViewData["TicketPriorityId"] = new SelectList(await _lookupService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _lookupService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _lookupService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,TicketPriorityId,TicketStatusId,TicketTypeId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            //ModelState.Remove("SubmitterUserId");

            if (ModelState.IsValid)
            {
                string userId = _userManager.GetUserId(User);                

                Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);

                try
                {
                    //ticket.SubmitterUserId = _userManager.GetUserId(User);
                    ticket.Created = DateTime.SpecifyKind(ticket.Created, DateTimeKind.Utc);
                    ticket.Updated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);


                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id);
                await _ticketHistory.AddHistoryAsync(oldTicket!, newTicket!, userId);

                return RedirectToAction(nameof(AllTickets));
            }

            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            //_context.Tickets
            //.Include(t => t.DeveloperUser)
            //.Include(t => t.Project)
            //.Include(t => t.SubmitterUser)
            //.Include(t => t.TicketPriority)
            //.Include(t => t.TicketStatus)
            //.Include(t => t.TicketType)
            //.FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);

            if (ticket != null)
            {
                await _ticketService.ArchiveTicketAsync(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AllTickets));
        }

        private async Task<bool> TicketExists(int id)
        {
            int companyId = User.Identity!.GetCompanyId();

            return (await _ticketService.GetAllTicketsByCompanyIdAsync(companyId)).Any(t=>t.Id == id);

            //return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpGet]
        public async Task<IActionResult> Restore(int id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            BTUser bTUser = await _userManager.GetUserAsync(User);
            var userId = bTUser.CompanyId;
            List<Ticket> tickets = await _ticketService.GetArchivedTicketsAsync(id);

            return View(tickets);
        }

        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreArchivedTicketsAsync(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContest.Projects' is null.");
            }

            Ticket? tickets = await _ticketService.GetTicketByIdAsync(id);

            if (tickets != null)
            {
                await _ticketService.RetoreArchivedTicketsAsync(tickets);
            }

            return RedirectToAction(nameof(MyTickets));

        }
    }
}

