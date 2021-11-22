using AlephVault.Unity.Meetgard.Auth.Protocols.Simple;
using AlephVault.Unity.Meetgard.Samples.Chat;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Server;
using AlephVault.Unity.Meetgard.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Samples
    {
        /// <summary>
        ///   This is the server side of the authenticated
        ///   chat implementation.
        /// </summary>
        [RequireComponent(typeof(SampleSimpleAuthProtocolServerSide))]
        public class SampleAuthChatProtocolServerSide : ProtocolServerSide<SampleAuthChatProtocolDefinition>
        {
            SampleSimpleAuthProtocolServerSide authProtocol;

            private Dictionary<ulong, SampleAccount> users = new Dictionary<ulong, SampleAccount>();

            private Func<IEnumerable<ulong>, SampleAccountPreview, Dictionary<ulong, Task>> BroadcastJoined;
            private Func<IEnumerable<ulong>, SampleAccountPreview, Dictionary<ulong, Task>> BroadcastLeft;
            private Func<IEnumerable<ulong>, Said, Dictionary<ulong, Task>> BroadcastSaid;

            protected new void Awake()
            {
                authProtocol = GetComponent<SampleSimpleAuthProtocolServerSide>();
                base.Awake();
            }

            protected override void Initialize()
            {
                authProtocol.OnSessionStarting += AuthProtocol_OnSessionStarting;
                authProtocol.OnSessionTerminating += AuthProtocol_OnSessionTerminating;
                BroadcastJoined = MakeBroadcaster<SampleAccountPreview>("Joined");
                BroadcastLeft = MakeBroadcaster<SampleAccountPreview>("Left");
                BroadcastSaid = MakeBroadcaster<Said>("Said");
            }

            protected void OnDestroy()
            {
                authProtocol.OnSessionStarting -= AuthProtocol_OnSessionStarting;
                authProtocol.OnSessionTerminating -= AuthProtocol_OnSessionTerminating;
            }

            private async Task AuthProtocol_OnSessionStarting(ulong arg1, SampleAccount arg2)
            {
                users.Add(arg1, arg2);
                Debug.Log($"SACPServer :: Session starting 1 {users} {arg2}");
                _ = BroadcastJoined(users.Keys, arg2.GetProfileDisplayData());
                Debug.Log($"SACPServer :: Session starting 2");
            }

            private async Task AuthProtocol_OnSessionTerminating(ulong arg1, Kicked arg2)
            {
                SampleAccount account = users[arg1];
                users.Remove(arg1);
                _ = BroadcastLeft(users.Keys, account.GetProfileDisplayData());
            }

            protected override void SetIncomingMessageHandlers()
            {
                AddIncomingMessageHandler<Line>("Say", authProtocol.LoginRequired<SampleAuthChatProtocolDefinition, Line>((proto, clientId, message) => {
                    return UntilBroadcastIsDone(BroadcastSaid(users.Keys, new Said() { Nickname = users[clientId].Username, Content = message.Content, When = DateTime.Now.ToString("F") }));
                }));
            }
        }
    }
}
