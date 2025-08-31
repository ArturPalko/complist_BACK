namespace complist_BACK.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<User>? Users { get; set; }
        public List<Mail>? Mails { get; set; }   

    }
}
