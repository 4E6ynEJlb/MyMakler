using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace LogicsLib.Services
{
    /// <summary>
    /// Сервис для удаления устаревших объявлений (см. время жизни в конфиге)
    /// </summary>
    public class OldAdsService : IHostedService
    {
        public OldAdsService(IServiceScopeFactory serviceScopeFactory, ILogger<OldAdsService> logger)
        {
            _Logics = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogics>();
            _Logger = logger;
        }
        private ILogics _Logics;
        private ILogger _Logger;
        public async Task StartAsync(CancellationToken token)
        {
            _Logger.LogDebug("Starting service DOA");
            _Logics.RemoveOldAds(token);
        }
        public async Task StopAsync(CancellationToken token)
        {
            _Logger.LogDebug("Stopping service DOA");
        }
    }
}
