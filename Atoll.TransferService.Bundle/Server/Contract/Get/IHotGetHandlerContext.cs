namespace Atoll.TransferService
{
    /// <summary>
    /// Контекст запроса получения данных.
    /// </summary>
    public interface IHotGetHandlerContext 
    {
        /// <summary>
        /// Получить описатель запроса.
        /// </summary>
        HotGetHandlerRequest Request { get; }

        /// <summary>
        /// Получить описатель кадра данных для передачи.
        /// </summary>
        HotGetHandlerFrame Frame { get; }

        /// <summary>
        /// Задать успешный результат обработчика.
        /// </summary>
        /// <returns>экземпляр контекста запроса.</returns>
        IHotGetHandlerContext Ok();

        /// <summary>
        /// Задать неуспешный результат обработчика, свидетельствующий о некорректном запросе.
        /// </summary>
        /// <param name="message">опциональное сообщение для профилирования.</param>
        /// <returns>экземпляр контекста запроса.</returns>
        IHotGetHandlerContext BadRequest(string message = null);

        /// <summary>
        /// Задать неуспешный результат обработчика, свидетельствующий об отсутствии запрошенных данных.
        /// </summary>
        /// <param name="message">опциональное сообщение для профилирования.</param>
        /// <returns>экземпляр контекста запроса.</returns>
        IHotGetHandlerContext NotFound(string message = null);

        /// <summary>
        /// Задать неуспешный результат обработчика, свидетельствующий об ошибке при обработке запроса.
        /// </summary>
        /// <param name="message">опциональное сообщение для профилирования.</param>
        /// <returns>экземпляр контекста запроса.</returns>
        IHotGetHandlerContext Error(string message = null);

        /// <summary>
        /// Задать неуспешный результат обработчика, свидетельствующий о нереализованной логики обработки запроса.
        /// </summary>
        /// <param name="message">опциональное сообщение для профилирования.</param>
        /// <returns>экземпляр контекста запроса.</returns>
        IHotGetHandlerContext NotImplemented(string message = null);
    }
}