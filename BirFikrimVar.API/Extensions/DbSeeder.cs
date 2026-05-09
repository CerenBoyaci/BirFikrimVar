using BirFikrimVar.Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace BirFikrimVar.API.Extensions
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager, UserManager<Kullanici> userManager)
        {
           
            string[] roller = { "Admin", "OnOnayci", "KomisyonUyesi", "StandartKullanici" };

            foreach (var rol in roller)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }

          
            if (await userManager.FindByEmailAsync("admin@birfikrimvar.com") == null)
            {
                var admin = new Kullanici
                {
                    UserName = "admin@birfikrimvar.com",
                    Email = "admin@birfikrimvar.com",
                    Ad = "Sistem",
                    Soyad = "Yöneticisi",
                    EmailConfirmed = true, 
                    AktifMi = true
                };

                //şifre kuralımız en az 6 karakter
                var result = await userManager.CreateAsync(admin, "Admin123.");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
