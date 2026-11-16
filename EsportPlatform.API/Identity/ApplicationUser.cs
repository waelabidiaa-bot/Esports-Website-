using Microsoft.AspNetCore.Identity;


namespace EsportPlatform.API.Identity;


public class ApplicationUser : IdentityUser
{
      public string FullName { get; set; } = string.Empty;
}