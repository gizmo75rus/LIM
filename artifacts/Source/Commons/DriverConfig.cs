using System;

namespace AstmDriver.Commons
{
    public class DriverConfig
    {
        public string Version { get; set; }
        public string RemoteHost { get; set; } = "127.0.0.1"; //"srv-dat-ap68";//"10.68.101.204"; //"127.0.0.1";
        public ushort Port { get; set; } = 9125;//10204; //8255;

        public string Encoding { get; set; } = "Default";

        [Obsolete("Use new parameter HostType")]
        public string LowLevel { get; set; } = "ASTM"; // "ASTM";
        
        /// <summary>
        /// Тип хоста допустимы: AsmHost, BeckmanCoulterHost, MetascanHost, MiuraHost, MllpHost
        /// </summary>
        public string HostType { get; set; } = "AsmHost";

        //Тип MessageLevel допустимы Saphir400, AU480, Metascan, Miura, Aspect
        public string MessageWorker { get; set; } = "Default"; //"Metascan"; //"Saphir400";
        public bool IsServerMode { get; set; } = true;
        public int ServerVersion { get; set; } = 3;
        
        public string ApiURL { get; set; } = "http://localhost/lis/";
        
        public double ApiRequestInterval { get; set; } = 1;
        public string OrganizationGuid { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Тип логгера File - файловый, DB - База данных
        /// </summary>
        public string LogType { get; set; } = "File";

        /// <summary>
        /// Интервал таймера для отправки результатов (Metascan)
        /// </summary>
        public int BackTimerInterval { get; set; } = 5;

        public int BackAnalyzerID { get; set; } = 28;

        internal object GetOperationMode()
        {
            return IsServerMode ? "Server" : "Client";
        }

        /// <summary>
        /// Уровень логгирования - 0- только ошибки, 1 -Trace, 2 -Info, 3-Debug
        /// </summary>
        public int LogLevel { get; set; } = 0;

    }
}
