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
        private const int CORRUPT_READ = 2;  // 2 is the constant used in the official version so that's what we'll use for consistency.
        private readonly ulong _appId;

        private IpcSocket _ipc;
        private Thread _readThread;
        private CancellationTokenSource _readThreadCancel;

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
        /// This event is raised when the pipe responds with an error message.
        /// </summary>
        public event EventHandler<IpcErrorEventArgs> OnError;

        /// <summary>
        /// This event is raised when the pipe disconnects due to an error.
        /// </summary>
        public event EventHandler<IpcErrorEventArgs> OnDisconnect;

        #endregion

        public enum State
        {
            /// <summary>
            /// The client has been created and has yet to connect.
            /// </summary>
            New,

            /// <summary>
            /// The client is in the process of connecting.
            /// </summary>
            Connecting,

            /// <summary>
            /// The client is connected.
            /// </summary>
            Connected,

            /// <summary>
            /// The client has been disconnected due to an error.
            /// </summary>
            Disconnected,

            /// <summary>
            /// The client has safely and normally shut down.
            /// </summary>
            Closed
        }

        /// <summary>
        /// The client's current state.
        /// </summary>
        public State ClientState { get; private set; }

        public void Close()
        {
            if (ClientState != State.Connected)
            {
                throw new InvalidOperationException("Can only close while connected.");
            }

            // Notify the read thread to shut down.
            _readThreadCancel.Cancel();
            _readThreadCancel = null;

            // We can remove the only reference to the read thread as it won't be garbage collected until it's done running.
            _readThread = null;

            // Dispose the object used to communicate with the client.
            _ipc.Dispose();
            _ipc = null;

            ClientState = State.Disconnected;
        }

        private void StartReadThread()
        {
            _readThread = new Thread(token =>
            {
                // Unbox the cancellation token as ParameterizedThreadStart only takes an object.
                var cancellationToken = (CancellationToken)token;

                bool shouldBreak = false;
                while (!shouldBreak)
                {
                    try
                    {
                        // Throw if the read thread should shut down so that it can clean up.
                        cancellationToken.ThrowIfCancellationRequested();

                        // Only read if data is available because otherwise Read will block.
                        if (_ipc.IsDataAvailable)
                        {
                            // Read data and raise the packet recieved event.
                            IpcPacket packet = _ipc.Read();
                            OnPacketRecieved(this, new PacketEventArgs(packet));

                            switch (packet.OpCode)
                            {
                                case OpCode.Frame:
                                    // Frame is the opcode for normal data transfer.
                                    var evt = (string)packet.Payload["evt"];
                                    if (evt == null) break;

                                    HandleEvent(evt, (JObject)packet.Payload["data"]);
                                        
                                    break;

                                case OpCode.Close:
                                    // Close means something has gone wrong and the discord client has severed the connection.
                                    Close();
                                    var closeData = packet.Payload["data"];
                                    OnDisconnect(this, new IpcErrorEventArgs((int)closeData["code"], (string)closeData["message"]));
                                    break;

                                case OpCode.Ping:
                                    // When we're pinged, pong with the same data.
                                    _ipc.Write(OpCode.Pong, packet.Payload, addNonce: false);
                                    break;

                                case OpCode.Pong:
                                    // The official version has a case for Pong but doesn't actually do anything so that's what we'll do.
                                    break;

                                default:
                                    // Bad data was recieved, so we safely close the connection and notify the user.
                                    Close();
                                    OnDisconnect(this, new IpcErrorEventArgs(CORRUPT_READ, "The IPC frame data is corrupt."));
                                    break;
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        shouldBreak = true;
                    }

                    // Wait 50ms before the next iteration.
                    Thread.Sleep(50);
                }
            });

            _readThread.Start(_readThreadCancel.Token);
        }

        private void HandleEvent(string evt, JObject data)
        {
            switch (evt)
            {
                case "ERROR":
                    OnError(this, new IpcErrorEventArgs((int)data["code"], (string)data["message"]));
                    break;

                case "ACTIVITY_JOIN":
                    OnActivityJoin(this, new ActivityEventArgs((string)data["secret"]));
                    break;

                case "ACTIVITY_SPECTATE":
                    OnActivitySpectate(this, new ActivityEventArgs((string)data["secret"]));
                    break;

                case "ACTIVITY_JOIN_REQUEST":
                    var user = data["user"];
                    OnActivityJoinRequest(this, new ActivityEventArgs((string)data["secret"], new User(
                        (string)user["username"],
                        (string)user["discriminator"],
                        ulong.Parse((string)user["id"]),
                        (string)user["avatar"])));
                    break;

                default:
                    break;
            }
        }
    }
}
