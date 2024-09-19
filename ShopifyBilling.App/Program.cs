using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ShopifyBilling.App.Data;
using ShopifyBilling.App.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


//configure cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        // push back expiration time each time the user uses the app.
        options.SlidingExpiration = true;
        //configure authentication to expire after 1 day
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.LogoutPath = "/Auth/Logout";
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";

    });
var configuration = builder.Configuration;

//Get Sql Conn Str
string GetSqlConnectionString(IConfiguration config)
{
    var partialConnStr = config.GetConnectionString("DefaultConnection");
    var password = config.GetValue<string>("SQL_PASSWORD");
    var connStr = new SqlConnectionStringBuilder(partialConnStr)
    {
        Password = password,
        Authentication = SqlAuthenticationMethod.SqlPassword
    };

    return connStr.ToString();
}

var sqlConnectionString = GetSqlConnectionString(configuration);

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(sqlConnectionString);
});


//Add Isecrets to dependecy injection
builder.Services.AddSingleton<ISecrets, Secrets>();

var app = builder.Build();

//a helper method used to test if the database is active
/*using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<DataContext>();

    if (!dbContext.Database.CanConnect())
    {
        throw new NotImplementedException("Cannot connect to db!");
    }
}*/

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
