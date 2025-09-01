namespace complist_BACK.Entities
{
    public class Section
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<User>? Users { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public List<Mail>? Mails { get; set; }
    }
}
