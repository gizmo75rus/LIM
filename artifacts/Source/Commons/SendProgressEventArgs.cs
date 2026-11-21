using System;

namespace AstmDriver.Commons
{
    public class SendProgressEventArgs : EventArgs
    {
        private double @_Progress;


        private string _Message;

        public double Progress
        {
            get
            {
                return @_Progress;
            }
            set
            {
                @_Progress = value;
            }
        }

        public string Message
        {
            get
            {
                return _Message;
            }
            set
            {
                _Message = value;
            }
        }


        public SendProgressEventArgs(double aProgress, string message)
        {
            @_Progress = aProgress;
            _Message = message;
        }
    }
}
