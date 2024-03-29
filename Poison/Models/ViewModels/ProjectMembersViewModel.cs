﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace Poison.Models.ViewModels
{
    public class ProjectMembersViewModel
    {
        public Project? Project { get; set; }

        public MultiSelectList? UsersList { get; set; }

        public List<string>? SelectedUser { get; set; }
    }
}
