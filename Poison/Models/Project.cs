﻿using Poison.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poison.Models
{
    public class Project
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }

        [Required]
        [StringLength(240, ErrorMessage = "the {0} must be at least {2} at most {1} characters long.", MinimumLength = 2)]
        [DisplayName("Project Name")]
        public string? Name { get; set; }

        [Required]
        [StringLength(2000, ErrorMessage = "the {0} must be at least {2} at most {1} characters long.", MinimumLength = 2)]
        [DisplayName("Project Description")]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Created")]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Project Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Project End Date")]
        public DateTime EndDate { get; set; }

        public int ProjectPriorityId { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        [MaxFileSize(1024 * 1024)]
        public IFormFile? ImageFormFile { get; set; }

        [DisplayName("File Name")]
        public string? ImageFileName { get; set; }

        [DisplayName("File Attachment")]
        public byte[]? ImageFileData { get; set; }

        [DisplayName("File Extension")]
        public string? ImageContentType { get; set; }
        

        public bool Archived { get; set; }

        //Navigation Properties
        public virtual Company? Company { get; set; }
        public virtual ProjectPriority? ProjectPriority { get; set; }

        public virtual ICollection<BTUser>? Members { get; set; } = new HashSet<BTUser>();

        public virtual ICollection<Ticket>? Tickets { get; set; } = new HashSet<Ticket>();


    }
}
