// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace StriveAI.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Home/ServiceDashboard");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                // MONGODB CONFIG
                //var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                using (HttpClient httpClient = new())
                {
                    try
                    {
                        HttpResponseMessage response = await httpClient.GetAsync("https://strive-api.azurewebsites.net/api/MongoDB/GetUser?username=" + Input.Email);
                        string responseData = await response.Content.ReadAsStringAsync();
                        using (JsonDocument doc = JsonDocument.Parse(responseData))
                        {
                            JsonElement root = doc.RootElement;
                            if (root.TryGetProperty("data", out JsonElement dataElement))
                            {
                                if (dataElement.ValueKind == JsonValueKind.Null)
                                {
                                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                                    return Page();
                                }
                                else
                                {
                                    if (dataElement.TryGetProperty("password", out JsonElement passwordElement))
                                    {
                                        if (Input.Password == passwordElement.GetString())
                                        {
                                            var claims = new List<Claim>
                                            {
                                                new Claim(ClaimTypes.Name, Input.Email),
                                                // Add other claims as needed
                                            };

                                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                                            var authProperties = new AuthenticationProperties
                                            {
                                                IsPersistent = true, // Keeps the user logged in across sessions
                                                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // Adjust expiration as needed
                                            };

                                            // Sign in the user
                                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                                new ClaimsPrincipal(claimsIdentity),
                                                authProperties);
                                            _logger.LogInformation("User logged in.");
                                            return LocalRedirect(returnUrl);
                                        }
                                        else
                                        {
                                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                                            return Page();
                                        }
                                    }
                                    else
                                    {
                                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                                        return Page();
                                    }
                                }
                            }
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        // Handle exceptions (e.g., network issues, API errors)
                        Console.WriteLine($"Request error: {e.Message}");
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return Page();
                    }
                }
                /*if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }*/
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
