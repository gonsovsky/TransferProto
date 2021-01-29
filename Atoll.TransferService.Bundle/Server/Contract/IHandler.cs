using System;

namespace Atoll.TransferService.Bundle.Server.Contract
{
    /// <summary>
    /// Контракт обработчика запроса получения данных.
    /// </summary>
    public interface IHandler : IDisposable
    {
        /// <summary>
        /// Выполнить инициализацию обработчика запроса.
        /// </summary>
        /// <param name="ctx">контекст запроса.</param>
        /// <returns>контекст запроса.</returns>
        IContext Open(IContext ctx);

        /// <summary>
        /// Выполнить итерацию чтения данных.
        /// </summary>
        /// <param name="ctx">контекст запроса.</param>
        /// <returns>контекст запроса.</returns>
        IContext Read(IContext ctx);

        bool ReadEnd(IContext ctx);
    }

    public interface IHandlerPut : IHandler
    {
        /// <summary>
        /// Выполнить итерацию приема данных.
        /// </summary>
        /// <param name="ctx">контекст запроса.</param>
        /// <returns>контекст запроса.</returns>
        IContext Write(IContext ctx);
    }
}