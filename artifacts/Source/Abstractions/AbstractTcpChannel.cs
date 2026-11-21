using AstmDriver.Commons;
using System;
using System.Net.Sockets;
using System.Text;
using AstmDriver.Core;

namespace AstmDriver.Abstractions
{
    public abstract class AbstractTcpChannel : ITransportChannel
    {
        #region Private Properties
        internal ILogger _logger;
        internal Encoding _encoding;

        internal Socket _socket;

        #endregion

        public event EventHandler<ChannelDataEventArgs> OnReceiveData;
        public event EventHandler<ChannelDataEventArgs> OnTransiveString;
        public event EventHandler<string> OnConnecting;
        public event EventHandler<string> OnError;
        public event EventHandler<string> OnDisconnecting;

        #region Constructors
        public AbstractTcpChannel(string hostAddress, ushort port, string encoding,ILogger logger)
        {
            _hostAddress = hostAddress;
            _port = port;
            _logger = logger ?? LoggerFactory.GetLogger(typeof(AbstractTcpChannel));

            _diag_timer = new System.Timers.Timer(5000);


            _diag_timer.AutoReset = true;
            _diag_timer.Elapsed += _diag_timer_Elapsed;
            _diag_timer.Enabled = true;
            

            switch (encoding)
            {
                default:
                case "Default":
                    _encoding = Encoding.Default;
                    break;
                case "UTF8":
                    _encoding = Encoding.UTF8;
                    break;
                case "ASCII":
                    _encoding = Encoding.ASCII;
                    break;
                case "Unicode":
                    _encoding = Encoding.Unicode;
                    break;
                case "1251":
                    _encoding = Encoding.GetEncoding(1251);
                    break;
            }
        }

        internal bool _isConnected = false;

        public bool IsConnected {  get { return _isConnected; } }

        private void _diag_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if(_socket != null)
                {
                    _diag_timer.Enabled = false;

                    if (_socket.ProbeConnection() == false) 
                    {
                        _isConnected = false;
                    };
                }

            }catch(Exception ex)
            {
                _logger.Error("TcpChannel->DiagTimer->Elapsed->Message:" + Environment.NewLine+ ex.ToString());
                _isConnected = false;
            }
            finally
            {
                _diag_timer.Enabled = true;
            }
        }

        internal System.Timers.Timer _diag_timer; 
        #endregion

        internal string _hostAddress;
        internal ushort _port { get; set; }

        public void ClearBuffers()
        {
            byte[] info = new byte[2];
            byte[] outval = new byte[2000];
            try
            {
                _socket.IOControl(IOControlCode.DataToRead, info, outval);
                uint bytesAvailable = BitConverter.ToUInt32(outval, 0);
                if (bytesAvailable != 0 && bytesAvailable < 2000)
                {
                    int len = _socket.Receive(outval);
                }
            }
            catch
            {
                //Игнорим ошибки этого уровня
                return;
            }
        }

        public virtual void Connect() { }

        internal void OnReceiveStringInvoke(ChannelDataEventArgs args)
        {
            OnReceiveData?.Invoke(this, args);
            _logger.Debug("TCP  |↓|" + ControlCodesUtility.ReplaceControlCodesToLoggingCodes(args.Data));
        }

        internal void OnTransiveStringInvoke(ChannelDataEventArgs args)
        {
            OnTransiveString?.Invoke(this, args);
        }

        internal void OnConnectingInvoke(string message)
        {
            OnConnecting?.Invoke(this,message);
        }

        internal void OnErrorInvoke(string message)
        {
            OnError?.Invoke(this, message);
        }

        internal void OnDisconnectingInvoke(string message)
        {
            OnDisconnecting?.Invoke(this, message);
        }

        public virtual void DisConnect()
        {
            if (_socket != null)
            {
                if (_socket.Connected)
                {
                    _socket.Close();
                }
                ref Socket socketPointer = ref _socket;
                IDisposable disposable = socketPointer;
                if (disposable == null)
                {
                }
                else
                {
                    disposable.Dispose();
                }
                socketPointer = null;
            }

            OnDisconnecting?.Invoke(this, "Force disconnect");
        }

        public void WriteData(string value)
        {
            byte[] buffer = _encoding.GetBytes(value);
            _socket.Send(buffer);
            _logger.Debug("TCP  |↑|" + ControlCodesUtility.ReplaceControlCodesToLoggingCodes(value));
            OnTransiveStringInvoke(new ChannelDataEventArgs(value));
        }
    }
}
