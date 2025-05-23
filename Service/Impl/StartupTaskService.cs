using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Service.Impl;

public class StartupTaskService : BackgroundService
{
    private readonly ILogger<StartupTaskService> _logger;

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public StartupTaskService(IServiceScopeFactory serviceScopeFactory, ILogger<StartupTaskService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background task for API and Permission update started.");

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            try
            {
                var resourceService = scope.ServiceProvider.GetRequiredService<IResourceService>();
                var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
                var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();
                var roleResourceService = scope.ServiceProvider.GetRequiredService<IRoleResourceService>();

                // Kiểm tra và cập nhật Permission nếu có thay đổi
                await permissionService.SeedPermissionsAsync();

                // Kiểm tra và cập nhật Resource từ API
                await resourceService.ScanAndSaveResourcesAsync();

                // Kiểm tra và cập nhật Role nếu có thay đổi
                await roleService.SeedRolesAsync();

                // Kiểm tra và cập nhật RoleResource nếu có thay đổi
                await roleResourceService.ScanAndSaveRoleResourcesAsync();

                _logger.LogInformation("API, Permission and Role update completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during API and Permission update.");
            }
        }
    }
}