using System.Text;
using System.Text.Json.Serialization;
using DataAccess;
using DataAccess.DAO;
using DocumentManagementSystemApplication.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository;
using Repository.Impl;
using Serilog;
using Serilog.Formatting.Json;
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

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false; // Cần bật true nếu chạy production
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true, // Thêm kiểm tra thời gian sống của token
            ValidateIssuerSigningKey = true, // Đảm bảo kiểm tra khóa ký token
            ValidAudience = builder.Configuration["JWT:Audience"],
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"] ?? throw new ArgumentNullException("JWT:Key"))
            ),
            //RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero // Giảm độ trễ token xuống 0 để token hết hạn đúng thời điểm
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

/*// ✅ Cấu hình Serilog để log JSON chuẩn, lưu vào Console, File và MongoDB
Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("Application", "DMS") // Gắn thêm thông tin nếu cần
    .WriteTo.Console(new JsonFormatter()) // Ghi log JSON ra console
    .WriteTo.File(new JsonFormatter(), "logs/log-.json", rollingInterval: RollingInterval.Day) // Ghi log JSON vào file
    .WriteTo.MongoDB(
        "mongodb://localhost:27017/DMS_DB",
        collectionName: "Logs",
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
    )
    .CreateLogger();

builder.Host.UseSerilog();*/

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
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRoleResourceRepository, RoleResourceRepository>();
builder.Services.AddScoped<IRoleResourceService, RoleResourceService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<IVerificationOtpRepository, VerificationOtpRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();



builder.WebHost.UseKestrel();

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

// Đăng ký middleware ghi log API
/*
app.UseMiddleware<ApiLoggingMiddleware>(); // ✅ Ghi log API request/response
*/

//app.UseSerilogRequestLogging();
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
/*
app.UsePermissionMiddleware();
*/
app.MapControllers();

app.Run();