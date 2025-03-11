namespace MediCareConnect.Models
{
    public class DoctorPublicProfileViewModel
    {
        public Doctor Doctor { get; set; } = null!;
        public double AverageRating { get; set; }
        public List<DoctorReview> Reviews { get; set; } = new List<DoctorReview>();
    }
}
