using System;

namespace Atoll.TransferService
{

    /// <summary>
    /// Реализация сервера приема и передачи данных.
    /// </summary>
    public sealed class HotServer : IDisposable
    {

        public void Start()
        {

        }

        public void Stop()
        {

        }

        public HotServer UseConfig(HotServerConfiguration config)
        {
            return this;
        }

        public HotServer UseRoutes(HotServerRouteCollection routes)
        {
            return this;
        }

        public void Dispose()
        {
        }

    }

}