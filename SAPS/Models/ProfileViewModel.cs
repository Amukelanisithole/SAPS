using Logic.DTO;

namespace SAPS.Models
{
    public class ProfileViewModel
    {
        public UpdateProfileDTO Profile { get; set; } = new();
        public ChangePasswordDTO ChangePassword { get; set; } = new();
    }
}
