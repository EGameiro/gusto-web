using Dapper;
using GustoConvenio.Web.Auth;
using GustoConvenio.Web.Data;
using GustoConvenio.Web.Data.Repositories;
using Microsoft.AspNetCore.DataProtection;

// Dapper: mapeia snake_case do MySQL para PascalCase do C# automaticamente
DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

// Persiste chaves de Data Protection em disco — evita erro de antiforgery token após reciclar o processo
var keysPath = new DirectoryInfo(Path.Combine("H:\\root\\home\\egameiro-001\\www\\AppFood", "dp-keys"));
if (!keysPath.Exists) keysPath.Create();
builder.Services.AddDataProtection()
    .SetApplicationName("GustoConvenio")
    .PersistKeysToFileSystem(keysPath);

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// Banco
builder.Services.AddScoped<DbConnectionFactory>();

// Repositórios
builder.Services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
builder.Services.AddScoped<ICardapioRepository, CardapioRepository>();
builder.Services.AddScoped<IEmpresaRepository, EmpresaRepository>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IRestauranteRepository, RestauranteRepository>();

// Auth
builder.Services.AddScoped<TenantService>();

builder.Services.AddAuthentication("GustoCookie")
    .AddCookie("GustoCookie", opt =>
    {
        opt.LoginPath = "/Login";
        opt.LogoutPath = "/Login/Logout";
        opt.AccessDeniedPath = "/AcessoNegado";
        opt.SlidingExpiration = true;
        opt.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("Empresa",     p => p.RequireClaim("EmpresaId"));
    opt.AddPolicy("Admin",       p => p.RequireClaim("IsAdmin", "true"));
    opt.AddPolicy("SuperAdmin",  p => p.RequireClaim("IsSuperAdmin", "true"));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
