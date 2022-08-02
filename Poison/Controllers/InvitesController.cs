using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Poison.Data;
using Poison.Extensions;
using Poison.Models;
using Poison.Services.Interfaces;

namespace Poison.Controllers
{
    public class InvitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPoisonProjectService _projectService;
        private readonly IPoisonCompanyInfoService _companyInfoService;
        private readonly IPoisonInviteService _inviteService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IDataProtector _protector;
        private readonly IEmailSender _emailSender;


        public InvitesController(ApplicationDbContext context,
                                    IPoisonProjectService projectService,
                                    IPoisonCompanyInfoService companyInfoService,
                                    IPoisonInviteService inviteService,
                                    UserManager<BTUser> userManager,
                                    IDataProtectionProvider protector,
                                    IEmailSender emailSender)
        {
            _context = context;
            _projectService = projectService;
            _companyInfoService = companyInfoService;
            _inviteService = inviteService;
            _userManager = userManager;
            _protector = protector.CreateProtector("Po!son.2022");
            _emailSender = emailSender;
        }

        // GET: Invites
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Invites.Include(i => i.Company).Include(i => i.Invitee).Include(i => i.Invitor).Include(i => i.Project);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Invites/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            var invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }


        [Authorize(Roles="Admin")]
        // GET: Invites/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId();

            ViewData["ProjectId"] = new SelectList(await _projectService.GetArchivedProjectsByCompanyIdAsync(companyId), "Id", "Name");
            return View();
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,InviteeEmail,InviteeFirstName,InviteeLastName,Message")] Invite invite)
        {
            ModelState.Remove("InvitorId");

            int companyId = User.Identity!.GetCompanyId();

            if (ModelState.IsValid)
            {                
                try
                {
                    Guid guid = Guid.NewGuid();

                    string token = _protector.Protect(guid.ToString());
                    string email = _protector.Protect(invite.InviteeEmail!);
                    string company = _protector.Protect(companyId.ToString());

                    string? callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email, company }, protocol: Request.Scheme);

                    string body = $@"{invite.Message} <br />
                              Please join my Company. <br />
                              Click the following link to join our team. <br />
                              <a href=""{callbackUrl}"">COLLABORATE</a>";

                    string? destination = invite.InviteeEmail;

                    Company poisonCompany = await _companyInfoService.GetCompanyInfoById(companyId);
                    string subject = $" Poison Bug Tracker : {poisonCompany.Name} Invite!";

                    await _emailSender.SendEmailAsync(destination, subject, body);

                    // Save invite in the database
                    invite.CompanyToken = guid;
                    invite.CompanyId = companyId;
                    invite.InviteDate = DateTime.UtcNow;
                    invite.InvitorId = _userManager.GetUserId(User);
                    invite.IsValid = true;

                    await _inviteService.AddNewInviteAsync(invite);

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception)
                {

                    throw;
                }       
                
            }
           
            ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyIdAsync(companyId), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessInvite(string token, string email, string company)
        {
            if (token == null)
            {
                return NotFound();
            }

            Guid companyToken = Guid.Parse(_protector.Unprotect(token));

            string inviteeEmail = _protector.Unprotect(email);

            int companyId = int.Parse(_protector.Unprotect(company));

            try
            {
                Invite invite = await _inviteService.GetInviteAsync(companyToken, inviteeEmail, companyId);

                if(invite != null)
                {
                    return View(invite);
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

    //    // GET: Invites/Delete/5
    //    public async Task<IActionResult> Delete(int? id)
    //    {
    //        if (id == null || _context.Invites == null)
    //        {
    //            return NotFound();
    //        }

    //        var invite = await _context.Invites
    //            .Include(i => i.Company)
    //            .Include(i => i.Invitee)
    //            .Include(i => i.Invitor)
    //            .Include(i => i.Project)
    //            .FirstOrDefaultAsync(m => m.Id == id);
    //        if (invite == null)
    //        {
    //            return NotFound();
    //        }

    //        return View(invite);
    //    }

    //    // POST: Invites/Delete/5
    //    [HttpPost, ActionName("Delete")]
    //    [ValidateAntiForgeryToken]
    //    public async Task<IActionResult> DeleteConfirmed(int id)
    //    {
    //        if (_context.Invites == null)
    //        {
    //            return Problem("Entity set 'ApplicationDbContext.Invites'  is null.");
    //        }
    //        var invite = await _context.Invites.FindAsync(id);
    //        if (invite != null)
    //        {
    //            _context.Invites.Remove(invite);
    //        }
            
    //        await _context.SaveChangesAsync();
    //        return RedirectToAction(nameof(Index));
    //    }

    //    private bool InviteExists(int id)
    //    {
    //      return (_context.Invites?.Any(e => e.Id == id)).GetValueOrDefault();
    //    }
    //}

