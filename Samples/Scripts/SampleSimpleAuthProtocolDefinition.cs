using AlephVault.Unity.Meetgard.Auth.Protocols.Simple;
using AlephVault.Unity.Meetgard.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Samples
    {
        /// <summary>
        ///   This sample login protocol has only one mean
        ///   of login: "Login:Sample" (Username, Password).
        /// </summary>
        public class SampleSimpleAuthProtocolDefinition : SimpleAuthProtocolDefinition<Nothing, LoginFailed, Kicked>
        {
            protected override void DefineLoginMessages()
            {
                DefineLoginMessage<UserPass>("Sample");
            }
        }
    }
}
