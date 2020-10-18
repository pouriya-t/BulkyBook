using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using BulkyBook.Utility;

namespace BulkyBook.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class UnconfirmedEmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public UnconfirmedEmailModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [TempData]
        public Guid UserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task OnGetAsync(Guid userId)
        {
            UserId = userId;
            var user = await _userManager.FindByIdAsync(userId.ToString());
            Input.Email = user.Email;
            ModelState.Clear();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(UserId.ToString());

                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return RedirectToPage("./CheckEmail");
                }

                if (user.Email != Input.Email)
                {
                    var errors = new List<IdentityError>();
                    if (_userManager.Options.User.RequireUniqueEmail)
                    {
                        var owner = await _userManager.FindByEmailAsync(Input.Email);
                        if (owner != null && !string.Equals
                           (await _userManager.GetUserIdAsync(owner),
                            await _userManager.GetUserIdAsync(user)))
                        {
                            ModelState.AddModelError(string.Empty,
                            new IdentityErrorDescriber().DuplicateEmail(Input.Email).Description);
                            return Page();
                        }
                    }

                    await _userManager.SetEmailAsync(user, Input.Email);
                }

                var result = await _userManager.UpdateSecurityStampAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        return Page();
                    }
                }

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = user.Id, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' > clicking here </a>.");

                return RedirectToPage("./CheckEmail");
            }

            return Page();
        }
    }
}