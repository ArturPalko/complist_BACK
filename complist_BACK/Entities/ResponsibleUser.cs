namespace complist_BACK.Entities
{
    public class ResponsibleUser
    {
        public int Id { get; set; }
        public int MailId { get; set; }   // FK → Mails
        public int UserId { get; set; }   // Відповідальна особа

        public Mail Mail { get; set; }
        public User User { get; set; }
    }

}
