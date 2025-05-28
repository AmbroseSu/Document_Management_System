using Microsoft.EntityFrameworkCore.Storage;
using Repository.Caching;

namespace Repository;

public interface IUnitOfWork : IDisposable
{
    IUserRepository UserUOW { get; }
    IResourceRepository ResourceUOW { get; }
    IPermissionRepository PermissionUOW { get; }
    IRoleRepository RoleUOW { get; }
    IRoleResourceRepository RoleResourceUOW { get; }
    IUserRoleRepository UserRoleUOW { get; }
    IVerificationOtpRepository VerificationOtpUOW { get; }
    IDigitalCertificateRepository DigitalCertificateUOW { get; }
    IDivisionRepository DivisionUOW { get; }
    IDocumentTypeRepository DocumentTypeUOW { get; }
    IWorkflowRepository WorkflowUOW { get; }
    IStepRepository StepUOW { get; }
    IFlowRepository FlowUOW { get; }
    IWorkflowFlowTransitionRepository WorkflowFlowTransitionUOW { get; }
    IWorkflowFlowRepository WorkflowFlowUOW { get; }
    ITaskRepository TaskUOW { get; }
    IDocumentRepository DocumentUOW { get; }
    IDocumentSignatureRepository DocumentSignatureUOW { get; }
    IArchivedDocumentRepository ArchivedDocumentUOW { get; }
    IArchiveDocumentSignatureRepository ArchiveDocumentSignatureUOW { get; }
    IDocumentWorkflowStatusRepository DocumentWorkflowStatusUOW { get; }
    IDocumentTypeWorkflowRepository DocumentTypeWorkflowUOW { get; }
    IDocumentVersionRepository DocumentVersionUOW { get; }
    IRedisCacheRepository RedisCacheUOW { get; }
    ICommentRepository CommentUOW { get; }
    IUserDocPermissionRepository UserDocPermissionUOW { get; }
    IDocumentElasticRepository DocumentElasticUOW { get; }
    IAttachmentRepository AttachmentUOW { get; }
    IAttachmentArchivedRepository AttachmentArchivedUOW { get; }
    Task<int> SaveChangesAsync();
    
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}