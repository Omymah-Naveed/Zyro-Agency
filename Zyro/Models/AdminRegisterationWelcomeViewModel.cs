namespace Zyro.Models
{
    public class AdminRegisterationWelcomeViewModel
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public int? Role { get; set; }

        public string? Phone { get; set; }

        public string? Action { get; set; }

        public string? Age { get; set; }

        public string? City { get; set; }

        public string? ImagePath { get; set; }
        public bool HasPhone => !string.IsNullOrWhiteSpace(Phone);
        public bool HasCity => !string.IsNullOrWhiteSpace(City);
        public bool HasAge => !string.IsNullOrWhiteSpace(Age?.ToString());
        public bool HasProfilePicture => !string.IsNullOrWhiteSpace(ImagePath);

    }
}
