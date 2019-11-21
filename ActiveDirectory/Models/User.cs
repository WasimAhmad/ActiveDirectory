namespace ActiveDirectory.Models
{
    public class User
    {
        public string Id { get; set; }
        public string Pid { get; set; }
        public string DisplayName { get; set; }
        public string SamAccountName { get; set; }
        public string Manager { get; set; }
        public string Image { get; set; }
        public string JobTitle { get; set; }
        public string Company { get; set; }
        public string Phone { get; set; }
        public string Disabled { get; set; }
    }
}