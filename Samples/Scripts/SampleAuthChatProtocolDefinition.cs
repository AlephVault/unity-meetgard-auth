using AlephVault.Unity.Meetgard.Auth.Protocols.Simple;
using AlephVault.Unity.Meetgard.Protocols;
using AlephVault.Unity.Meetgard.Samples.Chat;
using AlephVault.Unity.Meetgard.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Samples
    {
        /// <summary>
        ///   This is a small chat with authentication.
        /// </summary>
        public class SampleAuthChatProtocolDefinition : ProtocolDefinition
        {
            protected override void DefineMessages()
            {
                DefineClientMessage<Line>("Say");
                DefineServerMessage<Said>("Said");
                DefineServerMessage<SampleAccountPreview>("Joined");
                DefineServerMessage<SampleAccountPreview>("Left");
            }
        }
    }
}
