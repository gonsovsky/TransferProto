using Atoll.TransferService.Bundle.Server.Handler;

namespace Atoll.TransferService.Bundle.Server.Contract
{
    /// <summary>
    /// Контекст запроса получения данных.
    /// </summary>
    public interface IContext 
    {
        /// <summary>
        /// Получить описатель запроса.
        /// </summary>
        Request Request { get; }

        /// <summary>
        /// Получить описатель кадра данных для передачи.
        /// </summary>
        Frame Frame { get; }

        /// <summary>
        /// Задать успешный результат обработчика.
        /// </summary>
        /// <returns>экземпляр контекста запроса.</returns>
        IContext Ok();

        /// <summary>
        /// Задать неуспешный результат обработчика, свидетельствующий о некорректном запросе.
        /// </summary>
        /// <param name="message">опциональное сообщение для профилирования.</param>
        /// <returns>экземпляр контекста запроса.</returns>
        IContext BadRequest(string message = null);

        /// <summary>
        /// Задать неуспешный результат обработчика, свидетельствующий об отсутствии запрошенных данных.
        /// </summary>
        /// <param name="message">опциональное сообщение для профилирования.</param>
        /// <returns>экземпляр контекста запроса.</returns>
        IContext NotFound(string message = null);

        /// <summary>
        /// Задать неуспешный результат обработчика, свидетельствующий об ошибке при обработке запроса.
        /// </summary>
        /// <param name="message">опциональное сообщение для профилирования.</param>
        /// <returns>экземпляр контекста запроса.</returns>
        IContext Error(string message = null);

        /// <summary>
        /// Задать неуспешный результат обработчика, свидетельствующий о нереализованной логики обработки запроса.
        /// </summary>
        /// <param name="message">опциональное сообщение для профилирования.</param>
        /// <returns>экземпляр контекста запроса.</returns>
        IContext NotImplemented(string message = null);

    }

}