using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using BusinessObject.Option;
using DataAccess;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Repository;
using Repository.Caching;
using Repository.Caching.Impl;
using Repository.Impl;
using Serilog;
using Serilog.Debugging;
using Service;
using Serilog.Sinks.MongoDB;
using Service.Impl;
using Service.SignalRHub;
using Syncfusion.Licensing;
using RollingInterval = Serilog.Sinks.MongoDB.RollingInterval;


var builder = WebApplication.CreateBuilder(args);
// Cấu hình Serilog
// Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));

// Log.Logger = new LoggerConfiguration()
//     .MinimumLevel.Debug()
//     .WriteTo.Console()
//     .WriteTo.MongoDB(databaseUrl: "mongodb://superadmin:dmsCapstone@103.90.227.64:27017/DMS_MongoDB?authSource=admin", collectionName: "logs")
//     .CreateLogger();

// create sink instance with custom mongodb settings.
SelfLog.Enable(Console.Error);

Log.Logger= new LoggerConfiguration()
    .WriteTo.MongoDBBson(cfg =>
    {
        // Cấu hình MongoDB
        var mongoDbSettings = new MongoClientSettings
        {
            Credential = MongoCredential.CreateCredential("admin", "superadmin", "dmsCapstone"),
            Server = new MongoServerAddress("103.90.227.64", 27017),
            UseTls = true,
            AllowInsecureTls = true// Tắt TLS nếu server không yêu cầu
        };

        var mongoDbInstance = new MongoClient(mongoDbSettings).GetDatabase("DMS_MongoDB");

        // Sử dụng database DMS_MongoDB và collection logs
        cfg.SetMongoDatabase(mongoDbInstance);
        cfg.SetCollectionName("logs");
        // cfg.SetRollingInternal(RollingInterval.Month); // Bỏ comment nếu muốn rolling collection theo tháng
    })
    .CreateLogger();

// Sử dụng Serilog trong ứng dụng
builder.Host.UseSerilog();
// Add services to the container.
DotNetEnv.Env.Load();
var syncfusionLicenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);

builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
    serverOptions.Limits.MaxRequestBodySize = 209_715_200;
    //serverOptions.Limits.MinRequestBodyDataRate = null;
});
builder.Services.Configure<AppsetingOptions>(
    builder.Configuration.GetSection("ApiConfig")
);
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

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
            new string[] { }
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
        options.TokenValidationParameters = new TokenValidationParameters
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
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Cho phép lấy access token qua query (dành cho SignalR)
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
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

IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();

builder.Services.AddStackExchangeRedisCache(options =>
{
    // options.Configuration = configuration.GetConnectionString("Cache");
    options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions
    {
        EndPoints = { configuration.GetConnectionString("Cache") },
        ConnectTimeout = 10000,
        SyncTimeout = 10000,
        AbortOnConnectFail = false

    };
});
// Add logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // Logs to the terminal
builder.Logging.AddDebug();builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:3000",
                "http://120.72.85.88:8084",
                "http://nghetrenghetre.xyz:8084",
                "http://103.90.227.64:5290",
                "http://nghetrenghetre.xyz:8084",
                "http://signdoc-core.io.vn",
                "http://dms.signdoc-core.io.vn",
                "http://103.90.227.64:8084"
               
           
            ) // địa chỉ chạy file HTML
            .AllowAnyHeader()
            .AllowAnyMethod() 
            .AllowCredentials(); // rất quan trọng cho SignalR
    });
});

/*builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));*/
builder.Services.AddSingleton<ILoggerFactory, LoggerFactory>(); // Đảm bảo ILogger được inject
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();

builder.Services.AddScoped<IExternalApiService, ExternalApiService>();
builder.Services.AddScoped<IRedisCacheRepository, RedisCacheRepository>();
builder.Services.AddHostedService<StartupTaskService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<DocumentManagementSystemDbContext>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddHttpClient<IExternalApiService, ExternalApiService>();
builder.Services.AddScoped<IDigitalCertificateService, DigitalCertificateService>();
builder.Services.AddScoped<IDocumentSignatureRepository, DocumentSignatureRepository>();
builder.Services.AddScoped<IDocumentSignatureService, DocumentSignatureService>();
builder.Services.AddScoped<IDocumentWorkflowStatusRepository, DocumentWorkflowStatusRepository>();


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
builder.Services.AddScoped<IDigitalCertificateRepository, DigitalCertificateRepository>();
builder.Services.AddScoped<IDivisionRepository, DivisionRepository>();
builder.Services.AddScoped<IDivisionService, DivisionService>();
builder.Services.AddScoped<IDocumentTypeRepository, DocumentTypeRepository>();
builder.Services.AddScoped<IDocumentTypeService, DocumentTypeService>();
builder.Services.AddScoped<IWorkflowRepository, WorkflowRepository>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IStepRepository, StepRepository>();
builder.Services.AddScoped<IWorkflowFlowRepository, WorkflowFlowRepository>();
builder.Services.AddScoped<IFlowRepository, FlowRepository>();
builder.Services.AddScoped<IWorkflowFlowTransitionRepository, WorkflowFlowTransitionRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IArchivedDocumentRepository, ArchivedDocumentRepository>();
builder.Services.AddScoped<IArchiveDocumentService, ArchiveDocumentService>();
builder.Services.AddScoped<IArchiveDocumentSignatureRepository, ArchiveDocumentSignatureRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDocumentTypeWorkflowRepository, DocumentTypeWorkflowRepository>();
builder.Services.AddScoped<IFlowService, FlowService>();
builder.Services.AddScoped<IDocumentVersionRepository, DocumentVersionRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IUserDocPermissionRepository, UserDocPermissionRepository>();
builder.Services.AddScoped<IUserDocPermissionService, UserDocPermissionService>();
builder.Services.AddScoped<ISignApiService, SignApiService>();

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
// if (app.Environment.IsDevelopment())
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
app.UseCors("AllowFrontend");

app.MapHub<NotificationHub>("/notificationHub");

app.UseAuthentication();
app.UseAuthorization();
/*
app.UsePermissionMiddleware();
*/
app.MapControllers();

app.Run();