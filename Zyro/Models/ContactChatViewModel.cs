namespace Zyro.Models
{
    public class ContactChatViewModel
    {
        public int ContactId { get; set; }
        public string ContactName { get; set; }
        public List<ContactMessage> Messages { get; set; }
    }

}
