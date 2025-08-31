namespace complist_BACK.Entities
{
    public class PhoneType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Phone> Phones { get; set; }
    }
}
