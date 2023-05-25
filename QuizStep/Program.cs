using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using QuizStep.Data;
using QuizStep.Middleware;
using QuizStep.Services.Quiz;
using QuizStep.Services.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IValidationService, ValidationService>();
builder.Services.AddSingleton<IQuizService, QuizService>();

String? connectionString = builder.Configuration.GetConnectionString("Default");
MySqlConnection connection = new MySqlConnection(connectionString);
builder.Services.AddDbContext<DataContext>(options => options.UseMySql(
        connection, ServerVersion.AutoDetect(connection)
    )
);


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(360);
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

app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();
app.UseMiddleware<SessionAuthMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();