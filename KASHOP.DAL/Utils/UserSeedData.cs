using KASHOP.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASHOP.DAL.Utils
{
    public class UserSeedData : ISeedData
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserSeedData(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        async Task ISeedData.DataSeed()
        {
            if (!await _userManager.Users.AnyAsync())
            {
                var user1 = new ApplicationUser()
                {
                    UserName = "sIssa",
                    Email = "issasalehsara2001@gmail.com",
                    EmailConfirmed = true,
                    FullName = "Sara Issa"
                };
                var user2 = new ApplicationUser()
                {
                    UserName = "aRafat",
                    Email = "ahmad99@gmail.com",
                    EmailConfirmed = true,
                    FullName = "Ahmad Rafat"
                };
                var user3 = new ApplicationUser()
                {
                    UserName = "aKaled",
                    Email = "afefakhaled@gmail.com",
                    EmailConfirmed = true,
                    FullName = "Afefa Khaled"
                };

                await _userManager.CreateAsync(user1, "Pass@2001");
                await _userManager.CreateAsync(user2, "Pass@1999");
                await _userManager.CreateAsync(user3, "Pass@2003");

                await _userManager.AddToRoleAsync(user1, "Administrator");
                await _userManager.AddToRoleAsync(user2, "Admin");
                await _userManager.AddToRoleAsync(user3, "User");

            }
        }
    }
}
