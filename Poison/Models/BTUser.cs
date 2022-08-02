using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poison.Models
{
    public class BTUser : IdentityUser
    {
        [Required]
        [DisplayName("First Name")]
        [StringLength(40, ErrorMessage = "Go pound Sand and enter a real name, ya loser!", MinimumLength = 2)]
        public string? FirstName { get; set; }
        [Required]
        [DisplayName("Last Name")]
        [StringLength(40, ErrorMessage = "Nope. You are an idiot. Do you now know your own name?", MinimumLength = 2)]
        public string? LastName { get; set; }
        [NotMapped]
        [DisplayName("Full Name")]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? AvatarFormFile { get; set; }

        [DisplayName("Avatar")]
        public string? AvatarName { get; set; }
        public byte[]? AvatarData { get; set; }

        [DisplayName("File Extension")]
        public string? AvatarContentType { get; set; }
        public int CompanyId { get; set; }

        //Navigation Properties
        public virtual Company? Company { get; set; }

        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();


    }
}
