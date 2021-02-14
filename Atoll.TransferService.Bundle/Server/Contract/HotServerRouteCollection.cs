using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
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
        public readonly Dictionary<string, IHotGetHandlerFactory> GetRoutes;

        /// <summary>
        /// Словарь маршрутов запросов передачи данных.
        /// </summary>
        public readonly Dictionary<string, IHotPutHandlerFactory> PutRoutes;

        /// <summary>
        /// Конструктор.
        /// </summary>
        public HotServerRouteCollection()
        {
            this.GetRoutes = new Dictionary<string, IHotGetHandlerFactory>(StringComparer.OrdinalIgnoreCase);
            this.PutRoutes = new Dictionary<string, IHotPutHandlerFactory>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public static class HotServerRouteCollectionExtensions
    {

        /// <summary>
        /// Добавить маршрут запросов получения данных.
        /// </summary>
        /// <param name="route">маршрут.</param>
        /// <param name="factory">экземпляр фабрики обработчиков.</param>
        /// <returns>коллекция маршрутов обработки запросов.</returns>
        public static HotServerRouteCollection RouteGet(this HotServerRouteCollection routes, string route, IHotGetHandlerFactory factory)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            routes.GetRoutes.Add(route, factory);

            return routes;
        }

        /// <summary>
        /// Добавить маршрут запросов передачи данных.
        /// </summary>
        /// <param name="route">маршрут.</param>
        /// <param name="factory">экземпляр фабрики обработчиков.</param>
        /// <returns>коллекция маршрутов обработки запросов.</returns>
        public static HotServerRouteCollection RoutePut(this HotServerRouteCollection routes, string route, IHotPutHandlerFactory factory)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            routes.PutRoutes.Add(route, factory);

            return routes;
        }

        /// <summary>
        /// Добавить маршрут запросов получения данных.
        /// </summary>
        /// <param name="route">маршрут.</param>
        /// <param name="factory">экземпляр фабрики обработчиков.</param>
        /// <returns>коллекция маршрутов обработки запросов.</returns>
        public static HotServerRouteCollection RouteGet<THandler>(this HotServerRouteCollection routes, string route) where THandler :IHotGetHandler
        {
            if (route == null) throw new ArgumentNullException(nameof(route));

            routes.GetRoutes.Add(route, DefaultGetHandlerFactory<THandler>.Instance);

            return routes;
        }

        /// <summary>
        /// Добавить маршрут запросов передачи данных.
        /// </summary>
        /// <param name="route">маршрут.</param>
        /// <param name="factory">экземпляр фабрики обработчиков.</param>
        /// <returns>коллекция маршрутов обработки запросов.</returns>
        public static HotServerRouteCollection RoutePut<THandler>(this HotServerRouteCollection routes, string route) where THandler :IHotPutHandler
        {
            if (route == null) throw new ArgumentNullException(nameof(route));

            routes.PutRoutes.Add(route, DefaultPutHandlerFactory<THandler>.Instance);

            return routes;
        }
    }
}