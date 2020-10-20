using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BulkyBook.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ResetPasswordModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string Token { get; set; }
        }

        public IActionResult OnGet(string token,string email)
        {
            //if (code == null)
            //{
            //    return BadRequest("A code must be supplied for password reset.");
            //}
            //else
            //{
            //    Input = new InputModel
            //    {
            //        Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
            //    };
            //    return Page();
            Input = new InputModel
            {
                Token = token,
                Email = email
            };

            //}
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {


            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
                return RedirectToPage("./ResetPasswordConfirmation");

            var resetPassResult = await _userManager.ResetPasswordAsync(user, Input.Token, Input.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }

                return Page();
            }

            return RedirectToPage("./ResetPasswordConfirmation");

            //if (!ModelState.IsValid)
            //{
            //    return Page();
            //}

            //var user = await _userManager.FindByEmailAsync(Input.Email);
            //if (user == null)
            //{
            //    // Don't reveal that the user does not exist
            //    return RedirectToPage("./ResetPasswordConfirmation");
            //}

            //var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            //if (result.Succeeded)
            //{
            //    return RedirectToPage("./ResetPasswordConfirmation");
            //}

            //foreach (var error in result.Errors)
            //{
            //    ModelState.AddModelError(string.Empty, error.Description);
            //}
            //return Page();
        }
    }
}
