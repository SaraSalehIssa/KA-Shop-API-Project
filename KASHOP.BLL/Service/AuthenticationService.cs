using Azure.Core;
using KASHOP.DAL.DTO.Request;
using KASHOP.DAL.DTO.Response;
using KASHOP.DAL.Models;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KASHOP.BLL.Service
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthenticationService(UserManager<ApplicationUser> userManager, IConfiguration configuration, IEmailSender emailSender, SignInManager<ApplicationUser>
            signInManager
            )
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }
        public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginRequest.Email);
                if(user is null)
                {
                    return new LoginResponse()
                    {
                        Success = false,
                        Message = "Invalid Email!",
                    };
                }

                if(await _userManager.IsLockedOutAsync(user))
                {
                    return new LoginResponse()
                    {
                        Success = false,
                        Message = "Sorry, Accout is Locked! Please Try Again!"
                    };
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, true);
                if (result.IsLockedOut)
                {
                    return new LoginResponse()
                    {
                        Success = false,
                        Message = "Sorry, Accout is Locked! Please Try Again!"
                    };
                }

                else if (result.IsNotAllowed)
                {
                    return new LoginResponse()
                    {
                        Success = false,
                        Message = "Sorry, Please Confirm Your Email!"
                    };
                }

                if (!result.Succeeded)
                {
                    return new LoginResponse()
                    {
                        Success = false,
                        Message = "Sorry, Invalid Password!"
                    };
                }

                //var result = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
                //if (!result)
                //{
                //    return new LoginResponse()
                //    {
                //        Success = false,
                //        Message = "Invalid Password!",
                //    };
                //}

                return new LoginResponse()
                {
                    Success = true,
                    Message = "Login Successfully!",
                    AccessToken = await GenerateJwtToken(user)
                };
            } catch(Exception ex)
            {
                return new LoginResponse()
                {
                    Success = false,
                    Message = "Exception Error!",
                    Errors = new List<string>
                    {
                        ex.Message
                    }
                };
            }
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            try
            {
                var user = registerRequest.Adapt<ApplicationUser>();
                var result = await _userManager.CreateAsync(user, registerRequest.Password);

                if (!result.Succeeded)
                {
                    return new RegisterResponse()
                    {
                        Success = false,
                        Message = "User Creation Failed!",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }
                await _userManager.AddToRoleAsync(user, "User");

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                token = Uri.EscapeDataString(token);

                var emailUrl = $"https://localhost:7026/api/auth/Account/ConfirmEmail?token={token}&userid={user.Id}";

                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Welcome!",
                    $"<h1>Welcome {user.FullName}</h1>" +
                    $"<a href='{emailUrl}'>Confirm Email</a>"
                );

                return new RegisterResponse()
                {
                    Success = true,
                    Message = "Success!"
                };
            } catch(Exception ex)
            {
                return new RegisterResponse()
                {
                    Success = false,
                    Message = "Exception Error!",
                    Errors = new List<string>
                    {
                        ex.Message
                    }
                };
            }
        }

        public async Task<bool> ConfirmEmailAsync(string token, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return false;

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return false;

            return true;
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var userClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: userClaims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ForgetPasswordResponse> RequestPasswordReset(ForgetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if(user == null)
            {
                return new ForgetPasswordResponse { 
                    Success = false,
                    Message = "Email Not Found!",
                };
            }

            var random = new Random();
            var code = random.Next(1000, 9999).ToString();

            user.CodeResetPassword = code;
            user.PasswordResetCodeExpiry = DateTime.UtcNow.AddMinutes(15);

            await _userManager.UpdateAsync(user);


            await _emailSender.SendEmailAsync(request.Email, "Reset Password", $"<p>Code is: {code}</p>");
            return new ForgetPasswordResponse
            {
                Success = true,
                Message = "Code sent to your email"
            };
        }

        public async Task<ResetPasswordResponse> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Email Not Found!",
                };
            }

            else if (user.CodeResetPassword != request.Code)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Error Code!",
                };
            }

            else if (user.PasswordResetCodeExpiry < DateTime.UtcNow)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Expired Code!",
                };
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Error on Reset Password!",
                    Errors = result.Errors.Select(e=>e.Description).ToList()
                };
            }

            await _emailSender.SendEmailAsync(request.Email, "Changed Password", $"<p>Password is Changed!</p>");
            return new ResetPasswordResponse
            {
                Success = true,
                Message = "Password is Changed!"
            };
        }
    }
}
