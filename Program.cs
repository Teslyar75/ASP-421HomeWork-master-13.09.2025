using ASP_421.Data;
using ASP_421.Middleware;
using ASP_421.Services.Kdf;
using ASP_421.Services.Random;
using ASP_421.Services.Confirmation;
using Microsoft.EntityFrameworkCore;
using ASP_421.Services.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IRandomService, DefaultRandomService>();
builder.Services.AddSingleton<IKdfService, PbKdf1Service>();
builder.Services.AddSingleton<IStorageService, DiskStorageService>();
builder.Services.AddScoped<IConfirmationCodeService, ConfirmationCodeService>();

builder.Services.AddDbContext<DataContext>(options => 
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DataContext"))
);

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapStaticAssets();

app.UseAuthSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
