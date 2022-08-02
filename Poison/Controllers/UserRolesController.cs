using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Poison.Extensions;
using Poison.Models;
using Poison.Models.ViewModels;
using Poison.Services;
using Poison.Services.Interfaces;

namespace Poison.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserRolesController : Controller
    {
        private readonly IPoisonCompanyInfoService _companyInfoService;
        private readonly IPoisonRolesService _poisonRoles;

        public UserRolesController(IPoisonCompanyInfoService companyInfoService, IPoisonRolesService poisonRoles)
        {
            _companyInfoService = companyInfoService;
            _poisonRoles = poisonRoles;
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> model = new();

            int companyId = User.Identity!.GetCompanyId();
            List<BTUser> btUsers = await _companyInfoService.GetAllMembersAsync(companyId);

            foreach (BTUser btUser in btUsers)
            {
                ManageUserRolesViewModel viewModel = new();
                viewModel.BTUser = btUser;
                IEnumerable<string> currentRoles = await _poisonRoles.GetUserRolesAsync(btUser);
                viewModel.Roles = new MultiSelectList(await _poisonRoles.GetPoisonRolesAsync(), "Name", "Name", currentRoles);

                model.Add(viewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            int companyId = User.Identity!.GetCompanyId();

            BTUser? user = (await _companyInfoService.GetAllMembersAsync(companyId)).FirstOrDefault(c => c.Id == member.BTUser!.Id);

            IEnumerable<string> currentRoles = await _poisonRoles.GetUserRolesAsync(user);

            string? selectedUserRole = member.SelectedRoles!.FirstOrDefault();

            if (!string.IsNullOrEmpty(selectedUserRole))
            {
                if (await _poisonRoles.RemoveUserFromRolesAsync(user, currentRoles))
                {
                    await _poisonRoles.AddUserToRoleAsync(user!, selectedUserRole);
                }
            }
            else
            {
                return RedirectToAction(nameof(ManageUserRoles));
            }

            return RedirectToAction("CompanyMembers", "Companies");
        }
    }
}
