namespace Atoll.TransferService.Bundle.Server.Contract.Put
{

    /// <summary>
    /// Контракт фабрики обработчиков запросов отправки данных.
    /// </summary>
    public interface IHotPutHandlerFactory 
    {

        /// <summary>
        /// Выполнить создание экземпляра обработчика.
        /// </summary>
        /// <param name="ctx">контекст запроса.</param>
        /// <returns>экземпляр обработчика.</returns>
        IHotPutHandler Create(IHotPutHandlerContext ctx);

    }

}