using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Task = BusinessObject.Task;

namespace DataAccess;

public class DocumentManagementSystemDbContext : DbContext
{
    public DocumentManagementSystemDbContext() { }

    public DocumentManagementSystemDbContext(DbContextOptions<DocumentManagementSystemDbContext> options) : base(options) { }
    
    public virtual DbSet<ArchivedDocument> ArchivedDocuments { get; set; }
    public virtual DbSet<AttachmentArchivedDocument> AttachmentArchivedDocuments { get; set; }
    public virtual DbSet<AttachmentDocument> AttachmentDocuments { get; set; }
    public virtual DbSet<Comment> Comments { get; set; }
    public virtual DbSet<Deadline> Deadlines { get; set; }
    public virtual DbSet<Division> Divisions { get; set; }
    public virtual DbSet<Document> Documents { get; set; }
    public virtual DbSet<DocumentFileExtension> DocumentFileExtensions { get; set; }
    public virtual DbSet<DocumentType> DocumentTypes { get; set; }
    public virtual DbSet<Permission> Permissions { get; set; }
    public virtual DbSet<Resource> Resources { get; set; }
    public virtual DbSet<ResourcePermission> ResourcePermissions { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<RolePermission> RolePermissions { get; set; }
    public virtual DbSet<Step> Steps { get; set; }
    public virtual DbSet<Task> Tasks { get; set; }
    public virtual DbSet<TaskUser> TaskUsers { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserDocument> UserDocuments { get; set; }
    public virtual DbSet<UserDocumentPermission> UserDocumentPermissions { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<VerificationOtp> VerificationOtps { get; set; }
    public virtual DbSet<Workflow> Workflows { get; set; }
    
    private static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder => 
    {
        builder.AddConsole(); 
    });
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(GetConnectionString())
                .EnableSensitiveDataLogging() // Bật log dữ liệu nhạy cảm
                .UseLoggerFactory(MyLoggerFactory) // Kích hoạt logger
                .EnableDetailedErrors(); // Hiển thị lỗi chi tiết;
        }
    }
    
    private string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .Build();
        var strConn = config.GetConnectionString("DB");

        return strConn;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }

}