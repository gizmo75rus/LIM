using AstmDriver.Abstractions;
using AstmDriver.Commons;
using AstmDriver.Commons.ASTM.Models;
using AstmDriver.Core;
using AstmDriver.Exceptions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BaseLibraries.Ado.Helpers;
using System.Runtime.InteropServices;

namespace AstmDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            DisableConsoleQuickEdit.Go();

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(handler);

            Console.WriteLine("Подготовка к запуску....");
            Thread.Sleep(2000);
            Console.Clear();

            Dal.SetConnectionString(ConfigurationManager.ConnectionStrings["LIS"].ConnectionString);
            var configRepository = new ConfigRepository();
            var config = configRepository.Config;
            LoggerFactory.CreateLogger(config.LogType, config.LogLevel);

            SetTitle(config.Port, config.IsServerMode,"");

            Console.WriteLine("\r\n");
            Console.WriteLine("=========================================================================================");
            Console.WriteLine($"Driver version:    {Globals.Version}");
            Console.WriteLine($"Remote host:  {config.RemoteHost}");
            Console.WriteLine($"Port:         {config.Port}");
            Console.WriteLine($"Mode:              {config.GetOperationMode()}");
            Console.WriteLine($"HostType           {config.HostType}");
            Console.WriteLine($"MW:                {config.MessageWorker}");
            Console.WriteLine($"Log level:         {config.LogLevel}");
            Console.WriteLine("=========================================================================================");

            var logger = LoggerFactory.GetLogger(typeof(Program));

            DataHelper.OnError += (message) =>
            {
                logger.Error("DataHelper, ошибка:  " + message);
                DisplayError(message);
            };

            ITransportChannel channel = ChannelFactory.CreateChannel(config);
            
            IHost host = HostFactory.Create(config, channel);           
            IMessageWorker worker = MessageWorkerFactory.Create(config, host); 

            channel.OnConnecting += (s, ip) =>
            {
                Console.WriteLine("-----------------------------------------------------------------------------------------");
                DisplayInfo($"   Client connected: [{ip}]                                                     ");
                Console.WriteLine("-----------------------------------------------------------------------------------------");
                Console.WriteLine("     Дата время      | НПР |     Данные");
                Console.WriteLine("-----------------------------------------------------------------------------------------");
            };
            channel.OnDisconnecting += (s, message) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{message}");
                Console.ForegroundColor = ConsoleColor.Gray;
            };

            host.OnError += (s, message) => 
            {
                DisplayError($"{DateTime.Now}  | !!! |  {ControlCodesUtility.ClearControlCodes(message)}");
            };

            host.OnReconnect += (s, message) =>
            {
                DisplayInfo($"{DateTime.Now}  | VVV |  {message}");
            };

            worker.OnDataReciveid += (s, data) =>
            {
                Console.WriteLine($"{DateTime.Now}  | <<< |  {ControlCodesUtility.ReplaceControlCodesToLoggingCodes(data)}");
            };
            worker.OnDataTransivied += (s, data) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{DateTime.Now}  | >>> |  {ControlCodesUtility.ReplaceControlCodesToLoggingCodes(data)}");
                Console.ForegroundColor = ConsoleColor.Gray;
            };
            worker.OnLastEOT += (s, date) =>
            {
                SetTitle(config.Port, config.IsServerMode,date.ToString());
            };

            worker.OnError += (s, e) =>
            {
                DisplayError(e);
            };

            worker.OnInfo += (s, message) =>
            {
                DisplayInfo($"{DateTime.Now}  | VVV |  {message}");
            };

            host.Connect();

            while (true)
            {
                Thread.Sleep(500);
            };

        }

        private static void SetTitle(int port,bool is_server, string last_date_communicate) 
        {
            string type = is_server ? "Сервер" : "Клиент";

            Console.Title = $"[ {port} ] {type} v.{Globals.Version} | {last_date_communicate} ";
        }

        private static void DisplayError(string error) 
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private static void DisplayInfo(string message) 
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private static void handler(object sender, UnhandledExceptionEventArgs e)
        {
            var type = e.ExceptionObject.GetType().Name;
            bool autoRestart = false;
            string message = "";

            switch (type)
            {
                case nameof(SqlException):
                    message = ((Exception)e.ExceptionObject).Message;
                    break;
                case nameof(ArgumentConfigException):
                    message = ((Exception)e.ExceptionObject).Message;
                    break;
                case nameof(NotImplementedException):
                    message = ((Exception)e.ExceptionObject).Message + ((Exception)e.ExceptionObject).StackTrace;
                    break;
                default:
                    autoRestart = true;
                    message = "Перехвачено не обработанное исключение: " + e.ExceptionObject.ToString();
                    break;
            }

            var logger = LoggerFactory.GetLogger(typeof(Program));

            logger.WriteCrashDump(message);
            DisplayError(message);

            if (autoRestart)
            {
                Reboot();
            }
            else
            {
                UserAction();
            }
        }


        private static void UserAction()
        {
            DisplayInfo("Для закрытия программы введите Q илил ESC, для перезапуска R или ENTER");
            var key = Console.ReadKey();
            switch (key.Key)
            {
                case ConsoleKey.Q:
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;
                case ConsoleKey.R:
                case ConsoleKey.Enter:
                    Reboot();
                    break;
                default:
                    UserAction();
                    break;
            }
        }

        private static void Reboot()
        {
            DisplayInfo("Выполняется перезапуск драйвера");


            Thread.Sleep(5000);

            Process.Start(Assembly.GetExecutingAssembly().Location);
            Environment.Exit(0);
        }


    }

    static class DisableConsoleQuickEdit
    {

        const uint ENABLE_QUICK_EDIT = 0x0040;

        // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
        const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        internal static bool Go()
        {

            IntPtr consoleHandle = GetStdHandle(STD_INPUT_HANDLE);

            // get current console mode
            uint consoleMode;
            if (!GetConsoleMode(consoleHandle, out consoleMode))
            {
                // ERROR: Unable to get console mode.
                return false;
            }

            // Clear the quick edit bit in the mode flags
            consoleMode &= ~ENABLE_QUICK_EDIT;

            // set the new mode
            if (!SetConsoleMode(consoleHandle, consoleMode))
            {
                // ERROR: Unable to set console mode
                return false;
            }

            return true;
        }
    }
}
