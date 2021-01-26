using System;

namespace Atoll.TransferService.Bundle.Server.Contract.Get
{
    /// <summary>
    /// Контракт обработчика запроса получения данных.
    /// </summary>
    public interface IHotGetHandler : IDisposable
    {
        /// <summary>
        /// Выполнить инициализацию обработчика запроса.
        /// </summary>
        /// <param name="ctx">контекст запроса.</param>
        /// <returns>контекст запроса.</returns>
        IHotGetHandlerContext Open(IHotGetHandlerContext ctx);

        /// <summary>
        /// Выполнить итерацию чтения данных.
        /// </summary>
        /// <param name="ctx">контекст запроса.</param>
        /// <returns>контекст запроса.</returns>
        IHotGetHandlerContext Read(IHotGetHandlerContext ctx);

        bool ReadEnd(IHotGetHandlerContext ctx);
    }
}