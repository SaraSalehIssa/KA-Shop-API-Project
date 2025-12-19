using Azure.Core;
using KASHOP.DAL.DTO.Request;
using KASHOP.DAL.DTO.Response;
using KASHOP.DAL.Models;
using Mapster;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASHOP.BLL.Service
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthenticationService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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

                var result = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
                if (!result)
                {
                    return new LoginResponse()
                    {
                        Success = false,
                        Message = "Invalid Password!",
                    };
                }

                return new LoginResponse()
                {
                    Success = true,
                    Message = "Login Successfully!"
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
    }
}
