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
    
    public void EnsurePgCryptoExtension()
    {
        // Chạy lệnh tạo extension
        Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS \"pgcrypto\";");
    }
    
    /*using (var context = new ApplicationDbContext(options))
    {
        context.EnsurePgCryptoExtension();
    }*/

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArchivedDocument>(entity =>
        {
            entity.ToTable("ArchivedDocument");
            entity.HasKey(e => e.ArchivedDocumentId);
            entity.Property(e => e.ArchivedDocumentId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ArchivedDocumentName);
            entity.Property(e => e.ArchivedDocumentUrl);
            entity.Property(e => e.CreatedDate);

            entity.HasMany(e => e.UserDocumentPermissions)
                .WithOne(e => e.ArchivedDocument)
                .HasForeignKey(e => e.ArchivedDocumentId);
            entity.HasMany(e => e.AttachmentArchivedDocuments)
                .WithOne(e => e.ArchivedDocument)
                .HasForeignKey(a => a.ArchivedDocumentId);

        });

        modelBuilder.Entity<AttachmentArchivedDocument>(entity =>
        {
            entity.ToTable("AttachmentArchivedDocument");
            entity.HasKey(e => e.AttachmentArchivedDocumentId);
            entity.Property(e => e.AttachmentArchivedDocumentId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AttachmentName);
            entity.Property(e => e.AttachmentUrl);
            entity.Property(e => e.IsDeleted);
        });

        modelBuilder.Entity<AttachmentDocument>(entity =>
        {
            entity.ToTable("AttachmentDocument");
            entity.HasKey(e => e.AttachmentDocumentId);
            entity.Property(e => e.AttachmentDocumentId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AttachmentDocumentName);
            entity.Property(e => e.AttachmentDocumentUrl);
            entity.Property(e => e.IsDeleted);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("Comment");
            entity.HasKey(e => e.CommentId);
            entity.Property(e => e.CommentId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CommentContent);
            entity.Property(e => e.CreateDate);
            entity.Property(e => e.IsDeleted);
        });

        modelBuilder.Entity<Deadline>(entity =>
        {
            entity.ToTable("Deadline");
            entity.HasKey(e => e.DeadlineId);
            entity.Property(e => e.DeadlineId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title);
            entity.Property(e => e.Description);
            entity.Property(e => e.StartDate);
            entity.Property(e => e.EndDate);
            entity.Property(e => e.ReminderDate);
            entity.Property(e => e.IsDeleted);
        });

        modelBuilder.Entity<Division>(entity =>
        {
            entity.ToTable("Division");
            entity.HasKey(e => e.DivisionId);
            entity.Property(e => e.DivisionId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.DivisionName);
            entity.Property(e => e.IsDeleted);
            
            entity.HasMany(e => e.Users)
                .WithOne(e => e.Division)
                .HasForeignKey(e => e.DivisionId);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Document");
            entity.HasKey(e => e.DocumentId);
            entity.Property(e => e.DocumentId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.DocumentName);
            entity.Property(e => e.DocumentUrl);
            entity.Property(e => e.DocumentNumber);
            entity.Property(e => e.DocumentCode);
            entity.Property(e => e.CreatedDate);
            entity.Property(e => e.DocumentStatus);
            entity.Property(e => e.DocumentPriority);
            entity.Property(e => e.IsTemplate);
            entity.Property(e => e.IsDeleted);
            entity.Property(e => e.IsActive);
            
            entity.HasOne(e => e.Deadline)
                .WithOne(e => e.Document)
                .HasForeignKey<Deadline>(e => e.DeadlineId);
            entity.HasMany(e => e.Tasks)
                .WithOne(e => e.Document)
                .HasForeignKey(e => e.DocumentId);
            entity.HasMany(e => e.UserDocuments)
                .WithOne(e => e.Document)
                .HasForeignKey(e => e.DocumentId);
            entity.HasMany(e => e.AttachmentDocuments)
                .WithOne(e => e.Document)
                .HasForeignKey(e => e.DocumentId);
        });

        modelBuilder.Entity<DocumentFileExtension>(entity =>
        {
            entity.ToTable("DocumentFileExtension");
            entity.HasKey(e => e.DocumentFileExtensionId);
            entity.Property(e => e.DocumentFileExtensionId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.DocumentFileExtensionName);
            entity.Property(e => e.IsDeleted);
            
            entity.HasMany(e => e.Documents)
                .WithOne(e => e.DocumentFileExtension)
                .HasForeignKey(e => e.DocumentFileExtensionId);
            entity.HasMany(e => e.AttachmentDocuments)
                .WithOne(e => e.DocumentFileExtension)
                .HasForeignKey(e => e.DocumentFileExtensionId);
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.ToTable("DocumentType");
            entity.HasKey(e => e.DocumentTypeId);
            entity.Property(e => e.DocumentTypeId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.DocumentTypeName);
            entity.Property(e => e.IsDeleted);
            
            entity.HasMany(e => e.Workflows)
                .WithOne(e => e.DocumentType)
                .HasForeignKey(e => e.DocumentTypeId);
            entity.HasMany(e => e.Documents)
                .WithOne(e => e.DocumentType)
                .HasForeignKey(e => e.DocumentTypeId);
            entity.HasMany(e => e.ArchivedDocuments)
                .WithOne(e => e.DocumentType)
                .HasForeignKey(e => e.DocumentTypeId);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permission");
            entity.HasKey(e => e.PermissionId);
            entity.Property(e => e.PermissionId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.PermissionName);
            
            entity.HasMany(e => e.RolePermissions)
                .WithOne(e => e.Permission)
                .HasForeignKey(e => e.PermissionId);
            entity.HasMany(e => e.UserDocumentPermissions)
                .WithOne(e => e.Permission)
                .HasForeignKey(e => e.PermissionId);
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.ToTable("Resource");
            entity.HasKey(e => e.ResourceId);
            entity.Property(e => e.ResourceId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.ResourceApi);
            entity.Property(e => e.ResourceName);
            
            entity.HasMany(e => e.ResourcePermissions)
                .WithOne(e => e.Resource)
                .HasForeignKey(e => e.ResourceId);
        });

        modelBuilder.Entity<ResourcePermission>(entity =>
        {
            entity.ToTable("ResourcePermission");
            entity.HasKey(e => e.ResourcePermissionId);
            entity.Property(e => e.ResourcePermissionId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.RoleName);
            entity.Property(e => e.CreatedDate);
            
            entity.HasMany(e => e.TaskUsers)
                .WithOne(e => e.Role)
                .HasForeignKey(e => e.RoleId);
            entity.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(e => e.RoleId);
            entity.HasMany(e => e.RolePermissions)
                .WithOne(e => e.Role)
                .HasForeignKey(e => e.RoleId);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermission");
            entity.HasKey(e => e.RolePermissionId);
            entity.Property(e => e.RolePermissionId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            
            entity.HasMany(e => e.ResourcePermissions)
                .WithOne(e => e.RolePermission)
                .HasForeignKey(e => e.RolePermissionId);
        });

        modelBuilder.Entity<Step>(entity =>
        {
            entity.ToTable("Step");
            entity.HasKey(e => e.StepId);
            entity.Property(e => e.StepId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.StepNumber);
            entity.Property(e => e.Action);
            entity.Property(e => e.IsDeleted);
            
            entity.HasMany(e => e.Tasks)
                .WithOne(e => e.Step)
                .HasForeignKey(e => e.StepId);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.ToTable("Task");
            entity.HasKey(e => e.TaskId);
            entity.Property(e => e.TaskId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title);
            entity.Property(e => e.Description);
            entity.Property(e => e.StartDate);
            entity.Property(e => e.EndDate);
            entity.Property(e => e.TaskStatus);
            entity.Property(e => e.TaskType);
            entity.Property(e => e.CreatedDate);
            entity.Property(e => e.IsDeleted);
            entity.Property(e => e.IsActive);
            
            entity.HasMany(e => e.TaskUsers)
                .WithOne(e => e.Task)
                .HasForeignKey(e => e.TaskId);
            entity.HasMany(e => e.Comments)
                .WithOne(e => e.Task)
                .HasForeignKey(e => e.TaskId);
        });

        modelBuilder.Entity<TaskUser>(entity =>
        {
            entity.ToTable("TaskUser");
            entity.HasKey(e => e.TaskUserId);
            entity.Property(e => e.TaskUserId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.IsCreatedTaskByUser);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.FullName);
            entity.Property(e => e.UserName);
            entity.Property(e => e.Email);
            entity.Property(e => e.Password);
            entity.Property(e => e.PhoneNumber);
            entity.Property(e => e.Address);
            entity.Property(e => e.Avatar);
            entity.Property(e => e.Gender);
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.UpdatedAt);
            entity.Property(e => e.FcmToken);
            entity.Property(e => e.IsDeleted);
            entity.Property(e => e.IsEnable);
            
            entity.HasOne(e => e.VerificationOtp)
                .WithOne(e => e.User)
                .HasForeignKey<VerificationOtp>(e => e.UserId);
            entity.HasMany(e => e.TaskUsers)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.Comments)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.UserDocuments)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.Deadlines)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.UserDocumentPermissions)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId);
        });

        modelBuilder.Entity<UserDocument>(entity =>
        {
            entity.ToTable("UserDocument");
            entity.HasKey(e => e.UserDocumentId);
            entity.Property(e => e.UserDocumentId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.IsCreatedDocumentByUser);
        });

        modelBuilder.Entity<UserDocumentPermission>(entity =>
        {
            entity.ToTable("UserDocumentPermission");
            entity.HasKey(e => e.UserDocumentPermissionId);
            entity.Property(e => e.UserDocumentPermissionId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedDate);
            entity.Property(e => e.IsDeleted);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRole");
            entity.HasKey(e => e.UserRoleId);
            entity.Property(e => e.UserRoleId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.IsPrimary);
        });

        modelBuilder.Entity<VerificationOtp>(entity =>
        {
            entity.ToTable("VerificationOtp");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Otp);
            entity.Property(e => e.ExpirationTime);
            entity.Property(e => e.IsTrue);
        });

        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.ToTable("Workflow");
            entity.HasKey(e => e.WorkflowId);
            entity.Property(e => e.WorkflowId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.WorkflowName);
            
            entity.HasMany(e => e.Steps)
                .WithOne(e => e.Workflow)
                .HasForeignKey(e => e.WorkflowId);
        });
    }

}