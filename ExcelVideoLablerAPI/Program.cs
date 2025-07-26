using ExcelVideoLablerAPI.Data;
using ExcelVideoLablerAPI.Middlewares;
using ExcelVideoLablerAPI.Repositories;
using ExcelVideoLablerAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IVideoInfoRepository, VideoInfoRepository>();
builder.Services.AddScoped<IConfigRepository, ConfigRepository>();
builder.Services.AddScoped<VideoExcelService>();

builder.Services.AddSingleton<FileService>();
builder.Services.AddSingleton<ConfigService>();
builder.Services.AddSingleton<DownloadVideoService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var settingService = scope.ServiceProvider.GetRequiredService<ConfigService>();
    await settingService.Init(app.Services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthorization();

app.UseStaticFiles();

app.MapControllers();

app.Run();