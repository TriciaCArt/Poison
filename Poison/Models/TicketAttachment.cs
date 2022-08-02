using Poison.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Poison.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        [DisplayName("File Description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [DisplayName("Date Added")]
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        // foreign key items
        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }


        [NotMapped]
        [DisplayName("Select a file")]
        [DataType(DataType.Upload)]
        [MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] {".jpg",".jpeg", ".png",".doc",".docx", ".xls", ".xlsx", ".pdf", ".ppt", ".pptx", ".html", ".svg"})]
        public IFormFile? FormFile { get; set; }

        [DisplayName("File Name")]
        public string? FileName { get; set; }

        [DisplayName("File Attachment")]
        public byte[]? FileData { get; set; }

        [DisplayName("File Extension")]
        public string? FileContentType { get; set; }

        //Navigational properties

        [DisplayName("Ticket")]
        public virtual Ticket? Ticket { get; set; }

        [DisplayName("Team Member")]
        public virtual BTUser? User { get; set; }

    }
}
