using App.Data;
using App.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MongoDB.Driver;
using quan_ly_kho_hang.Data;
using quan_ly_kho_hang.Menu;
using quan_ly_kho_hang.Models;
using quan_ly_kho_hang.Repositories;
using quan_ly_kho_hang.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cấu hình Mail
builder.Services.AddOptions();
var mailsetting = builder.Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailsetting);
builder.Services.AddSingleton<IEmailSender, SendMailService>();

// Đọc cấu hình từ appsettings.json
var mongoConnection = builder.Configuration.GetConnectionString("MongoDb");
var mongoDatabase = builder.Configuration.GetConnectionString("DatabaseName");

// Đăng ký MongoClient và DataContext
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnection));
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabase);
});
builder.Services.AddSingleton<AppDbContext>(); 

builder.Services
    .AddIdentity<AppUser, AppRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
    })
    .AddMongoDbStores<AppUser, AppRole, Guid>(
        mongoConnection, mongoDatabase)
    .AddDefaultTokenProviders();

// IdentityOptions
builder.Services.Configure<IdentityOptions>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;
    options.Password.RequiredUniqueChars = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedAccount = true;
});

// Cookie config
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/login/";
    options.LogoutPath = "/logout/";
    options.AccessDeniedPath = "/khongduoctruycap.html";
});

// External login
builder.Services.AddAuthentication()
    .AddGoogle(options => {
        var gconfig = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = gconfig["ClientId"];
        options.ClientSecret = gconfig["ClientSecret"];
        options.CallbackPath = "/dang-nhap-tu-google";
    })
    .AddFacebook(options => {
        var fconfig = builder.Configuration.GetSection("Authentication:Facebook");
        options.AppId = fconfig["AppId"];
        options.AppSecret = fconfig["AppSecret"];
        options.CallbackPath = "/dang-nhap-tu-facebook";
    });

// Authorization policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ViewManageMenu", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(RoleName.Administrator);
    });
});


builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddTransient<AdminSidebarService>();
builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IReceiptInRepository, ReceiptInRepository>();
builder.Services.AddScoped<IReceiptInService, ReceiptInService>();
builder.Services.AddScoped<IReceiptOutRepository, ReceiptOutRepository>();
builder.Services.AddScoped<IReceiptOutService, ReceiptOutService>();


var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
