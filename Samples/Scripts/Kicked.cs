using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Auth.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Samples
    {
        public class Kicked : IKickMessage<Kicked>
        {
            public string Reason;

            public void Serialize(Serializer serializer)
            {
                serializer.Serialize(ref Reason);
            }

            public Kicked WithAccountLoadErrorReason()
            {
                Reason = "An error has occurred while trying to load the account";
                return this;
            }

            public Kicked WithGhostedReason()
            {
                Reason = "The same account logged in from another client connection";
                return this;
            }

            public Kicked WithLoginTimeoutReason()
            {
                Reason = "Login timeout - the client took too much to login";
                return this;
            }

            public Kicked WithNonGracefulDisconnectionErrorReason(Exception reason)
            {
                Reason = $"Exception of type {reason?.GetType()?.FullName ?? "<graceful>"} on disconnection: {reason?.Message ?? "graceful"}";
                return this;
            }

            public Kicked WithSessionInitializationErrorReason()
            {
                Reason = $"An error has occurred while trying to initialize the session";
                return this;
            }
        }
    }
}
