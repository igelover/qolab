using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Qolab.API.Data;
using Qolab.API.Managers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo
        {
            Title = "Qolab API",
            Version = "v1"
        }
     );

    c.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, "Qolab.API.xml"));
});

var connectionString = builder.Configuration.GetConnectionString("QolabDb");
builder.Services.AddDbContext<DataContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped(typeof(PapersManager));
builder.Services.AddScoped(typeof(ArticlesManager));

var app = builder.Build().MigrateDatabase();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
