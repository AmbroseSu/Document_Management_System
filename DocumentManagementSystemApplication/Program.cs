using System.Text.Json.Serialization;
using DataAccess;
using DataAccess.DAO;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Repository;
using Repository.Impl;
using Service;
using Service.Impl;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo() { Title = "Your API", Version = "v1" });

    // Configure Swagger to use the Bearer token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
builder.Logging.AddConsole();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

/*builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));*/
builder.Services.AddHostedService<StartupTaskService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<DocumentManagementSystemDbContext>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
/*builder.Services.AddScoped<UserDao>();*/

/*builder.Services.AddDbContext<DocumentManagementSystemDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DB")));*/

/*var applicationAssembly = typeof(Program).Assembly;
var serviceAssembly = typeof(IUserService).Assembly;
var repositoryAssembly = typeof(IUserRepository).Assembly;
builder.Services.AddHttpContextAccessor();


builder.Services.Scan(scan => scan
    .FromAssemblies(applicationAssembly, serviceAssembly, repositoryAssembly)
    .AddClasses(classes => classes.InNamespaces("Service", "Service.Impl", "Repository", "Repository.Impl")) // Thay bằng namespace thực tế
    .AsMatchingInterface()  // Đăng ký các lớp dựa trên interface phù hợp
    .WithScopedLifetime()
);*/


builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/*using (var scope = app.Services.CreateScope())
{
    var apiResourceService = scope.ServiceProvider.GetRequiredService<IResourceService>();
    await apiResourceService.ScanAndSaveResourcesAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<IPermissionService>();
    await seeder.SeedPermissionsAsync();
}*/

/*using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DocumentManagementSystemDbContext>();
    context.EnsurePgCryptoExtension();
}*/

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
/*
app.UsePermissionMiddleware();
*/
app.MapControllers();

app.Run();