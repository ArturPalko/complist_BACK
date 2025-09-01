namespace complist_BACK.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? DepartmentId { get; set; }
        public Department? Department {  get; set; }
        public int? SectionId { get; set; }
        public Section? Section { get; set; }
        public int? PositionId {  get; set; }
        public Position? Position {  get; set; }
        public List<Phone>? Phones { get; set; } 
        public List<Mail>? Mails { get; set; }   


    }
}
