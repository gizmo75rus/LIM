using AstmDriver.Abstractions;
using AstmDriver.Commons;
using AstmDriver.Commons.ASTM;
using AstmDriver.Exceptions;
using AstmDriver.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AstmDriver.Core;

namespace AstmDriver.Behavior.MessageWorker
{
    public abstract class AbstractAstmWorker : IMessageWorker
    {
        internal readonly IHost _host;
        internal readonly ILogger _logger;
        internal const char CR = '\r';

        internal ConcurrentQueue<Message> _messages = null;

        internal Message _currentMessage = null;
        internal Analyzer _analyzer = null;
        public event EventHandler<string> OnError = delegate { };
        public event EventHandler<string> OnDataReciveid = delegate { };
        public event EventHandler<string> OnDataTransivied = delegate { };
        public event EventHandler<DateTime> OnLastEOT = delegate { };
        public event EventHandler<string> OnInfo = delegate { };

        public AbstractAstmWorker(IHost host, ILogger logger, Analyzer analyzer)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
 _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
            
            _host.OnReceiveString += _host_OnReceiveString;
            _host.OnEOTReceived += _host_OnEOTReceived;
            _host.OnConnectionClosed += _host_OnConnectionClosed;

            
            _messages = new ConcurrentQueue<Message>();
        }

        private void _host_OnConnectionClosed(object sender, EventArgs e)
        {
            _logger.Debug("Host on connection closed");
        }
        private void _host_OnReceiveString(object sender, Commons.ChannelDataEventArgs e)
        {
            try
            {
                string[] records = e.Data.Split(new char[1] {   '\r' });
                int i = 0;

                if (records == null)
                {
                    return;
                }
                for (; i < (int)records.LongLength; i++)
                {
                    string r = records[i];
                    if (!string.IsNullOrEmpty(r))
                    {
                        OnDataReciveid?.Invoke(this, e.Data);
                        Task.Run(() =>
                        {
                            ParseReceivedRecord(r, e.RawData);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
            }
        }
        internal virtual void _host_OnEOTReceived(object sender, EventArgs e)
        {
            OnLastEOT?.Invoke(this, DateTime.Now);
            _logger.Info("Конец передачи");

            ProceesData();

        }

        internal virtual void ParseReceivedRecord(string record, string rawData)
        {
            var recordType = record[0];
            if (recordType == 'H')
            {
                _logger.Info("Начало пакета");
                _currentMessage = new Message();
            }

            if (_currentMessage == null)
                return;

            _currentMessage.RawData.Add(rawData);
            _currentMessage.Records.Add(record);


            if (recordType == 'Q')
            {
                _currentMessage.IsQuery = true;
            }

            if (recordType == 'R')
            {
                _currentMessage.IsResult = true;
            }

            if (recordType == 'O')
            {
                _currentMessage.IsOrder = true;
            }

            if (recordType == 'L')
            {
                _logger.Info("Конец пакета");
                _messages.Enqueue(_currentMessage.Clone());
                _currentMessage = null;
            }
        }

        internal virtual void ProceesData()
        {
            Task.Run(() =>
            {
                try
                {
                    Message message;
                    if (_messages.TryDequeue(out message))
                    {
                        if (message.IsResult)
                        {
                            _logger.Info("Начата отправка результатов в БД");

                            ProcessResultMessage(message);

                            _logger.Info("Успех");
                        }

                        if (message.IsQuery)
                        {
                            _logger.Info("Обработка запроса задания от анализатора");
                            ProcessQueryMessage(message);
                        }

                        if (message.IsOrder)
                        {
                            _logger.Info("Обработка ордера");
                            ProcessOrderMessage(message);
                        }


                        if (_messages.Any())
                            ProceesData();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Не удалось обработать пакет:" + ex.ToString());
                    OnErrorInvoke("Ошибка, не удалось обработать пакет, см. лог");
                    //TODO запись битого пакета во временное хранилище
                }

                _currentMessage = null;
            });
        }

        internal virtual void ProcessQueryMessage(Message message) { }

        internal virtual void ProcessOrderMessage(Message message) { }

        internal virtual void ProcessResultMessage(Message message) {
            _logger.Info("Всего записей:" + message.Records.Count(x => x.StartsWith("R|")));
            foreach (var raw in message.RawData)
            {
                Dal.WriteLog(_host.GetClientIP(), raw, _analyzer.Port);
            }
        }

        internal virtual bool TrySendToAnalyzer(List<string> packetBuffer)
        {
            try
            {
                _logger.Trace("Host->Establishing");
                if (_host.EstablishSendMode())
                {
                    foreach (var line in packetBuffer)
                    {
                        _logger.Trace("Host.SendMessage=>");
                        _host.SendMessage($"{line}{CR}");
                        OnDataTransivied?.Invoke(this, line);
                        Thread.Sleep(50);
                    }

                    _logger.Trace("Host.StopSendMode()");
                    _host.StopSendMode();
                    return true;
                }
                else
                {
                    OnErrorInvoke("Не удалось переключиться в режим отправки сообщений");
                }
            }
            catch (HostConnectionException ex)
            {
                OnErrorInvoke("Не удалось отправить сообщение (HostConnectionException): " + ex.Message);
            }
            catch (Exception ex)
            {
                OnErrorInvoke("Не удалось отправить сообщение (Exception): " + ex.Message);
            }

            return false;

        }

        internal void OnErrorInvoke(string message)
        {
            _logger.Error(message);
            OnError?.Invoke(this, message);
        }

        internal void OnDataReciveidInvoke(string message)
        {
            OnDataReciveid?.Invoke(this, message);
        }

        internal void OnDataTransiveInvoke(string message)
        {
            OnDataTransivied?.Invoke(this, message);
        }

        internal void OnLastEOTInvoke()
        {
            OnLastEOT?.Invoke(this, DateTime.Now);
        }

        internal void OnInfoInvoke(string message)
        {
            OnInfo?.Invoke(this, message);
        }
    }
}
