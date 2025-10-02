using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

    [Display(Name = "First Name")]
    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; }

    [Display(Name = "Last Name")]
    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; }

    [Display(Name = "Mobile Number")]
    public string MobileNumber { get; set; }

    [Display(Name = "Email")]
    public string Email { get; set; }

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

    public string Name => $"{FirstName} {LastName}";

    public bool HasSocialMediaLinks => !string.IsNullOrEmpty(FaceBookUrl) || !string.IsNullOrEmpty(TwitterUrl)
        || !string.IsNullOrEmpty(LinkedInUrl) || !string.IsNullOrEmpty(GoogleScholarUrl);

    public IList<SelectListItem> AvailableGenders { get; set; }
}