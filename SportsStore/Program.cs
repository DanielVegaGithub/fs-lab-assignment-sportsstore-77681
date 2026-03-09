using Microsoft.EntityFrameworkCore;
using SportsStore.Models;
using Microsoft.AspNetCore.Identity;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SportsStore application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddControllersWithViews();

    builder.Services.AddDbContext<StoreDbContext>(opts =>
    {
        opts.UseSqlServer(
            builder.Configuration["ConnectionStrings:SportsStoreConnection"]);
    });

    builder.Services.AddScoped<IStoreRepository, EFStoreRepository>();
    builder.Services.AddScoped<IOrderRepository, EFOrderRepository>();

    builder.Services.AddRazorPages();
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession();
    builder.Services.AddScoped<Cart>(sp => SessionCart.GetCart(sp));
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddServerSideBlazor();

    builder.Services.AddDbContext<AppIdentityDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration["ConnectionStrings:IdentityConnection"]));

    builder.Services.AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<AppIdentityDbContext>();

    var app = builder.Build();

    if (app.Environment.IsProduction())
    {
        app.UseExceptionHandler("/error");
    }

    app.UseSerilogRequestLogging();

    app.UseRequestLocalization(opts =>
    {
        opts.AddSupportedCultures("en-US")
            .AddSupportedUICultures("en-US")
            .SetDefaultCulture("en-US");
    });

    app.UseStaticFiles();
    app.UseSession();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute("catpage",
        "{category}/Page{productPage:int}",
        new { Controller = "Home", action = "Index" });

    app.MapControllerRoute("page", "Page{productPage:int}",
        new { Controller = "Home", action = "Index", productPage = 1 });

    app.MapControllerRoute("category", "{category}",
        new { Controller = "Home", action = "Index", productPage = 1 });

    app.MapControllerRoute("pagination",
        "Products/Page{productPage}",
        new { Controller = "Home", action = "Index", productPage = 1 });

    app.MapDefaultControllerRoute();
    app.MapRazorPages();
    app.MapBlazorHub();
    app.MapFallbackToPage("/admin/{*catchall}", "/Admin/Index");

    Log.Information("Ensuring seed data is populated");

    SeedData.EnsurePopulated(app);
    IdentitySeedData.EnsurePopulated(app);

    Log.Information("SportsStore application started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}