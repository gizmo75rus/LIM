using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstmDriver.Abstractions
{
    public interface IMessageWorker
    {
        event EventHandler<string> OnError;
        event EventHandler<string> OnDataReciveid;
        event EventHandler<string> OnDataTransivied;
        event EventHandler<DateTime> OnLastEOT;
        event EventHandler<string> OnInfo;
    }
}
