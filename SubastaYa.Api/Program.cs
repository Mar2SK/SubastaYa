using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Middleware;
using SubastaYa.Api.Repositories;
using SubastaYa.Api.Services;
using SubastaYa.Api.Workers;
using SubastaYa.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddScoped<IAuctionService, AuctionService>();

builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.AddScoped<IBidRepository, BidRepository>();
builder.Services.AddScoped<IBidService, BidService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<
    IAuctionClosingService,
    AuctionClosingService>();

builder.Services.AddHostedService<AuctionClosingWorker>();
builder.Services.AddSignalR();
builder.Services.AddScoped<
    IUserActivityRepository,
    UserActivityRepository>();

builder.Services.AddScoped<
    IUserActivityService,
    UserActivityService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext context =
        scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await DbInitializer.InitializeAsync(context);
}

app.UseMiddleware<ApiVersionMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<AuctionHub>("/hubs/auctions");

app.Run();