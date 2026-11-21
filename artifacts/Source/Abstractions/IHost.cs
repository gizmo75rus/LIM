using AstmDriver.Commons;
using System;
using System.Threading.Tasks;

namespace AstmDriver.Abstractions
{
    /// <summary>
    /// Хост
    /// </summary>
    public interface IHost : IDisposable
    {
        /// <summary>
        /// Статус транспортного канала
        /// </summary>
        ChannelStatus Status { get; set; }
        
        /// <summary>
        /// Канал
        /// </summary>
        ITransportChannel TransportChannel { get; set; }
        
        /// <summary>
        /// Открыть соединение
        /// </summary>
        void Connect();

        /// <summary>
        /// Прервать соединение
        /// </summary>
        void DisConnect();

        /// <summary>
        /// Установить режим передачи
        /// </summary>
        /// <returns></returns>
        bool EstablishSendMode();

        /// <summary>
        /// Оправить сообщение
        /// </summary>
        /// <param name="message"></param>
        void SendMessage(string message);

        /// <summary>
        /// Запуск таймера отправки сообщений
        /// </summary>
        void StartReceiveTimeoutTimer();

        /// <summary>
        /// Остановить режим отправки
        /// </summary>
        void StopSendMode();

        /// <summary>
        /// Получить IP подключенного устройства
        /// </summary>
        /// <returns>IP адрес в строковом представлении</returns>
        string GetClientIP();

        /// <summary>
        /// Событе, соединение закрыто
        /// </summary>
        event EventHandler OnConnectionClosed;

        /// <summary>
        /// Событие, строка отправлена
        /// </summary>
        event EventHandler<ChannelDataEventArgs> OnReceiveString;

        /// <summary>
        /// Событие, истекло время ожидания после отправки
        /// </summary>
        event EventHandler OnReceiveTimeOut;

        /// <summary>
        /// Событие, от устройства получен символ EOT (End of transmission),
        /// </summary>
        event EventHandler OnEOTReceived;
        
        /// <summary>
        /// При возникновении ошибки
        /// </summary>
        event EventHandler<string> OnError;
        
        /// <summary>
        /// При переподключении
        /// </summary>
        event EventHandler<string> OnReconnect;
    }
}
