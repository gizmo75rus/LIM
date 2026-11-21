using System;

namespace LIM.Connector.Abstraction
{
    /// <summary>
    /// Интерфейс обработчика сообщений
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Событие, возникающее при получении сообщения
        /// </summary>
        event EventHandler<string> MessageReceived;
    }
}