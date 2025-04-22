using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DataAccess;

public class DocumentManagementSystemDbContext : DbContext
{
    private static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

    public DocumentManagementSystemDbContext()
    {
    }

    public DocumentManagementSystemDbContext(DbContextOptions<DocumentManagementSystemDbContext> options) :
        base(options)
    {
    }

    public virtual DbSet<ArchivedDocument> ArchivedDocuments { get; set; }
    public virtual DbSet<ArchiveDocumentSignature> ArchiveDocumentSignatures { get; set; }
    public virtual DbSet<AttachmentArchivedDocument> AttachmentArchivedDocuments { get; set; }
    public virtual DbSet<AttachmentDocument> AttachmentDocuments { get; set; }
    public virtual DbSet<Comment> Comments { get; set; }
    public virtual DbSet<DigitalCertificate> DigitalCertificates { get; set; }
    public virtual DbSet<Division> Divisions { get; set; }
    public virtual DbSet<Document> Documents { get; set; }
    public virtual DbSet<DocumentSignature> DocumentSignatures { get; set; }
    public virtual DbSet<DocumentType> DocumentTypes { get; set; }
    public virtual DbSet<DocumentTypeWorkflow> DocumentTypeWorkflows { get; set; }
    public virtual DbSet<DocumentVersion> DocumentVersions { get; set; }
    public virtual DbSet<Flow> Flows { get; set; }
    public virtual DbSet<Permission> Permissions { get; set; }
    public virtual DbSet<Resource> Resources { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<RoleResource> RoleResources { get; set; }
    public virtual DbSet<Step> Steps { get; set; }
    public virtual DbSet<Tasks> Tasks { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserDocumentPermission> UserDocumentPermissions { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<VerificationOtp> VerificationOtps { get; set; }
    public virtual DbSet<Workflow> Workflows { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseNpgsql(GetConnectionString())
                .EnableSensitiveDataLogging() // Bật log dữ liệu nhạy cảm
                .UseLoggerFactory(MyLoggerFactory) // Kích hoạt logger
                .EnableDetailedErrors(); // Hiển thị lỗi chi tiết;
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
            entity.Property(e => e.ArchivedDocumentContent);
            entity.Property(e => e.NumberOfDocument);
            entity.Property(e => e.SignedBy);
            entity.Property(e => e.ArchivedDocumentUrl);
            entity.Property(e => e.CreatedDate);
            entity.Property(e => e.Sender);
            entity.Property(e => e.CreatedBy);
            entity.Property(e => e.ExternalPartner);
            entity.Property(e => e.ArchivedDocumentStatus);
            entity.Property(e => e.DateIssued);
            entity.Property(e => e.DateReceived);
            entity.Property(e => e.DateSented);
            entity.Property(e => e.DocumentRevokeId);
            entity.Property(e => e.DocumentReplaceId);
            entity.Property(e => e.Scope);
            entity.Property(e => e.IsTemplate);

            entity.HasMany(e => e.UserDocumentPermissions)
                .WithOne(e => e.ArchivedDocument)
                .HasForeignKey(e => e.ArchivedDocumentId);
            entity.HasMany(e => e.AttachmentArchivedDocuments)
                .WithOne(e => e.ArchivedDocument)
                .HasForeignKey(a => a.ArchivedDocumentId);
            entity.HasMany(e => e.ArchiveDocumentSignatures)
                .WithOne(e => e.ArchivedDocument)
                .HasForeignKey(e => e.ArchivedDocumentId);
            entity.HasMany(e => e.DocumentReplaces)
                .WithOne()
                .HasForeignKey(e => e.DocumentRevokeId);
            entity.HasMany(e => e.DocumentRevokes)
                .WithOne()
                .HasForeignKey(e => e.DocumentReplaceId);
            entity.HasMany(e => e.CreateDocuments)
                .WithOne(e => e.TemplateArchiveDocument)
                .HasForeignKey(e => e.TemplateArchiveDocumentId);
            entity.HasOne(e => e.FinalDocument)
                .WithOne(e => e.FinalArchiveDocument)
                .HasForeignKey<Document>(e => e.FinalArchiveDocumentId);
        });

        modelBuilder.Entity<ArchiveDocumentSignature>(entity =>
        {
            entity.ToTable("ArchiveDocumentSignature");
            entity.HasKey(e => e.ArchiveDocumentSignatureId);
            entity.Property(e => e.ArchiveDocumentSignatureId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.SignedAt);
            entity.Property(e => e.OrderIndex);
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

        modelBuilder.Entity<DigitalCertificate>(entity =>
        {
            entity.ToTable("DigitalCertificate");
            entity.HasKey(e => e.DigitalCertificateId);
            entity.Property(e => e.DigitalCertificateId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.SerialNumber);
            entity.Property(e => e.Issuer);
            entity.Property(e => e.ValidFrom);
            entity.Property(e => e.ValidTo);
            entity.Property(e => e.Subject);
            entity.Property(e => e.SignatureImageUrl);

            entity.HasMany(e => e.ArchiveDocumentSignatures)
                .WithOne(e => e.DigitalCertificate)
                .HasForeignKey(e => e.DigitalCertificateId);
            entity.HasMany(e => e.DocumentSignatures)
                .WithOne(e => e.DigitalCertificate)
                .HasForeignKey(e => e.DigitalCertificateId);
        });

        modelBuilder.Entity<Division>(entity =>
        {
            entity.ToTable("Division");
            entity.HasKey(e => e.DivisionId);
            entity.Property(e => e.DivisionId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.DivisionName);
            entity.Property(e => e.CreateAt);
            entity.Property(e => e.CreateBy);
            
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
            entity.Property(e => e.DocumentContent);
            entity.Property(e => e.NumberOfDocument);
            entity.Property(e => e.SignedBy);
            entity.Property(e => e.CreatedDate);
            entity.Property(e => e.UpdatedDate);
            entity.Property(e => e.Deadline);
            entity.Property(e => e.ProcessingStatus);
            entity.Property(e => e.DocumentPriority);
            entity.Property(e => e.Sender);
            entity.Property(e => e.DateReceived);
            entity.Property(e => e.DateIssued);
            entity.Property(e => e.IsDeleted);

            entity.HasMany(e => e.Tasks)
                .WithOne(e => e.Document)
                .HasForeignKey(e => e.DocumentId);
            entity.HasMany(e => e.AttachmentDocuments)
                .WithOne(e => e.Document)
                .HasForeignKey(e => e.DocumentId);
            entity.HasMany(e => e.DocumentVersions)
                .WithOne(e => e.Document)
                .HasForeignKey(e => e.DocumentId);
            entity.HasMany(e => e.DocumentWorkflowStatuses)
                .WithOne(e => e.Document)
                .HasForeignKey(e => e.DocumentId);
        });

        modelBuilder.Entity<DocumentSignature>(entity =>
        {
            entity.ToTable("DocumentSignature");
            entity.HasKey(e => e.DocumentSignatureId);
            entity.Property(e => e.DocumentSignatureId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.SignedAt);
            entity.Property(e => e.OrderIndex);
        });


        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.ToTable("DocumentType");
            entity.HasKey(e => e.DocumentTypeId);
            entity.Property(e => e.DocumentTypeId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.DocumentTypeName);
            entity.Property(e => e.CreateAt);
            entity.Property(e => e.Acronym);
            entity.Property(e => e.CreateBy);
            entity.Property(e => e.IsDeleted);

            entity.HasMany(e => e.DocumentTypeWorkflows)
                .WithOne(e => e.DocumentType)
                .HasForeignKey(e => e.DocumentTypeId);
            entity.HasMany(e => e.ArchivedDocuments)
                .WithOne(e => e.DocumentType)
                .HasForeignKey(e => e.DocumentTypeId);
            entity.HasMany(e => e.Documents)
                .WithOne(e => e.DocumentType)
                .HasForeignKey(e => e.DocumentTypeId);
        });

        modelBuilder.Entity<DocumentTypeWorkflow>(entity =>
        {
            entity.ToTable("DocumentTypeWorkflow");
            entity.HasKey(e => e.DocumentTypeWorkflowId);
            entity.Property(e => e.DocumentTypeWorkflowId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<DocumentVersion>(entity =>
        {
            entity.ToTable("DocumentVersion");
            entity.HasKey(e => e.DocumentVersionId);
            entity.Property(e => e.DocumentVersionId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.VersionNumber);
            entity.Property(e => e.DocumentVersionUrl);
            entity.Property(e => e.CreateDate);
            entity.Property(e => e.IsFinalVersion);
            
            entity.HasMany(e => e.DocumentSignatures)
                .WithOne(e => e.DocumentVersion)
                .HasForeignKey(e => e.DocumentVersionId);
            entity.HasMany(e => e.Comments)
                .WithOne(e => e.DocumentVersion)
                .HasForeignKey(e => e.DocumentVersionId);
        });

        modelBuilder.Entity<DocumentWorkflowStatus>(entity =>
        {
            entity.ToTable("DocumentWorkflowStatus");
            entity.HasKey(e => e.DocumentWorkflowStatusId);
            entity.Property(e => e.DocumentWorkflowStatusId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.StatusDocWorkflow);
            entity.Property(e => e.StatusDoc);
            entity.Property(e => e.UpdatedAt);
            
        });
        

        modelBuilder.Entity<Flow>(entity =>
        {
            entity.ToTable("Flow");
            entity.HasKey(e => e.FlowId);
            entity.Property(e => e.FlowId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.RoleStart);
            entity.Property(e => e.RoleEnd);

            entity.HasMany(e => e.WorkflowFlows)
                .WithOne(e => e.Flow)
                .HasForeignKey(e => e.FlowId);
            entity.HasMany(e => e.Steps)
                .WithOne(e => e.Flow)
                .HasForeignKey(e => e.FlowId);
        });
        

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permission");
            entity.HasKey(e => e.PermissionId);
            entity.Property(e => e.PermissionId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.PermissionName);

            entity.HasMany(e => e.Resources)
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

            entity.HasMany(e => e.RoleResources)
                .WithOne(e => e.Resource)
                .HasForeignKey(e => e.ResourceId);
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
            entity.Property(e => e.IsDeleted);

            entity.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(e => e.RoleId);
            entity.HasMany(e => e.RoleResources)
                .WithOne(e => e.Role)
                .HasForeignKey(e => e.RoleId);
            entity.HasMany(e => e.Steps)
                .WithOne(e => e.Role)
                .HasForeignKey(e => e.RoleId);
        });

        modelBuilder.Entity<RoleResource>(entity =>
        {
            entity.ToTable("RoleResource");
            entity.HasKey(e => e.RoleResourceId);
            entity.Property(e => e.IsDeleted);
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
            entity.Property(e => e.NextStepId);
            entity.Property(e => e.RejectStepId);
            entity.Property(e => e.IsDeleted);

            entity.HasMany(e => e.Tasks)
                .WithOne(e => e.Step)
                .HasForeignKey(e => e.StepId);
        });

        modelBuilder.Entity<Tasks>(entity =>
        {
            entity.ToTable("Tasks");
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
            entity.Property(e => e.UpdatedDate);
            entity.Property(e => e.TaskNumber);
            entity.Property(e => e.CreatedBy);
            entity.Property(e => e.IsDeleted);
            entity.Property(e => e.IsActive);


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
            entity.Property(e => e.IdentityCard);
            entity.Property(e => e.CreatedAt);
            entity.Property(e => e.UpdatedAt);
            entity.Property(e => e.FcmToken);
            entity.Property(e => e.Position);
            entity.Property(e => e.DateOfBirth);
            entity.Property(e => e.IsDeleted);
            entity.Property(e => e.IsEnable);


            entity.HasMany(e => e.VerificationOtps)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.Comments)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.UserDocumentPermissions)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.Tasks)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.Documents)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId);
            entity.HasMany(e => e.DigitalCertificates)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId);
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
            entity.Property(e => e.IsDeleted);
            entity.Property(e => e.AttemptCount);
        });

        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.ToTable("Workflow");
            entity.HasKey(e => e.WorkflowId);
            entity.Property(e => e.WorkflowId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.WorkflowName);
            entity.Property(e => e.Scope);
            entity.Property(e => e.RequiredRolesJson);
            entity.Property(e => e.CreateAt);
            entity.Property(e => e.CreateBy);
            entity.Property(e => e.IsAllocate);
            entity.Property(e => e.IsDeleted);

            entity.HasMany(e => e.WorkflowFlows)
                .WithOne(e => e.Workflow)
                .HasForeignKey(e => e.WorkflowId);
            entity.HasMany(e => e.DocumentTypeWorkflows)
                .WithOne(e => e.Workflow)
                .HasForeignKey(e => e.WorkflowId);
            entity.HasMany(e => e.DocumentWorkflowStatuses)
                .WithOne(e => e.Workflow)
                .HasForeignKey(e => e.WorkflowId);
        });
        
        
        modelBuilder.Entity<WorkflowFlow>(entity =>
        {
            entity.ToTable("WorkflowFlow");
            entity.HasKey(e => e.WorkflowFlowId);
            entity.Property(e => e.WorkflowFlowId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.FlowNumber);
            
            entity.HasMany(e => e.CurrentWorkflowFlowTransitions)
                .WithOne(e => e.CurrentWorkFlowFlow)
                .HasForeignKey(e => e.CurrentWorkFlowFlowId);
            entity.HasMany(e => e.NextWorkflowFlowTransitions)
                .WithOne(e => e.NextWorkFlowFlow)
                .HasForeignKey(e => e.NextWorkFlowFlowId);
            entity.HasMany(e => e.DocumentWorkflowStatuses)
                 .WithOne(e => e.CurrentWorkflowFlow)
                 .HasForeignKey(e => e.CurrentWorkflowFlowId);
        });
        
        modelBuilder.Entity<WorkflowFlowTransition>(entity =>
        {
            entity.ToTable("WorkflowFlowTransition");
            entity.HasKey(e => e.WorkflowFlowTransitionId);
            entity.Property(e => e.WorkflowFlowTransitionId)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Condition);
            entity.Property(e => e.IsDeleted);
        });
    }
}