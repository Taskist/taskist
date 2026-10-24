using Taskist.Web.Helpers.Attributes;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Taskist.Core.Enums;
using Taskist.Web.Helpers.Appearance;

namespace Taskist.Web.Models.Users;

public class ProfileModel
{
    public ProfileModel()
    {
        AvailableGenders = new List<SelectListItem> {
            new SelectListItem { Value = "1", Text = "Male" },
            new SelectListItem { Value = "2", Text = "Female" }
        };
    }

    [ScaffoldColumn(false)]
    public int Id { get; set; }

    [LocalizedDisplayName("Profile.FirstName")]
    public string FirstName { get; set; }

    [LocalizedDisplayName("Profile.LastName")]
    public string LastName { get; set; }

    [LocalizedDisplayName("Profile.FriendlyName")]
    public string FriendlyName { get; set; }

    [LocalizedDisplayName("Profile.Mobile")]
    public string MobileNumber { get; set; }

    [LocalizedDisplayName("Profile.MaritalStatus")]
    public MaritalStatus MaritalStatus { get; set; }

    [LocalizedDisplayName("Profile.Email")]
    public string Email { get; set; }

    public bool HasPhoto { get; set; }

    public IList<Taskist.Core.Dtos.Git.GitAccountStatusDto> GitConnections { get; set; } = [];

    [Display(Name = "Gender")]
    public int? Gender { get; set; }

    [Display(Name = "Date Of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [ScaffoldColumn(false)]
    public string DateOfBirthString { get; set; }

    [Display(Name = "Communication Address")]
    public string Address { get; set; }

    [Display(Name = "About Me")]
    public string Introduction { get; set; }

    [Display(Name = "Facebook")]
    [RegularExpression(@"((?:https?\:\/\/|\/.)(?:[-a-z0-9]+\.)*[-a-z0-9]+.*)", ErrorMessage = "Provide a valid url")]
    public string FaceBookUrl { get; set; }

    [Display(Name = "Twitter")]
    [RegularExpression(@"((?:https?\:\/\/|\/.)(?:[-a-z0-9]+\.)*[-a-z0-9]+.*)", ErrorMessage = "Provide a valid url")]
    public string TwitterUrl { get; set; }

    [Display(Name = "LinkedIn")]
    [RegularExpression(@"((?:https?\:\/\/|\/.)(?:[-a-z0-9]+\.)*[-a-z0-9]+.*)", ErrorMessage = "Provide a valid url")]
    public string LinkedInUrl { get; set; }

    [Display(Name = "Google Scholar")]
    [RegularExpression(@"((?:https?\:\/\/|\/.)(?:[-a-z0-9]+\.)*[-a-z0-9]+.*)", ErrorMessage = "Provide a valid url")]
    public string GoogleScholarUrl { get; set; }

    public string Name => string.IsNullOrWhiteSpace(FriendlyName) ? $"{FirstName} {LastName}" : FriendlyName;

    public bool HasSocialMediaLinks => !string.IsNullOrEmpty(FaceBookUrl) || !string.IsNullOrEmpty(TwitterUrl)
        || !string.IsNullOrEmpty(LinkedInUrl) || !string.IsNullOrEmpty(GoogleScholarUrl);

    public IList<SelectListItem> AvailableGenders { get; set; }

    public AppearanceSettings Appearance { get; set; } = new();
}
