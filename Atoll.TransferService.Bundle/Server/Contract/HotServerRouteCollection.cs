using System;
using System.Collections.Generic;

namespace Atoll.TransferService
{

    /// <summary>
    /// Коллекция маршрутов обработки запросов.
    /// </summary>
    public sealed class HotServerRouteCollection 
    {

        /// <summary>
        /// Словарь маршрутов запросов получения данных.
        /// </summary>
        private readonly Dictionary<string, IHotGetHandlerFactory> getRoutes;

        /// <summary>
        /// Словарь маршрутов запросов передачи данных.
        /// </summary>
        private readonly Dictionary<string, IHotPutHandlerFactory> putRoutes;

        /// <summary>
        /// Конструктор.
        /// </summary>
        public HotServerRouteCollection()
        {
            this.getRoutes = new Dictionary<string, IHotGetHandlerFactory>(StringComparer.OrdinalIgnoreCase);
            this.putRoutes = new Dictionary<string, IHotPutHandlerFactory>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Добавить маршрут запросов получения данных.
        /// </summary>
        /// <param name="route">маршрут.</param>
        /// <param name="factory">экземпляр фабрики обработчиков.</param>
        /// <returns>коллекция маршрутов обработки запросов.</returns>
        public HotServerRouteCollection RouteGet(string route, IHotGetHandlerFactory factory)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            this.getRoutes.Add(route, factory);

            return this;
        }

        /// <summary>
        /// Добавить маршрут запросов передачи данных.
        /// </summary>
        /// <param name="route">маршрут.</param>
        /// <param name="factory">экземпляр фабрики обработчиков.</param>
        /// <returns>коллекция маршрутов обработки запросов.</returns>
        public HotServerRouteCollection RoutePut(string route, IHotPutHandlerFactory factory)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            this.putRoutes.Add(route, factory);

            return this;
        }

        /// <summary>
        /// Добавить маршрут запросов получения и передачи данных.
        /// </summary>
        /// <param name="route">маршрут.</param>
        /// <param name="getFactory">экземпляр фабрики обработчиков запросов получения данных.</param>
        /// <param name="putFactory">экземпляр фабрики обработчиков запросов передачи данных.</param>
        /// <returns>коллекция маршрутов обработки запросов.</returns>
        public HotServerRouteCollection Route(string route, IHotGetHandlerFactory getFactory, IHotPutHandlerFactory putFactory)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));
            if (getFactory == null) throw new ArgumentNullException(nameof(getFactory));
            if (putFactory == null) throw new ArgumentNullException(nameof(putFactory));

            this.getRoutes.Add(route, getFactory);
            this.putRoutes.Add(route, putFactory);

            return this;
        }

    }

}