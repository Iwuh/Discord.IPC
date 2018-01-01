using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.IPC.Entities
{
    /// <summary>
    /// Event data for errors.
    /// </summary>
    public sealed class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The error's numerical identifier.
        /// </summary>
        public int ErrorCode { get; private set; }

        /// <summary>
        /// The error message.
        /// </summary>
        public string Message { get; private set; }

        public ErrorEventArgs(int code, string message)
        {
            ErrorCode = code;
            Message = message;
        }
    }
}
