using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Repository.Impl;

public class UnitOfWork : IUnitOfWork
{
    private readonly DocumentManagementSystemDbContext _context;
    private bool _disposed;

    public UnitOfWork(IUserRepository userUow, DocumentManagementSystemDbContext context,
        IResourceRepository resourceUow, IPermissionRepository permissionUow, IRoleRepository roleUow,
        IRoleResourceRepository roleResourceUow, IUserRoleRepository userRoleUow,
        IVerificationOtpRepository verificationOtpUow, IDigitalCertificateRepository digitalCertificateUow,
        IDivisionRepository divisionUow, IDocumentTypeRepository documentTypeUow, IWorkflowRepository workflowUow, IStepRepository stepUow, IFlowRepository flowUow, IWorkflowFlowTransitionRepository workflowFlowTransitionUow, IWorkflowFlowRepository workflowFlowUow)
    {
        UserUOW = userUow ?? throw new ArgumentNullException(nameof(userUow));
        _disposed = false;
        _context = context;
        ResourceUOW = resourceUow ?? throw new ArgumentNullException(nameof(resourceUow));
        PermissionUOW = permissionUow ?? throw new ArgumentNullException(nameof(permissionUow));
        ;
        RoleUOW = roleUow ?? throw new ArgumentNullException(nameof(roleUow));
        RoleResourceUOW = roleResourceUow ?? throw new ArgumentNullException(nameof(roleResourceUow));
        UserRoleUOW = userRoleUow ?? throw new ArgumentNullException(nameof(userRoleUow));
        VerificationOtpUOW = verificationOtpUow ?? throw new ArgumentNullException(nameof(verificationOtpUow));
        DigitalCertificateUOW = digitalCertificateUow ?? throw new ArgumentNullException(nameof(digitalCertificateUow));
        DivisionUOW = divisionUow ?? throw new ArgumentNullException(nameof(divisionUow));
        DocumentTypeUOW = documentTypeUow ?? throw new ArgumentNullException(nameof(documentTypeUow));
        WorkflowUOW = workflowUow;
        StepUOW = stepUow;
        FlowUOW = flowUow;
        WorkflowFlowTransitionUOW = workflowFlowTransitionUow;
        WorkflowFlowUOW = workflowFlowUow;
    }

    public IUserRepository UserUOW { get; }
    public IResourceRepository ResourceUOW { get; }
    public IPermissionRepository PermissionUOW { get; }
    public IRoleRepository RoleUOW { get; }
    public IRoleResourceRepository RoleResourceUOW { get; }
    public IUserRoleRepository UserRoleUOW { get; }
    public IVerificationOtpRepository VerificationOtpUOW { get; }
    public IDigitalCertificateRepository DigitalCertificateUOW { get; }
    public IDivisionRepository DivisionUOW { get; }
    public IDocumentTypeRepository DocumentTypeUOW { get; }
    public IWorkflowRepository WorkflowUOW { get; }
    public IStepRepository StepUOW { get; }
    public IFlowRepository FlowUOW { get; }
    public IWorkflowFlowTransitionRepository WorkflowFlowTransitionUOW { get; }
    public IWorkflowFlowRepository WorkflowFlowUOW { get; }

    public async Task<int> SaveChangesAsync()
    {
        //using var context = new DocumentManagementSystemDbContext();
        //return await _context.SaveChangesAsync();
        if (_context.ChangeTracker.HasChanges())
            return await _context.SaveChangesAsync();
        // Không có thay đổi, có thể log hoặc trả về 0
        return 0;
    }
    
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        _context.Database.SetCommandTimeout(600);
        return await _context.Database.BeginTransactionAsync();
    }
    
    public async Task CommitTransactionAsync()
    {
        await _context.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _context.Database.RollbackTransactionAsync();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // protected virtual void Dispose(bool disposing)
    // {
    //     if (!_disposed)
    //     {
    //         if (disposing)
    //             // Giải phóng các tài nguyên được quản lý
    //             _context?.Dispose();
    //
    //         _disposed = true;
    //     }
    // }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    if (_context.Database.CurrentTransaction != null)
                    {
                        _context.Database.CurrentTransaction.Dispose();
                    }
                    _context.Dispose();
                }
            }
            _disposed = true;
        }
    }
}