using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.IPC.Entities
{
    /// <summary>
    /// Event data for <see cref="IpcPacket"/> related events.
    /// </summary>
    public sealed class PacketEventArgs : EventArgs
    {
        /// <summary>
        /// The packet that was sent or recieved.
        /// </summary>
        public IpcPacket Packet { get; private set; }

        public PacketEventArgs(IpcPacket packet)
        {
            Packet = packet;
        }
    }
}
