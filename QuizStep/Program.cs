using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using QuizStep.Data;
using QuizStep.Middleware;
using QuizStep.Services.Hash;
using QuizStep.Services.Kdf;
using QuizStep.Services.Quiz;
using QuizStep.Services.RandomService;
using QuizStep.Services.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IValidationService, ValidationService>();
builder.Services.AddSingleton<IQuizService, QuizService>();
builder.Services.AddSingleton<IRandomService, RandomService>();
builder.Services.AddSingleton<IHashService, HashService>();
builder.Services.AddSingleton<IKdfService, KdfService>();

String? connectionString = builder.Configuration.GetConnectionString("Default");
MySqlConnection connection = new MySqlConnection(connectionString);
builder.Services.AddDbContext<DataContext>
    (
        options => options.UseMySql(
            connection, ServerVersion.AutoDetect(connection)),
        ServiceLifetime.Transient
    );


builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(180);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
    pattern: "{controller=Account}/{action=Auth}/{id?}",
    defaults: new { controller = "Account", action = "Auth" });

app.Run();