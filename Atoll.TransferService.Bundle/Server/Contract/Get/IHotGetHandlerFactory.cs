namespace Atoll.TransferService
{

    /// <summary>
    /// Контракт фабрики обработчиков запросов получения данных.
    /// </summary>
    public interface IHotGetHandlerFactory 
    {

        /// <summary>
        /// Выполнить создание экземпляра обработчика.
        /// </summary>
        /// <param name="ctx">контекст запроса.</param>
        /// <returns>экземпляр обработчика.</returns>
        IHotGetHandler Create(IHotGetHandlerContext ctx);

    }

}