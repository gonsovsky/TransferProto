namespace Atoll.TransferService.Bundle.Server.Contract
{
    /// <summary>
    /// Контракт фабрики обработчиков запросов получения данных.
    /// </summary>
    public interface IFactory 
    {
        /// <summary>
        /// Выполнить создание экземпляра обработчика.
        /// </summary>
        /// <param name="ctx">контекст запроса.</param>
        /// <returns>экземпляр обработчика.</returns>
        Handler.IHandler Create(IHotGetHandlerContext ctx);
    }
}