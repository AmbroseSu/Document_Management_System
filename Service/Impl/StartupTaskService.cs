using System.ComponentModel.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Service.Impl;

public class StartupTaskService : BackgroundService
{
    
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<StartupTaskService> _logger;

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

                // Kiểm tra và cập nhật Resource từ API
                await resourceService.ScanAndSaveResourcesAsync();

                // Kiểm tra và cập nhật Permission nếu có thay đổi
                await permissionService.SeedPermissionsAsync();

                _logger.LogInformation("API and Permission update completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during API and Permission update.");
            }
        }
    }
}