namespace ChieuT4_Nhom05_WebQLCF.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Address { get; set; }
        public string City { get; set; } = string.Empty;
        public string Password { get; set; }
    }   
}
