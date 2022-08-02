using Microsoft.AspNetCore.Mvc.Rendering;

namespace Poison.Models.ViewModels
{
    public class AssignProjectManagerViewModel
    {
        public Project? Project { get; set; }
        public string? PMID { get; set; }

        public SelectList? PMList { get; set; }


    }
}
