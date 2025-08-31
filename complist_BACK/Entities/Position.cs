namespace complist_BACK.Entities
{
    public class Position
    {
        public int Id {  get; set; }
        public string Name { get; set; }
        public List<User> Users { get; set; }
    }
}
