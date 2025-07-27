using ExcelVideoLabler.API.Extensions;
using ExcelVideoLabler.API.Hubs;
using ExcelVideoLabler.API.Middlewares;
using ExcelVideoLabler.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSqliteDbContext(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddServices(builder.Configuration);
builder.Services.AddSignalR();

// enable cors
builder.Services.AddCors(c =>
{
    c.AddPolicy("AllowOrigin", option => option
        .WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});


var app = builder.Build();

app.StartMigrationPending();

using (var scope = app.Services.CreateScope())
{
    var configService = scope.ServiceProvider.GetService<ConfigService>();
    if (configService != null)
    {
        await configService.Init(scope.ServiceProvider);
    } 
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowOrigin");

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthorization();

app.UseStaticFiles();

app.MapHub<VideoDowloadHub>("/VideoDowloadHub");
app.MapControllers();

app.Run();