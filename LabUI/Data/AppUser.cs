using Microsoft.AspNetCore.Identity;

namespace LabUI.Data
{
    public class AppUser: IdentityUser
    {
        public byte[] Avatar { get; set; }
    }
}
