namespace Atoll.TransferService.Bundle.Server.Contract.Put
{
    /// <summary>
    /// Контекст запроса отправки данных.
    /// </summary>
    public interface IHotPutHandlerContext 
    {
        /// <summary>
        /// Получить описатель запроса.
        /// </summary>
        HotPutHandlerRequest Request { get; }

        /// <summary>
        /// Получить описатель кадра принятых данных.
        /// </summary>
        HotPutHandlerFrame Frame { get; }

        /// <summary>
        /// Задать успешный результат обработчика.
        /// </summary>
        /// <returns>экземпляр контекста запроса.</returns>
        IHotPutHandlerContext Ok();

        /// <summary>
        /// Задать неуспешный результат обработчика, свидетельствующий о некорректном запросе.
        /// </summary>
        /// <param name="message">опциональное сообщение для профилирования.</param>
        /// <returns>экземпляр контекста запроса.</returns>
        IHotPutHandlerContext BadRequest(string message = null);

        /// <summary>
        /// Задать неуспешный результат обработчика, свидетельствующий об ошибке при обработке запроса.
        /// </summary>
        /// <param name="message">опциональное сообщение для профилирования.</param>
        /// <returns>экземпляр контекста запроса.</returns>
        IHotPutHandlerContext Error(string message = null);

        /// <summary>
        /// Задать неуспешный результат обработчика, свидетельствующий о нереализованной логики обработки запроса.
        /// </summary>
        /// <param name="message">опциональное сообщение для профилирования.</param>
        /// <returns>экземпляр контекста запроса.</returns>
        IHotPutHandlerContext NotImplemented(string message = null);
    }
}