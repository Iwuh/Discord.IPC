using Discord.IPC.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Discord.IPC
{
    /// <summary>
    /// Used to communicate with the Discord client over IPC.
    /// </summary>
    public sealed class IpcClient : IDisposable
    {
        private const int VERSION = 1;
        private readonly ulong _appId;

        private FileStream _ipc;
        private Thread _readThread;
        private CancellationTokenSource _readThreadCancel;
        private State _state;

        public IpcClient(ulong applicationId)
        {
            _appId = applicationId;
        }

        #region Events

        /// <summary>
        /// This event is raised when an <see cref="IpcClient"/> is sent.
        /// </summary>
        public event EventHandler<PacketEventArgs> OnPacketSent;

        /// <summary>
        /// This event is raised when an <see cref="IpcPacket"/> is recieved.
        /// </summary>
        public event EventHandler<PacketEventArgs> OnPacketRecieved;

        /// <summary>
        /// This event is raised when a user joins an activity.
        /// </summary>
        public event EventHandler<ActivityEventArgs> OnActivityJoin;

        /// <summary>
        /// This event is raised when a user spectates an activity.
        /// </summary>
        public event EventHandler<ActivityEventArgs> OnActivitySpectate;

        /// <summary>
        /// This event is raised when a user requests to join an activity.
        /// </summary>
        public event EventHandler<ActivityEventArgs> OnActivityJoinRequest;

        /// <summary>
        /// This event is raised when the IPC bridge encounters an error.
        /// </summary>
        public event EventHandler<IpcErrorEventArgs> OnError;

        #endregion

        private enum State
        {
            New,
            Connecting,
            Connected,
            Closed,
            Errored
        }

        private void StartReadThread()
        {
            _readThread = new Thread((token) =>
            {
                var cancellationToken = (CancellationToken)token;

                bool shouldBreak = false;
                while (!shouldBreak)
                {
                    try
                    {
                        
                    }
                    catch (OperationCanceledException)
                    {
                        shouldBreak = true;
                    }
                }

                Thread.Sleep(50);
            });

            _readThread.Start(_readThreadCancel.Token);
        }        
    }
}
