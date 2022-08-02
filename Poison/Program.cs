using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Poison.Data;
using Poison.Models;
using Poison.Services;
using Poison.Services.Factories;
using Poison.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString = DataUtility.GetConnectionString(builder.Configuration);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

// Add services to the container.

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<BTUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddClaimsPrincipalFactory<BTUserClaimsPrincipalFactory>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

builder.Services.AddScoped<IPoisonProjectService, PoisonProjectService>();
builder.Services.AddScoped<IPoisonTicketService, PoisonTicketService>();
builder.Services.AddScoped<IPoisonRolesService, PoisonRolesService>();
builder.Services.AddScoped<IPoisonFileService, PoisonFileService>();
builder.Services.AddScoped<IPoisonCompanyInfoService, PoisonCompanyInfoService>();
builder.Services.AddScoped<IPoisonInviteService, PoisonInviteService>();
builder.Services.AddScoped<IEmailSender, PoisonEmailService>();
builder.Services.AddScoped<IPoisonTicketHistoryService, PoisonTicketHistoryService>();
builder.Services.AddScoped<IPoisonNotificationService, PoisonNotificationService>();
builder.Services.AddScoped<IPoisonLookupService, PoisonLookupService>();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddMvc();

var app = builder.Build();

var scope = app.Services.CreateScope();
await DataUtility.ManageDataAsync(scope.ServiceProvider);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=LandingPage}/{id?}");
app.MapRazorPages();

app.Run();
