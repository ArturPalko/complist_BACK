using System.ComponentModel.DataAnnotations.Schema;

namespace complist_BACK.Entities
{
    public class Mail
    {
        public int Id { get; set; }
        public string? PreviousName { get; set; }
        public string Name { get; set; }
        public string? Password { get; set; }
        public int MailTypeIdP { get; set; }
        public MailType MailType { get; set; }
        public int? DepartmentId { get; set; }
        public Department? Department { get; set;}
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
