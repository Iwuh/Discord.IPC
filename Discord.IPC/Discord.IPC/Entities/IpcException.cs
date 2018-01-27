using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.IPC.Entities
{
    public class IpcException : Exception
    {
        public IpcException()
        {
        }

        public IpcException(string message) : base(message)
        {
        }

        public IpcException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
