namespace Zyro.Models
{
    public class ProfileJourneyViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }

        // Optional fields (null/empty means missing)
        public string Phone { get; set; }
        public string City { get; set; }
        public string Age { get; set; } // keep string if your DB stores it as text
        public string ImagePath { get; set; }

        // Computed
        public int ProfileCompletionPercentage { get; set; }

        // Convenience flags for the view
        public bool HasPhone => !string.IsNullOrWhiteSpace(Phone);
        public bool HasCity => !string.IsNullOrWhiteSpace(City);
        public bool HasAge => !string.IsNullOrWhiteSpace(Age?.ToString());
        public bool HasProfilePicture => !string.IsNullOrWhiteSpace(ImagePath);

    }
}
