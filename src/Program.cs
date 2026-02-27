using Lif.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Lif.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// Seed rollen en admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    // Zorg ervoor dat de rollen bestaan
    await EnsureRolesAsync(roleManager);
    
    // Maak admin user aan
    await CreateAdminUserAsync(userManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

// Helper methods voor seeding
async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
{
    string[] roleNames = { "admin", "user" };
    
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            var result = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                Console.WriteLine($"Rol '{roleName}' is aangemaakt.");
            }
            else
            {
                Console.WriteLine($"Fout bij het aanmaken van rol '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}

async Task CreateAdminUserAsync(UserManager<ApplicationUser> userManager)
{
    var adminEmail = "info@lifgroup.nl";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new ApplicationUser 
        { 
            UserName = adminEmail, 
            Email = adminEmail,
            EmailConfirmed = true 
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "admin");
            Console.WriteLine($"Admin gebruiker '{adminEmail}' is aangemaakt met rol 'admin'.");
        }
        else
        {
            Console.WriteLine($"Fout bij het aanmaken van admin gebruiker: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        // Controleer of de admin user de admin rol heeft
        var isInRole = await userManager.IsInRoleAsync(adminUser, "admin");
        if (!isInRole)
        {
            await userManager.AddToRoleAsync(adminUser, "admin");
            Console.WriteLine($"Admin rol toegevoegd aan bestaande gebruiker '{adminEmail}'.");
        }
    }
}
