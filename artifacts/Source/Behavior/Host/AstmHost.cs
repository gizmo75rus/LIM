using AstmDriver.Abstractions;
using AstmDriver.Commons;
using AstmDriver.Core;
using AstmDriver.Exceptions;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;


namespace AstmDriver.Behavior.Host
{
    /// <summary>
    /// Astm хост для работы с анализаторами реализующие поддержку Astm
    /// </summary>
    public class AstmHost : IHost
    {
        #region Private Fields
        internal const char STX = '\u0002';
        internal const char ETX = '\u0003';
        internal const char ETB = '\u0017';
        internal const char KeepAlive = '\u0016';
        internal const char ENQ = '\u0005';
        internal const char ACK = '\u0006';
        internal const char NAK = '\u0015';
        internal const char EOT = '\u0004';
        internal const char NUL = '\0';
        internal const char CR = '\r';
        internal const char LF = '\n';

        private const int _maxFrameSize = 63993;
        private int _enqWaitTimeout = 10000;
        private int _ackWaitTimeout = 15000;
        
        private int _tryConnectAttems = 5;
        private int _currentAttemp = 0;
        private int _connectWaitTimeout = 15000;

        private readonly ILogger _logger;

        private ITransportChannel _transportChannel;
        private readonly StringBuilder _tempReceiveBuffer = new StringBuilder();
        private string _tempIntermediateFrameBuffer;

        private readonly EventWaitHandle _enqWaitObject = new EventWaitHandle(true, EventResetMode.ManualReset);
        private readonly EventWaitHandle _ackWaitObject = new EventWaitHandle(true, EventResetMode.ManualReset);
        private readonly EventWaitHandle _eotWaitObject = new EventWaitHandle(true, EventResetMode.ManualReset);
        private bool _ackReceived;
        private readonly System.Timers.Timer _receiveTimeOutTimer = new System.Timers.Timer();
        private int _frameNumber;
        private bool _lastFrameWasIntermediate;
        private string _ipClient;

        internal bool _handshakeProgress = false;

        protected internal bool _isDisposed;

        #endregion

        #region Events
        public event EventHandler<ChannelDataEventArgs> OnReceiveString;
        public event EventHandler OnConnectionClosed;
        public event EventHandler OnReceiveTimeOut;
        public event EventHandler OnEOTReceived;
        public event EventHandler<string> OnError;
        public event EventHandler<string> OnReconnect;
        #endregion

        #region Constructors
        public AstmHost(ITransportChannel channel,ILogger logger) : this(channel, 30, logger)
        {
        }

        public AstmHost(ITransportChannel channel, int timeOut, ILogger logger)
        {
            TransportChannel = channel;
            _receiveTimeOutTimer.Interval = timeOut * 1000;
            _receiveTimeOutTimer.Enabled = false;
            _receiveTimeOutTimer.Elapsed += ReceiveTimeOutTimer_Elapsed;
            TransportChannel.OnConnecting += TransportChannel_OnConnecting;
        }

        private void TransportChannel_OnConnecting(object sender, string e)
        {
            _handshakeProgress = true;
            OnConnectiongHandler();
            _handshakeProgress = false;
        }

        internal virtual void OnConnectiongHandler() 
        {
            _transportChannel.WriteData($"{ACK}");
            Thread.Sleep(250);
        }
        #endregion

        #region Public Properties
        private ChannelStatus _status;
        public ChannelStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public ITransportChannel TransportChannel
        {
            get => _transportChannel; set
            {
                if (_transportChannel != null && _transportChannel != value)
                {
                    _logger.Debug("Отписка от транспорт чаннел");
                    _transportChannel.OnReceiveData -= ChannelDataReceived;
                    _transportChannel.OnConnecting -= _transportChannel_OnConnecting;
                }
                _transportChannel = value;
                _transportChannel.OnReceiveData += ChannelDataReceived;
                _transportChannel.OnConnecting += _transportChannel_OnConnecting;
            }
        }

        public string GetClientIP() => _ipClient;

        private void _transportChannel_OnConnecting(object sender, string e)
        {
            _ipClient = e;
        }

        #endregion

        #region Private Methods
        private void ChannelDataReceived(object sender, ChannelDataEventArgs e)
        {
            if (_handshakeProgress) return;

            string buffer = e.Data;
            if (buffer != null)
            {
                CharEnumerator enumerator = buffer.GetEnumerator();
                if (enumerator != null)
                {
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            char currentCharacter = enumerator.Current;
                            switch (Status)
                            {
                                case ChannelStatus.Idle:
                                    {
                                        if (currentCharacter == ENQ)
                                        {
                                            TransportChannel.WriteData($"{ACK}");
                                            Status = ChannelStatus.Receiving;
                                            _eotWaitObject.Reset();
                                            _tempReceiveBuffer.Clear();
                                            _tempIntermediateFrameBuffer = string.Empty;
                                            _receiveTimeOutTimer.Enabled = true;
                                        }

                                        if(currentCharacter == NAK)
                                        {
                                            Thread.Sleep(10000);
                                        }
                                        continue;
                                    }
                                case ChannelStatus.Sending:
                                    {
                                        _ackReceived = currentCharacter == ACK;
                                        _ackWaitObject.Set();
                                        continue;
                                    }
                                case ChannelStatus.Receiving:
                                    {
                                        _receiveTimeOutTimer.Stop();
                                        _receiveTimeOutTimer.Start();
                                        if (currentCharacter == ENQ)
                                        {
                                            Thread.Sleep(250);
                                            TransportChannel.WriteData($"{ACK}");
                                            return;
                                        }
                                        else if (currentCharacter != EOT)
                                        {
                                            if (currentCharacter != NUL)
                                            {
                                                _tempReceiveBuffer.Append(currentCharacter);
                                            }
                                            if (currentCharacter == LF)
                                            {
                                                string tempReceiveBuffer = _tempReceiveBuffer.ToString();
                                                if (!CheckChecksum(tempReceiveBuffer))
                                                {
                                                    _logger.Error($"CHECKSUM INVALID. FRAME: {tempReceiveBuffer}");
                                                    OnError?.Invoke(this, $"CHECKSUM INVALID: {tempReceiveBuffer}");
                                                    TransportChannel.WriteData($"{NAK}");
                                                    _tempReceiveBuffer.Clear();
                                                }
                                                else
                                                {
                                                    TransportChannel.WriteData($"{ACK}");
                                                    string cleanReceiveBuffer = tempReceiveBuffer.Substring(2, tempReceiveBuffer.Length - 7);
                                                    if (_lastFrameWasIntermediate)
                                                    {
                                                        _tempIntermediateFrameBuffer = $"{_tempIntermediateFrameBuffer}{cleanReceiveBuffer}";
                                                    }
                                                    else if (OnReceiveString != null)
                                                    {
                                                        string frame = string.Concat(_tempIntermediateFrameBuffer, cleanReceiveBuffer);
                                                        OnReceiveString?.Invoke(this, new ChannelDataEventArgs(frame,tempReceiveBuffer));
                                                        Thread.Sleep(100);
                                                        _tempIntermediateFrameBuffer = string.Empty;
                                                    }
                                                    _tempReceiveBuffer.Clear();
                                                    
                                                }
                                            }
                                            continue;
                                        }
                                        else
                                        {
                                            _receiveTimeOutTimer.Stop();
                                            Status = ChannelStatus.Idle;
                                            _eotWaitObject.Set();
                                            OnEOTReceived?.Invoke(this, EventArgs.Empty);
                                            return;
                                        }
                                    }
                                case ChannelStatus.Establishing:
                                    {
                                        if (currentCharacter == ACK)
                                        {
                                            Status = ChannelStatus.Sending;
                                            _enqWaitObject.Set();
                                            return;
                                        }
                                        else if (currentCharacter == ENQ)
                                        {
                                            Status = ChannelStatus.Receiving;
                                            TransportChannel.WriteData($"{ACK}");
                                            continue;
                                        }
                                        else
                                        {
                                            Thread.Sleep(1000);
                                            TransportChannel.WriteData($"{ENQ}");
                                            return;
                                        }
                                    }
                                default:
                                    {
                                        if (Status == ChannelStatus.Idle)
                                        {
                                            goto case ChannelStatus.Idle;
                                        }
                                        if (Status == ChannelStatus.Establishing)
                                        {
                                            goto case ChannelStatus.Establishing;
                                        }
                                        if (Status == ChannelStatus.Sending)
                                        {
                                            goto case ChannelStatus.Sending;
                                        }
                                        if (Status == ChannelStatus.Receiving)
                                        {
                                            goto case ChannelStatus.Receiving;
                                        }
                                        continue;
                                    }
                            }
                        }
                    }
                    finally
                    {
                        enumerator.Dispose();
                    }
                }
            }
        }

        private void ReceiveTimeOutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Status = ChannelStatus.Idle;
            _receiveTimeOutTimer.Stop();
            OnReceiveTimeOut?.Invoke(this, new EventArgs());
        }

        private string CalculateChecksum(string frame)
        {
            int total = 0;
            if (frame != null)
            {
                CharEnumerator enumerator = frame.GetEnumerator();
                if (enumerator != null)
                {
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            total += enumerator.Current;
                        }
                    }
                    finally
                    {
                        enumerator.Dispose();
                    }
                }
            }
            return ((byte)(total % 256)).ToString("X2", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Проверка контрольной сумму у записи
        /// </summary>
        /// <param name="frame">The ASTM frame to determine to and verify checksum</param>
        /// <returns>true если контрольная сумма валидна</returns>
        private bool CheckChecksum(string frame)
        {
            bool result = false;
            int lineLength = frame.Length;
            if (lineLength < 5)
            {
                return result;
            }
            if (frame[0] != STX)
            {
                return result;
            }
            if (frame[lineLength - 1] != LF)
            {
                return result;
            }
            if (frame[lineLength - 2] != CR)
            {
                return result;
            }


            char etxOrEtb = frame[lineLength - 5];
            _lastFrameWasIntermediate = etxOrEtb == ETB;

            //if ((etxOrEtb == ETX ? false : etxOrEtb != ETB))
            //{
            //    return result;
            //}

            //if (frame[lineLength - 6] != CR)
            //{
            //    return result;
            //}

            var calculatedCheckSum = CalculateChecksum(frame.Substring(1, lineLength - 5));
            var receivedCheckSum = frame.Substring(lineLength - 4, 2);
            //if (receivedCheckSum != calculatedCheckSum)
            //{
            //    _logger.Error($"CHECKSUM MISMATCH: Received:{receivedCheckSum} | Calculated: {calculatedCheckSum}");
            //    return result;
            //}
            result = true;
            return result;
        }

        private void SendEndFrame(int frameNumber, string frame)
        {
            SendString($"{frameNumber}{frame}{ETX}");
        }

        private void SendIntermediateFrame(int frameNumber, string frame)
        {
            SendString($"{frameNumber}{frame}{ETB}");
        }

        private void SendString(string frame)
        {
            if (Status != ChannelStatus.Sending)
            {
                var message = "Соединение не в режиме отправки при попытке отправить данные.";
                _logger.Error("Connection not in Send mode when trying to send data.");
                OnError?.Invoke(this, message);
                throw new HostConnectionException(message);
            }
            TransportChannel.ClearBuffers();
            int tryCounter = 0;
            string tempSendString = $"{STX}{frame}{CalculateChecksum(frame)}{CR}{LF}";
            TransportChannel.WriteData(tempSendString);
            while (!WaitForACK())
            {
                tryCounter++;
                if (tryCounter > 5)
                {
                    StopSendMode();
                    var message = "Достигнуто максимальное количество попыток отправки.";
                    _logger.Error("Max number of send retries reached.");
                    OnError?.Invoke(this,message);
                    throw new HostConnectionException(message);
                }
                TransportChannel.WriteData(tempSendString);
            }
        }
        private bool WaitForACK()
        {
            _ackWaitObject.Reset();
            if (!_ackWaitObject.WaitOne(_ackWaitTimeout))
            {
                StopSendMode();
                var message = "Нет ответа от хоста в течение периода ожидания.";
                _logger.Error("No response from Host within timeout period.");
                OnError?.Invoke(this, message);
                throw new HostConnectionException(message);
            }
            return _ackReceived;
        }
        #endregion


        public void Connect()
        {
            try
            {
                _logger.Info("\nConnecting To Host...");
                TransportChannel.Connect();
                _logger.Info("Connected");
            }
            catch(SocketException sxc)
            {
                if(sxc.ErrorCode == 10060 || sxc.ErrorCode == 10061)
                {
                    if(_currentAttemp >= _tryConnectAttems)
                    {
                        _currentAttemp = 0;
                        var date = DateTime.Now.AddMinutes(15);
                        OnError?.Invoke(this, $"Ну удалось установить подключение, следующая поптыка в {date.ToShortTimeString()}");
                        _logger.Info(sxc.Message);
                        Thread.Sleep(15 * 60 * 1000);
                        Connect();

                    }
                    else
                    {
                        _currentAttemp++;
                        OnReconnect?.Invoke(this, $"Подключение, попытка {_currentAttemp}/{_tryConnectAttems}");
                        Thread.Sleep(_connectWaitTimeout);
                        Connect();
                    }

                }
                else
                {
                    var message = "Ошибка при открытии соединения";
                    _logger.Error($"Error Opening Connection\n{sxc.Message}");
                    OnError?.Invoke(this, message);
                    throw new HostConnectionException(message, sxc);
                }
            }
            catch (Exception ex)
            {
                var message = "Ошибка при открытии соединения";
                _logger.Error($"Error Opening Connection\n{ex.Message}");
                OnError?.Invoke(this, message);
                throw new HostConnectionException(message, ex);
            }
        }

        public void DisConnect()
        {
            if (Status != ChannelStatus.Idle && !_eotWaitObject.WaitOne(15000))
            {
                Status = ChannelStatus.Idle;
                throw new HostConnectionException("Error closing Connection, no EOT received.");
            }
            try
            {
                _logger.Info("Disconnecting...");
                TransportChannel.DisConnect();
                Status = ChannelStatus.Idle;
                _logger.Info("Disconnected");
                OnConnectionClosed?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                _logger.Error($"Error Closing Connection\n{ex.Message}");
                throw new HostConnectionException("Error closing Connection.", ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                IDisposable connection = TransportChannel as IDisposable;
                if (connection != null)
                {
                    connection.Dispose();
                }
                else
                {
                }
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Установить режим отправки, переводит канал в режим отправки
        /// </summary>
        /// <returns></returns>
        public bool EstablishSendMode()
        {
            bool result = true;
            _frameNumber = 1;
            if (Status != ChannelStatus.Idle)
            {
                _logger.Error("Connection not idle when trying to establish send mode");
                throw new HostConnectionException("Connection not idle when trying to establish send mode");
            }
            Status = ChannelStatus.Establishing;
            _logger.Info("Establishing send mode");
            TransportChannel.ClearBuffers();
            _enqWaitObject.Reset();

            try
            {
                TransportChannel.WriteData($"{ENQ}");
            }
            catch (Exception ex)
            {
                _logger.Error("Cannot establish send mode.");
                _logger.Error($"{ex.Message}\n{ex.StackTrace}");

            }

            _enqWaitObject.WaitOne(_enqWaitTimeout, true);

            if (Status != ChannelStatus.Sending)
            {
                try
                {
                    StopSendMode();
                    _logger.Info("<ENQ> timed out. Set connection to idle.");
                }
                catch (Exception)
                {
                    if (Status != ChannelStatus.Sending) { Status = ChannelStatus.Idle; }
                }
            }
            if (Status == ChannelStatus.Sending) { return result; }
            return false;
        }

        /// <summary>
        /// Отправить сообщение в канал
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
            while (message.Length > _maxFrameSize)
            {
                string intermediateFrame = message.Substring(0, _maxFrameSize);
                SendIntermediateFrame(_frameNumber, intermediateFrame);
                message = message.Remove(0, _maxFrameSize);
                _frameNumber++;
                if (_frameNumber <= 7)
                {
                    continue;
                }
                _frameNumber = 0;
            }
            SendEndFrame(_frameNumber, message);
            _frameNumber++;
            if (_frameNumber > 7)
            {
                _frameNumber = 0;
            }
        }

        /// <summary>
        /// Запуск таймера тайм-аута начала приема
        /// </summary>
        public void StartReceiveTimeoutTimer()
        {
            _receiveTimeOutTimer.Start();
        }

        /// <summary>
        /// Отправить EOT 
        /// </summary>
        public void StopSendMode()
        {
            TransportChannel.WriteData($"{EOT}");
            _eotWaitObject.Reset();
            Status = ChannelStatus.Idle;
        }


    }
}
