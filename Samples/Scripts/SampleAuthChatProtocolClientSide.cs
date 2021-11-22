using AlephVault.Unity.Meetgard.Auth.Protocols.Simple;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Client;
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
        ///   This is the client side of the authenticated
        ///   chat implementation.
        /// </summary>
        [RequireComponent(typeof(SampleSimpleAuthProtocolClientSide))]
        public class SampleAuthChatProtocolClientSide : ProtocolClientSide<SampleAuthChatProtocolDefinition>
        {
            private Func<Line, Task> SendSay;

            protected override void Initialize()
            {                
                SendSay = MakeSender<Line>("Say");
            }

            protected override void SetIncomingMessageHandlers()
            {
                AddIncomingMessageHandler<Said>("Said", async (proto, message) =>
                {
                    Debug.Log($"{message.When} {message.Nickname}: {message.Content}");
                });
                AddIncomingMessageHandler<SampleAccountPreview>("Left", async (proto, message) =>
                {
                    Debug.Log($"{message.Username} left");
                });
                AddIncomingMessageHandler<SampleAccountPreview>("Joined", async (proto, message) =>
                {
                    Debug.Log($"{message.Username} joined");
                });
            }

            /// <summary>
            ///   Says something to the server
            /// </summary>
            /// <param name="message">The text to say</param>
            public Task Say(string message)
            {
                return SendSay(new Line() { Content = message });
            }
        }
    }
}
