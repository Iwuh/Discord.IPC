using System;
using System.Collections.Generic;
using System.Text;

namespace Discord.IPC.Entities
{
    /// <summary>
    /// Event data for activity related events.
    /// </summary>
    public sealed class ActivityEventArgs : EventArgs
    {
        /// <summary>
        /// The secret that was provided when setting the rich presence.
        /// </summary>
        public string Secret { get; private set; }

        /// <summary>
        /// In a join request, the user who requested to join. Otherwise null.
        /// </summary>
        public User User { get; private set; }

        public ActivityEventArgs(string secret, User user)
        {
            Secret = secret; // I've got a secret
            User = user;
        }
    }
}
