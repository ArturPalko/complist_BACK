namespace complist_BACK.Entities
{
    public class Phone
    {     
        public int Id { get; set; }
        public string Number { get; set; }
        public int PhoneTypeId { get; set; }
        public PhoneType PhoneType { get; set; }
        public List<User>? Users { get; set; }
}
}
