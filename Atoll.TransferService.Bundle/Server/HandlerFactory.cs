﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using Atoll.TransferService.Bundle.Server.Contract;

namespace Atoll.TransferService.Bundle.Server
{
    /// <summary>
    /// Реализация фабрики обработчиков запросов получения данных, выполняющей вызов конструктора по-умолчанию.
    /// </summary>
    /// <typeparam name="THandler"></typeparam>
    public class HandlerFactory<THandler> : IFactory where THandler : IHandler
    {
        /// <summary>
        /// Синглетон-экземпляр фабрики.
        /// </summary>
        public static readonly HandlerFactory<THandler> Instance;

        /// <summary>
        /// Статический конструктор фабрики.
        /// </summary>
        static HandlerFactory() 
        {
            // Получаем тип обработчика.
            var handlerType = typeof(THandler);

            // Обработчик не должен быть абстрактным.
            if (handlerType.IsAbstract) 
                throw new ArgumentException(string.Concat(
                "Specified handler type '", handlerType.FullName, 
                "' is abstract and cannot be used with default factory."));
            
            // Обработчик не должен содержать Generic-параметры.
            if (handlerType.IsGenericTypeDefinition) 
                throw new ArgumentException(string.Concat(
                    "Specified handler type '", handlerType.FullName, 
                    "' is generic type definition and cannot be used with default factory."));
          
            // Ищем конструктор без параметров.
            var constructorInfo = handlerType.GetConstructor(BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, CallingConventions.Any, new Type[0], new ParameterModifier[0]);
            if (constructorInfo == null) 
                throw new ArgumentException(string.Concat(
                    "Specified handler type '", handlerType.FullName, 
                    "' does not contain public parameter-less constructor and cannot be used with default factory."));

            // Компилируем делегат вызова конструктора.
            var constructorMethod = Expression.Lambda<Func<THandler>>(Expression.New(constructorInfo)).Compile();

            // Создаем синглетон-экземпляр фабрики.
            Instance = new HandlerFactory<THandler>(constructorMethod);
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="factoryMethod">экземпляр метода, выполняющего создание.</param>
        protected HandlerFactory(Func<THandler> factoryMethod) 
        {
            this.factoryMethod = factoryMethod ?? throw new ArgumentNullException(nameof(factoryMethod));
        }

        /// <summary>
        /// Метод, выполняющий создание.
        /// </summary>
        private readonly Func<THandler> factoryMethod;

        /// <inheritdoc />
        IHandler IFactory.Create(IContext ctx) => 
            this.factoryMethod();
    }

    public class PutHandlerFactory<THandler> : HandlerFactory<THandler> where THandler : IHandler
    {
        protected PutHandlerFactory(Func<THandler> factoryMethod) : base(factoryMethod)
        {
        }
    }
}