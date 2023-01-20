using System;
using System.Threading.Tasks;
using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Client;
using AlephVault.Unity.Support.Utils;
using UnityEngine;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Protocols
    {
        namespace Simple
        {
            /// <summary>
            ///   This is the client side implementation of
            ///   a simple register protocol. The same server
            ///   allowing the register, is the one running
            ///   the game. This is optional (many games do
            ///   their register process in an external site).
            /// </summary>
            /// <typeparam name="Definition">A subclass of <see cref="SimpleRegisterProtocolDefinition{RegisterOK,RegisterFailed}"/></typeparam>
            /// <typeparam name="RegisterOK">The type of "successful register" message</typeparam>
            /// <typeparam name="RegisterFailed">The type of "failed register" message</typeparam>
            [RequireComponent(typeof(MandatoryHandshakeProtocolClientSide))]
            public abstract class SimpleRegisterProtocolClientSide<Definition, RegisterOK, RegisterFailed> : ProtocolClientSide<Definition>
                where RegisterOK : ISerializable, new()
                where RegisterFailed : ISerializable, new()
                where Definition : SimpleRegisterProtocolDefinition<RegisterOK, RegisterFailed>, new()
            {
                /// <summary>
                ///   The related handshake handler.
                /// </summary>
                public MandatoryHandshakeProtocolClientSide Handshake { get; private set; }
                
                protected override void Setup()
                {
                    base.Setup();
                    Handshake = GetComponent<MandatoryHandshakeProtocolClientSide>();
                }

                /// <summary>
                ///   Defines all the needed messages. One Logout
                ///   message and then invokes MakeLoginRequestSenders.
                ///   Override that method instead of this one.
                /// </summary>
                protected override void Initialize()
                {
                    MakeRegisterRequestSenders();
                }

                protected override void SetIncomingMessageHandlers()
                {
                    AddIncomingMessageHandler<RegisterOK>("OK", async (proto, message) =>
                    {
                        // The OnLoginOK event is triggered. Expect a disconnection
                        // after this event triggers.
                        await (OnRegisterOK?.InvokeAsync(message) ?? Task.CompletedTask);
                    });
                    AddIncomingMessageHandler<RegisterFailed>("Failed", async (proto, message) =>
                    {
                        // The OnRegisterFailed event is triggered. Expect a disconnection
                        // after this event triggers.
                        await (OnRegisterFailed?.InvokeAsync(message) ?? Task.CompletedTask);
                    });
                }

                /// <summary>
                ///   Implement this method with several calls to
                ///   <see cref="MakeRegisterRequestSender{T}(string)"/>,
                ///   each one for each allowed register method.
                ///   Typically, only one will be used.
                /// </summary>
                protected abstract void MakeRegisterRequestSenders();
                
                /// <summary>
                ///   Makes a sender for the register messages. Each message
                ///   must correspond to a registered message with the method
                ///   <see cref="SimpleRegisterProtocolDefinition{RegisterOK, RegisterFailed}.DefineRegisterMessage{T}(string)"/>.
                /// </summary>
                /// <typeparam name="T">The type of the register message</typeparam>
                /// <param name="method">The name of the method to use</param>
                /// <returns>The sender for that register message</returns>
                protected Func<T, Task> MakeRegisterRequestSender<T>(string method) where T : ISerializable, new()
                {
                    return MakeSender<T>("Register:" + method);
                }
                
                /// <summary>
                ///   Triggered when the client successfully registered.
                /// </summary>
                public event Func<RegisterOK, Task> OnRegisterOK = null;

                /// <summary>
                ///   Triggered when the client failed to register.
                /// </summary>
                public event Func<RegisterFailed, Task> OnRegisterFailed = null;
            }
        }
    }
}