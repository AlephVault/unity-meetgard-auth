using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Types
    {
        /// <summary>
        ///   This enumeration tells what would happen when
        ///   two connections authenticate with the same
        ///   account.
        /// </summary>
        public enum AccountAlreadyLoggedManagementMode
        {
            /// <summary>
            /// Kill the currently existing session(s).
            /// </summary>
            Ghost,

            /// <summary>
            ///   Reject the new session.
            /// </summary>
            Reject,

            /// <summary>
            ///   Allow all the sessions
            /// </summary>
            AllowAll
        }
    }
}
