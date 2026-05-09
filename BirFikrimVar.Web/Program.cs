using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. MVC Servislerini Ekle
builder.Services.AddControllersWithViews();

// 2. API ile konuşacak HttpClient Yapılandırması (Port: 7284)
builder.Services.AddHttpClient("BirFikrimVarAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7284/api/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// 3. Web Arayüzü İçin Cookie Tabanlı Kimlik Doğrulama
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "BirFikrimVar.Session";
        options.LoginPath = "/Account/Login";         // Giriş yapılmamışsa buraya atar
        options.AccessDeniedPath = "/Account/AccessDenied"; // Yetki yoksa buraya atar
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);  // Cookie süresi
    });

var app = builder.Build();

// 4. HTTP Pipeline Yapılandırması
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Eski sürümlerde MapStaticAssets yerine bu kullanılır

app.UseRouting();

// KRİTİK: Önce Authentication sonra Authorization gelmeli
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();