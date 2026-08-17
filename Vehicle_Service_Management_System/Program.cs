using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.Mappings;
using Vehicle_Service_Management_System.Application.Services;
using Vehicle_Service_Management_System.Infrastructure.Data;
using Vehicle_Service_Management_System.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);


// ============================================================
// 1. DATABASE
// ============================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});


// ============================================================
// 2. ASP.NET CORE IDENTITY
// ============================================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // ----------------------------------------------------
        // Password
        // ----------------------------------------------------

        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;

        // ----------------------------------------------------
        // User
        // ----------------------------------------------------

        options.User.RequireUniqueEmail = true;

        // ----------------------------------------------------
        // Lockout
        // ----------------------------------------------------

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// ============================================================
// 3. MVC
// ============================================================

builder.Services.AddControllersWithViews();



builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AutoMapperProfile>();
});


// ============================================================
// 5. APPLICATION SERVICES
// ============================================================

builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<JobCardService>();
builder.Services.AddScoped<MechanicService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<ServiceBookingService>();
builder.Services.AddScoped<ServiceTypeService>();
builder.Services.AddScoped<SparePartCategoryService>();
builder.Services.AddScoped<SparePartService>();
builder.Services.AddScoped<VehicleBrandService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<VehicleTypeService>();
builder.Services.AddScoped<ReportService>();


// ============================================================
// 6. BUILD APPLICATION
// ============================================================

var app = builder.Build();


// ============================================================
// 7. DATABASE MIGRATION + SEED
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context =
            services.GetRequiredService<ApplicationDbContext>();

        // ----------------------------------------------------
        // Apply EF Core migrations
        // ----------------------------------------------------

        await context.Database.MigrateAsync();

        // ----------------------------------------------------
        // Seed Identity roles + admin
        // ----------------------------------------------------

        await DbInitializer.SeedAsync(services);

        Console.WriteLine(
            "=================================================");

        Console.WriteLine(
            "Database seeding completed successfully.");

        Console.WriteLine(
            "=================================================");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            "=================================================");

        Console.WriteLine(
            "Database initialization failed:");

        Console.WriteLine(ex.ToString());

        Console.WriteLine(
            "=================================================");
    }
}


// ============================================================
// 8. HTTP PIPELINE
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


// ============================================================
// HTTPS
// ============================================================

app.UseHttpsRedirection();


// ============================================================
// STATIC FILES
// ============================================================

app.UseStaticFiles();


// ============================================================
// ROUTING
// ============================================================

app.UseRouting();


// ============================================================
// AUTHENTICATION
// ============================================================

app.UseAuthentication();


// ============================================================
// AUTHORIZATION
// ============================================================

app.UseAuthorization();


// ============================================================
// DEFAULT ROUTE
// ============================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");


// ============================================================
// RUN
// ============================================================

app.Run();