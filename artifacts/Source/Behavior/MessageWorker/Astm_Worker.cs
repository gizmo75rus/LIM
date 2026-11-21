using AstmDriver.Abstractions;
using AstmDriver.Commons;
using AstmDriver.Commons.ASTM;
using AstmDriver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AstmDriver.Models;

namespace AstmDriver.Behavior.MessageWorker
{
    public class Astm_Worker : AbstractAstmWorker
    {
        public Astm_Worker(IHost host, ILogger logger, Analyzer analyzer) : base(host, logger, analyzer)
        {
        }

        internal override void ProcessQueryMessage(Message message)
        {

        }

        internal override void ProcessOrderMessage(Message message)
        {
            
        }


    }
}
