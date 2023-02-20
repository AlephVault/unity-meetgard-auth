using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Auth.Types;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlephVault.Unity.Support.Utils;
using UnityEngine;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Protocols
    {
        namespace Simple
        {
            /// <summary>
            ///   This is the client-side implementation of a simple
            ///   authentication protocol. The same server doing the
            ///   authentication, is the server mounting the game.
            ///   This client is the counterpart and connects to a
            ///   single server (it may be used in more complex
            ///   login interactions, though).
            /// </summary>
            /// <typeparam name="Definition">A subclass of <see cref="SimpleAuthProtocolDefinition{LoginOK, LoginFailed, Kicked}"/></typeparam>
            /// <typeparam name="LoginOK">The type of the "successful login" message</typeparam>
            /// <typeparam name="LoginFailed">The type of the "failed login" message</typeparam>
            /// <typeparam name="Kicked">The type of the "kicked" message</typeparam>
            [RequireComponent(typeof(MandatoryHandshakeProtocolClientSide))]
            public abstract class SimpleAuthProtocolClientSide<Definition, LoginOK, LoginFailed, Kicked> : ProtocolClientSide<Definition>
                where LoginOK : ISerializable, new()
                where LoginFailed : ISerializable, new()
                where Kicked : IKickMessage<Kicked>, new()
                where Definition : SimpleAuthProtocolDefinition<LoginOK, LoginFailed, Kicked>, new()
            {
                // This is a sender for the Logout message.
                private Func<Task> SendLogout;

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
                ///   Tells whether this client is currently logged
                ///   in, somehow.
                /// </summary>
                public bool LoggedIn { get; private set; }

                /// <summary>
                ///   Defines all the needed messages. One Logout
                ///   message and then invokes MakeLoginRequestSenders.
                ///   Override that method instead of this one.
                /// </summary>
                protected override void Initialize()
                {
                    SendLogout = MakeSender("Logout");
                    MakeLoginRequestSenders();
                }

                /// <summary>
                ///   Implement this method with several calls to
                ///   <see cref="MakeLoginRequestSender{T}(string)"/>,
                ///   each one for each allowed login method.
                /// </summary>
                protected abstract void MakeLoginRequestSenders();

                protected override void SetIncomingMessageHandlers()
                {
                    AddIncomingMessageHandler<LoginOK>("OK", async (proto, message) =>
                    {
                        // The OnLoginOK event is triggered.
                        LoggedIn = true;
                        await (OnLoginOK?.InvokeAsync(message, Tasks.DefaultOnError) ?? Task.CompletedTask);
                    });
                    AddIncomingMessageHandler<LoginFailed>("Failed", async (proto, message) =>
                    {
                        // The OnLoginFailed event is triggered. Expect a disconnection
                        // after this event triggers.
                        client.Close();
                        await (OnLoginFailed?.InvokeAsync(message, Tasks.DefaultOnError) ?? Task.CompletedTask);
                    });
                    AddIncomingMessageHandler<Kicked>("Kicked", async (proto, message) =>
                    {
                        // The OnKicked event is triggered. Expect a disconnection
                        // after this event triggers.
                        LoggedIn = false;
                        client.Close();
                        await (OnKicked?.InvokeAsync(message, Tasks.DefaultOnError) ?? Task.CompletedTask);
                    });
                    AddIncomingMessageHandler("LoggedOut", async (proto) =>
                    {
                        // The OnLoggedOut event is triggered. Expect a disconnection
                        // after this event triggers.
                        LoggedIn = false;
                        client.Close();
                        await (OnLoggedOut?.InvokeAsync(Tasks.DefaultOnError) ?? Task.CompletedTask);
                    });
                    AddIncomingMessageHandler("AccountAlreadyInUse", async (proto) =>
                    {
                        // The AccountAlreadyInUse event is triggered. The implementation
                        // should refresh the UI appropriately. Expect a disconnection
                        // after this event triggers.
                        client.Close();
                        await (OnAccountAlreadyInUse?.InvokeAsync(Tasks.DefaultOnError) ?? Task.CompletedTask);
                    });
                    AddIncomingMessageHandler("AlreadyLoggedIn", async (proto) =>
                    {
                        // The AlreadyLoggedIn event is triggered. The implementation
                        // should refresh the UI appropriately.
                        await (OnAlreadyLoggedIn?.InvokeAsync(Tasks.DefaultOnError) ?? Task.CompletedTask);
                    });
                    AddIncomingMessageHandler("NotLoggedIn", async (proto) =>
                    {
                        // The NotLoggedIn event is triggered. The implementation
                        // should refresh the UI appropriately.
                        await (OnNotLoggedIn?.InvokeAsync(Tasks.DefaultOnError) ?? Task.CompletedTask);
                    });
                    AddIncomingMessageHandler("Forbidden", async (proto) =>
                    {
                        // The Forbidden event is triggered. The implementation
                        // should refresh the UI appropriately.
                        await (OnForbidden?.InvokeAsync(Tasks.DefaultOnError) ?? Task.CompletedTask);
                    });
                }

                /// <summary>
                ///   Makes a sender for the login messages. Each message must
                ///   correspond to a registered message with the method
                ///   <see cref="SimpleAuthProtocolDefinition{LoginOK, LoginFailed, Kicked}.DefineLoginMessage{T}(string)"/>.
                /// </summary>
                /// <typeparam name="T">The type of the login message</typeparam>
                /// <param name="method">The name of the method to use</param>
                /// <returns>The sender for that login message</returns>
                protected Func<T, Task> MakeLoginRequestSender<T>(string method) where T : ISerializable, new()
                {
                    return MakeSender<T>("Login:" + method);
                }
                
                /// <summary>
                ///   Triggered when the client successfully authenticated.
                /// </summary>
                public event Func<LoginOK, Task> OnLoginOK = null;

                /// <summary>
                ///   Triggered when the client failed to authenticate.
                /// </summary>
                public event Func<LoginFailed, Task> OnLoginFailed = null;

                /// <summary>
                ///   Triggered when the session is kicked.
                /// </summary>
                public event Func<Kicked, Task> OnKicked = null;

                /// <summary>
                ///   Triggered when the session was gracefully closed.
                /// </summary>
                public event Func<Task> OnLoggedOut = null;

                /// <summary>
                ///   Triggered when the server tells the client is already logged in.
                /// </summary>
                public event Func<Task> OnAlreadyLoggedIn = null;

                /// <summary>
                ///   Triggered when the server tells the same account is already logged in.
                /// </summary>
                public event Func<Task> OnAccountAlreadyInUse = null;

                /// <summary>
                ///   Triggered when the server tells the client is not logged in.
                /// </summary>
                public event Func<Task> OnNotLoggedIn = null;

                /// <summary>
                ///   Triggered when the server tells the client attempted an action they
                ///   have not permission to perform.
                /// </summary>
                public event Func<Task> OnForbidden = null;

                /// <summary>
                ///   Sends a logout message and immediately closes.
                /// </summary>
                public async Task Logout()
                {
                    // Notes: SendLogout must be awaited for. Otherwise,
                    // the logout message is NOT sent.
                    await SendLogout();
                    LoggedIn = false;
                    client.Close();
                }
            }
        }
    }
}
