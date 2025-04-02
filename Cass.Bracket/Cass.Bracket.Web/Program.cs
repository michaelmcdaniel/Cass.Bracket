using Cass.Bracket.Web;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var mvcBuilder = builder.Services.AddControllersWithViews();
#if DEBUG
    mvcBuilder.AddRazorRuntimeCompilation();
#endif 

builder.Services.Configure<UserManager.Options>(options => {
    options.ConnectionString = builder.Configuration.GetValue<string>("connection-string")!;
    options.Timeout = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("connection-timeout-in-seconds", (int)options.Timeout.TotalSeconds));
});
builder.Services.AddTransient<UserManager>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
