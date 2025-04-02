using Microsoft.EntityFrameworkCore.Storage;

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

    Task<int> SaveChangesAsync();
    
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}