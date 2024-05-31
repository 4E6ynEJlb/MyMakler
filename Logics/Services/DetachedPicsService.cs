using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace LogicsLib.Services
{
    /// <summary>
    /// Сервис для удаления фотографий, названия которых не фигурируют в бд
    /// </summary>
    public class DetachedPicsService : IHostedService
    {
        public DetachedPicsService(IServiceScopeFactory serviceScopeFactory, ILogger<DetachedPicsService> logger)
        {
            _Logics = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogics>();
            _Logger = logger;
        }
        private ILogics _Logics;
        private ILogger _Logger;
        public async Task StartAsync(CancellationToken token)
        {            
            _Logger.LogDebug("Starting service DPS");
            _Logics.DeleteDetachedPics(token);
        }
        public async Task StopAsync(CancellationToken token)
        {
            _Logger.LogDebug("Stopping service DPS");
        }
    }
}
