using AstmDriver.Abstractions;
using AstmDriver.Commons.ASTM;
using System;
using System.Runtime.CompilerServices;

namespace AstmDriver.Commons
{
    public class ReceiveRecordEventArgs : EventArgs
    {
        private AbstractAstmRecord @_ReceivedRecord;
        private AstmRecordType @_RecordType;

        public AbstractAstmRecord ReceivedRecord
        {
            get
            {
                return @_ReceivedRecord;
            }
            set
            {
                @_ReceivedRecord = value;
            }
        }

        public AstmRecordType RecordType
        {
            get
            {
                return @_RecordType;
            }
            set
            {
                @_RecordType = value;
            }
        }

        public ReceiveRecordEventArgs(AbstractAstmRecord aReceivedRecord, AstmRecordType aLisRecordType)
        {
            @_ReceivedRecord = aReceivedRecord;
            @_RecordType = aLisRecordType;
        }

        public ReceiveRecordEventArgs()
        {
        }
    }
}
