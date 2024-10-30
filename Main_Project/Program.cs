using System.Globalization;
using Main_Project;
using Main_Project.Models;
using Main_Project.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Localization support
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en"),
        new CultureInfo("hi")
    };

    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Email Service
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configure DbContext
builder.Services.AddDbContext<NetflixDataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NetflixDatabase")));

builder.Services.AddHttpContextAccessor();

// Add distributed memory cache and session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Make the session cookie essential
});
builder.Services.AddControllersWithViews();

// Configure Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Sign_in"; // Redirect to the login page if not authenticated
        options.LogoutPath = "/Logout"; // Redirect to logout page
        options.Cookie.HttpOnly = true; // Make the cookie HTTP-only
        options.Cookie.IsEssential = true; // Make the cookie essential
        options.SlidingExpiration = true; // Refresh cookie expiration on each request
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // Set cookie expiration
    });

// Set the form options to allow for large file uploads (6 GB)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 8442450944; // 8 GB limit
});

// Configure Kestrel for large uploads (6 GB)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 8442450944; // 8 GB limit
});

var app = builder.Build();

// Localization middleware
var localizationOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session middleware
app.UseSession();
app.UseSubscription_Middleware(); // Ensure custom middleware is implemented

app.UseAuthentication(); // Enable authentication middleware
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.Run();
