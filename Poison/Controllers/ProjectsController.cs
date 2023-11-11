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
using Poison.Services.Interfaces;
using Project = Poison.Models.Project;

namespace Poison.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IPoisonProjectService _projectService;
        private readonly IPoisonRolesService _rolesService;
        private readonly IPoisonFileService _fileService;
        private readonly IPoisonLookupService _lookupService;

        public ProjectsController(ApplicationDbContext context, UserManager<BTUser> userManager, IPoisonProjectService projectService, IPoisonRolesService rolesService, IPoisonFileService fileService, IPoisonLookupService lookupService)
        {
            _context = context;
            _userManager = userManager;
            _projectService = projectService;
            _rolesService = rolesService;
            _fileService = fileService;
            _lookupService = lookupService;
        }

        // GET: Projects
        [Authorize]
        public async Task<IActionResult> AllProjects()
        {
            BTUser btUser = await _userManager.GetUserAsync(User);
            var companyId = btUser.CompanyId;

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            return View(projects);
        }

        public async Task<IActionResult> MyProjects()
        {
            BTUser user = await _userManager.GetUserAsync(User);
            
            List<Project> projects = await _projectService.GetUserProjectsAsync(user.Id);

            return View(projects);
        }

        

        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignProjectMembers(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            ProjectMembersViewModel model = new();

            int companyId = User.Identity!.GetCompanyId();

            model.Project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(PoisonRoles.Developer), companyId);
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(PoisonRoles.Submitter), companyId);

            List<BTUser> teamMembers = developers.Concat(submitters).ToList();

            List<string> projectMembers = model.Project.Members!.Select(m => m.Id).ToList();

            model.UsersList = new MultiSelectList(teamMembers, "Id", "FullName", projectMembers);

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignProjectMembers(ProjectMembersViewModel model)
        {
            if(model.SelectedUser != null)
            {
                List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project!.Id)).Select(model => model.Id).ToList();

                foreach(string member in memberIds)
                {
                    await _projectService.RemoveUserFromProjectAsync(member, model.Project.Id);
                }

                foreach(string member in model.SelectedUser)
                {
                    await _projectService.AddUserToProjectAsync(member, model.Project.Id);
                }

                return RedirectToAction(nameof(Details), new { id = model.Project.Id });

            }

            return RedirectToAction(nameof(AssignProjectMembers), new {id = model.Project!.Id });
        }

        public async Task<IActionResult> AssignProjectManager(int? projectId)
        {
            if (projectId == null)
            {
                return NotFound();
            }

            AssignProjectManagerViewModel model = new();
            int companyId = User.Identity!.GetCompanyId();

            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(PoisonRoles.ProjectManager), companyId), "Id", "FullName");

            model.Project = await _projectService.GetProjectByIdAsync(projectId.Value, companyId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignProjectManager(AssignProjectManagerViewModel model)
        {
            if(model.PMID != null)
            {
                await _projectService.AddProjectManagerAsync(model.PMID, model.Project.Id);

                

                return RedirectToAction(nameof(Details), new { id = model.Project.Id});
                // overload 'go directly to the action with the parameter 
            }
            return RedirectToAction(nameof(AssignProjectManager), new { projectId = model.Project.Id });
        }

        //Create a new View(UnassignedProjects) that will list the unassigned projects for the company. Get and Post
        public async Task<IActionResult> UnassignedProjects(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            List<Project> projects = await _projectService.GetUnassignedProjectsAsync(companyId);

            return View(projects);
        }

        public async Task<IActionResult> ArchivedProjects()
        {
            BTUser btUser = await _userManager.GetUserAsync(User);
            var companyId = btUser.CompanyId;

            List<Project> projects = await _projectService.GetArchivedProjectsByCompanyIdAsync(companyId);

            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }
            //BTUser btUser = await _userManager.GetUserAsync(User);
            //var companyId = btUser.CompanyId;

            int companyId = User.Identity!.GetCompanyId();

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        public async Task<IActionResult> Create()
        {
            AddProjectWithPMViewModel model = new();

            int companyId = User.Identity!.GetCompanyId();
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(PoisonRoles.ProjectManager.ToString(), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(await _lookupService.GetProjectPrioritiesAsync(), "Id", "Name");

            return View(model);
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Project?.ImageFormFile != null)
                {
                    model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                    model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                    model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                }

                model.Project!.CompanyId = User.Identity!.GetCompanyId();
                model.Project.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                model.Project.StartDate = DateTime.SpecifyKind(model.Project.StartDate, DateTimeKind.Utc);
                model.Project.EndDate = DateTime.SpecifyKind(model.Project.EndDate, DateTimeKind.Utc);

                await _projectService.AddNewProjectAsync(model.Project);

                //TODO: Allow Admin to add Project Manager on create
                if (!string.IsNullOrEmpty(model.PMID))
                {
                    await _projectService.AddProjectManagerAsync(model.PMID, model.Project.Id);
                }

                return RedirectToAction(nameof(AllProjects));
            }

            int companyId = User.Identity!.GetCompanyId();
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(PoisonRoles.ProjectManager), companyId), "Id", "FullName");
            model.PriorityList = new SelectList(_context.ProjectPriorities, "Id", "Name");
            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", model.Project!.CompanyId);
            //ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", model.Project!.ProjectPriorityId);
            return View(model);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Projects == null)
            {

                return NotFound();
            }
            AddProjectWithPMViewModel model = new();

            int companyId = User.Identity!.GetCompanyId();

            Project project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            model.Project = project;

            BTUser projectManager = await _projectService.GetProjectManagerAsync(project.Id);
            if (projectManager != null)
            {
                model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(PoisonRoles.ProjectManager), companyId), "Id", "FullName", projectManager.Id);
            }
            else
            {
                model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(PoisonRoles.ProjectManager), companyId), "Id", "FullName");
            }

            model.PriorityList = new SelectList(_context.ProjectPriorities, "Id", "Name");

            if (project == null)
            {
                return NotFound();
            }

            return View(model);            
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
        {
            if (model.Project == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (model.Project.ImageFormFile != null)
                {
                    model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                    model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                    model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                }

                try
                {
                    
                    model.Project.Created = DateTime.SpecifyKind(model.Project.Created, DateTimeKind.Utc);

                    model.Project.StartDate = DateTime.SpecifyKind(model.Project.StartDate, DateTimeKind.Utc);
                    model.Project.EndDate = DateTime.SpecifyKind(model.Project.EndDate, DateTimeKind.Utc);

                    //call Service method to update/edit projects
                    await _projectService.UpdateProjectAsync(model.Project);

                    if (!string.IsNullOrEmpty(model.PMID))
                    {
                        await _projectService.AddProjectManagerAsync(model.PMID, model.Project.Id);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(model.Project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(AllProjects));
            }

            int companyId = User.Identity!.GetCompanyId();

            BTUser projectManager = await _projectService.GetProjectManagerAsync(model.Project.Id);

            if (projectManager != null)
            {
                model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(PoisonRoles.ProjectManager), companyId), "Id", "FullName", projectManager.Id);
            }
            else
            {
                model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(PoisonRoles.ProjectManager), companyId), "Id", "FullName");
            }

            model.PriorityList = new SelectList(_context.ProjectPriorities, "Id", "Name");
            
            return View(model);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            //var project = await _context.Projects
            //    .Include(p => p.Company)
            //    .Include(p => p.ProjectPriority)
            //    .FirstOrDefaultAsync(m => m.Id == id);
            int companyId = User.Identity!.GetCompanyId();

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                //Call Service Method to archive(Delete) project
                await _projectService.ArchiveProjectAsync(project);

            }
           
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpGet]
        public async Task<IActionResult> Restore(int id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            Project project = await _projectService.GetProjectByIdAsync(id, companyId);

            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreArchivedProjectsAsync(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }

            int companyId = User.Identity!.GetCompanyId();
            Project project = await _projectService.GetProjectByIdAsync(id, companyId);

            if (project != null)
            {
                //Call Service Method to archive(Delete) project
                await _projectService.RestoreArchivedProjectsAsync(project);

            }

            return RedirectToAction(nameof(Index));
        }
    }
}
