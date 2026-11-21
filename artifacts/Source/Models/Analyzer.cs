using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstmDriver.Models
{
    public class Analyzer
    {
        public int id;
        public string ip;
        public string text;
        public string port;
        public string Header;
        public string doSend;
        public string astm_system_name;
        public string astm_name;
        public string astm_pass;
        public string server_type;
        public int Port => Int32.Parse(this.port);

        public Analyzer()
        {

        }
        public Analyzer(

            int id,
            string ip,
            string text,
            string port,
            string Header,
            string doSend,
            string astm_system_name,
            string astm_name,
            string astm_pass,
            string server_type

        )
        {
            this.id = id;
            this.ip = ip;
            this.text = text;
            this.port = port;
            this.Header = Header;
            this.doSend = doSend;
            this.astm_system_name = astm_system_name;
            this.astm_name = astm_name;
            this.astm_pass = astm_pass;
            this.server_type = server_type;
        }
    }
}
