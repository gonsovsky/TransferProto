using System;
using System.Collections.Generic;
using Atoll.TransferService.Bundle.Server.Contract;

namespace Atoll.TransferService.Bundle.Server
{
    /// <summary>
    /// Коллекция маршрутов обработки запросов.
    /// </summary>
    public sealed class RoutesCollection
    {
        /// <summary>
        /// Словарь маршрутов запросов получения данных.
        /// </summary>
        public readonly Dictionary<string, IFactory> Routes;

        /// <summary>
        /// Конструктор.
        /// </summary>
        public RoutesCollection()
        {
            this.Routes = new Dictionary<string, IFactory>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public static class HotServerRouteCollectionExtensions
    {
        /// <summary>
        /// Добавить маршрут запросов получения данных.
        /// </summary>
        /// <param name="routesCollection"></param>
        /// <param name="route">маршрут.</param>
        /// <param name="factory">экземпляр фабрики обработчиков.</param>
        /// <returns>коллекция маршрутов обработки запросов.</returns>
        public static RoutesCollection RouteGet(this RoutesCollection routesCollection, string route, IFactory factory)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            routesCollection.Routes.Add(route, factory);
            return routesCollection;
        }

        /// <summary>
        /// Добавить маршрут запросов передачи данных.
        /// </summary>
        /// <param name="routesCollection"></param>
        /// <param name="route">маршрут.</param>
        /// <param name="factory">экземпляр фабрики обработчиков.</param>
        /// <returns>коллекция маршрутов обработки запросов.</returns>
        public static RoutesCollection RoutePut(this RoutesCollection routesCollection, string route, IFactory factory)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            routesCollection.Routes.Add(route, factory);
            return routesCollection;
        }

        /// <summary>
        /// Добавить маршрут запросов получения и передачи данных.
        /// </summary>
        /// <param name="routesCollection"></param>
        /// <param name="route">маршрут.</param>
        /// <param name="getFactory">экземпляр фабрики обработчиков запросов получения данных.</param>
        /// <returns>коллекция маршрутов обработки запросов.</returns>
        public static RoutesCollection Route(this RoutesCollection routesCollection, string route, IFactory getFactory)
        {
            if (route == null) throw new ArgumentNullException(nameof(route));
            if (getFactory == null) throw new ArgumentNullException(nameof(getFactory));
            //if (putFactory == null) throw new ArgumentNullException(nameof(putFactory));
            routesCollection.Routes.Add(route, getFactory);
            //routesCollection.PutRoutes.Add(route, putFactory);
            return routesCollection;
        }

        /// <summary>
        /// Добавить маршрут запросов получения данных.
        /// </summary>
        /// <param name="routesCollection"></param>
        /// <param name="route">маршрут.</param>
        /// <returns>коллекция маршрутов обработки запросов.</returns>
        public static RoutesCollection RouteGet<THandler>(this RoutesCollection routesCollection, string route) where THandler : IHandler
        {
            if (route == null) throw new ArgumentNullException(nameof(route));
            routesCollection.Routes.Add(route, HandlerFactory<THandler>.Instance);
            return routesCollection;
        }

        /// <summary>
        /// Добавить маршрут запросов передачи данных.
        /// </summary>
        /// <param name="routesCollection"></param>
        /// <param name="route">маршрут.</param>
        /// <returns>коллекция маршрутов обработки запросов.</returns>
        public static RoutesCollection RoutePut<THandler>(this RoutesCollection routesCollection, string route) where THandler : IHandler
        {
            if (route == null) throw new ArgumentNullException(nameof(route));
            routesCollection.Routes.Add(route, HandlerFactory<THandler>.Instance);
            return routesCollection;
        }
    }
}