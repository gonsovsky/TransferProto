﻿using System;

// ReSharper disable once CheckNamespace
namespace Atoll.TransferService
{

    /// <summary>
    /// Контракт обработчика запроса сохранения данных.
    /// </summary>
    public interface IHotPutHandler : IDisposable
    {

        /// <summary>
        /// Выполнить инициализацию обработчика запроса.
        /// </summary>
        /// <param name="ctx">контекст запроса.</param>
        /// <returns>контекст запроса.</returns>
        IHotPutHandlerContext Open(IHotPutHandlerContext ctx);

        /// <summary>
        /// Выполнить итерацию приема данных.
        /// </summary>
        /// <param name="ctx">контекст запроса.</param>
        /// <returns>контекст запроса.</returns>
        IHotPutHandlerContext Write(IHotPutHandlerContext ctx);

    }

}