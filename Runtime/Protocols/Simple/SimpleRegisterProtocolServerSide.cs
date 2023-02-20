using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Server;
using AlephVault.Unity.Support.Utils;
using UnityEngine;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Protocols
    {
        namespace Simple
        {
            /// <summary>
            ///   This is the server-side implementation of a simple
            ///   registration protocol. The same server doing the
            ///   authentication, is the server mounting the game.
            ///   This is optional (many games do their register
            ///   process in an external site).
            /// </summary>
            /// <typeparam name="Definition">A subclass of <see cref="SimpleRegisterProtocolDefinition{RegisterOK, RegisterFailed}"/></typeparam>
            /// <typeparam name="RegisterOK"></typeparam>
            /// <typeparam name="RegisterFailed"></typeparam>
            [RequireComponent(typeof(MandatoryHandshakeProtocolServerSide))]
            public abstract class SimpleRegisterProtocolServerSide<
                Definition, RegisterOK, RegisterFailed
            > : ProtocolServerSide<Definition>
                where RegisterOK : ISerializable, new()
                where RegisterFailed : ISerializable, new()
                where Definition : SimpleRegisterProtocolDefinition<RegisterOK, RegisterFailed>, new()
            {
                private Func<ulong, RegisterOK, Task> SendRegisterOK;
                private Func<ulong, RegisterFailed, Task> SendRegisterFailed;
                
                /// <summary>
                ///   The related handshake handler.
                /// </summary>
                public MandatoryHandshakeProtocolServerSide Handshake { get; private set; }
                
                protected override void Setup()
                {
                    base.Setup();
                    Handshake = GetComponent<MandatoryHandshakeProtocolServerSide>();
                }

                /// <summary>
                ///   Typically, in this Start callback function
                ///   all the Send* shortcuts will be instantiated.
                ///   This time, also the timeout coroutine is
                ///   spawned immediately.
                /// </summary>
                protected override void Initialize()
                {
                    base.Initialize();
                    SendRegisterOK = MakeSender<RegisterOK>("OK");
                    SendRegisterFailed = MakeSender<RegisterFailed>("Failed");
                }
                
                // The only client-side messages that will be set are:
                // 1. Register:* (as much as needed).
                protected override void SetIncomingMessageHandlers()
                {
                    SetRegisterMessageHandlers();
                }

                /// <summary>
                ///   Implement this method with several calls to
                ///   <see cref="AddRegisterMessageHandler{T}"/>,
                ///   each one for each allowed register method.
                /// </summary>
                protected abstract void SetRegisterMessageHandlers();
                
                /// <summary>
                ///   A shortcut function to reject a register.
                /// </summary>
                /// <param name="failure">The failure to use</param>
                /// <returns>An appropriate result tuple</returns>
                protected Tuple<bool, RegisterOK, RegisterFailed> RejectRegister(RegisterFailed failure)
                {
                    return new Tuple<bool, RegisterOK, RegisterFailed>(false, default, failure);
                }

                /// <summary>
                ///   A shortcut function to accept a register.
                /// </summary>
                /// <param name="ok">The success to use (usually trivial)</param>
                /// <returns>An appropriate result tuple</returns>
                protected Tuple<bool, RegisterOK, RegisterFailed> AcceptRegister(RegisterOK ok)
                {
                    return new Tuple<bool, RegisterOK, RegisterFailed>(true, ok, default);
                }
                
                /// <summary>
                ///   <para>
                ///     Adds a register handler for a specific method type.
                ///     The register handler returns a tuple with 3 elements:
                ///     whether the register was successful, the said success
                ///     message (or null if it was not successful) and the
                ///     register failure message (or null if it was successful).
                ///   </para>
                ///   <para>
                ///     The register handler is responsible of registering.
                ///   </para>
                ///   <para>
                ///     The <see cref="doRegister"/> argument should be wrapped
                ///     with some sort of a LogoutRequired callback, stemming
                ///     from a SimpleAuthProtocolServerSide component.
                ///   </para>
                /// </summary>
                /// <typeparam name="T">The type of the login message</typeparam>
                /// <param name="method">The name of the method to use</param>
                /// <param name="doRegister">The handler to use to perform the register</param>
                protected void AddRegisterMessageHandler<T>(string method, Func<T, Task<Tuple<bool, RegisterOK, RegisterFailed>>> doRegister) where T : ISerializable, new()
                {
                    AddIncomingMessageHandler<T>("Register:" + method, async (proto, clientId, message) => {
                        // 1. Receive the message.
                        // 2. Process the message.
                        // 3. On success: trigger the success.
                        // 4. On failure: trigger the failure.
                        await Exclusive(async () => {
                            try
                            {
                                if (Handshake.RemoveHandshakePending(clientId))
                                {
                                    Tuple<bool, RegisterOK, RegisterFailed> result = await doRegister(message);
                                    if (result.Item1)
                                    {
                                        if (!EqualityComparer<RegisterFailed>.Default.Equals(result.Item3, default))
                                        {
                                            Debug.LogWarning($"Register was successful but a {typeof(RegisterFailed).FullName} argument " +
                                                             $"is specified: {result.Item3}");
                                        }
                                        if (EqualityComparer<RegisterOK>.Default.Equals(result.Item2, default))
                                        {
                                            Debug.LogWarning($"Register was successful but a {typeof(RegisterOK).FullName} argument " +
                                                             "is not specified");
                                        }
                                        await SendRegisterOK(clientId, result.Item2);
                                    }
                                    else
                                    {
                                        if (EqualityComparer<RegisterFailed>.Default.Equals(result.Item3, default))
                                        {
                                            Debug.LogWarning($"Register was unsuccessful but a {typeof(RegisterFailed).FullName} argument " +
                                                             "is not specified");
                                        }
                                        if (!EqualityComparer<RegisterOK>.Default.Equals(result.Item2, default))
                                        {
                                            Debug.LogWarning($"Register was unsuccessful but a {typeof(RegisterOK).FullName} argument " +
                                                             $"is specified: {result.Item2}");
                                        }
                                        await SendRegisterFailed(clientId, result.Item3);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                await Tasks.DefaultOnError(e);
                                Debug.LogError("The connection will abruptly terminate now, since it cannot " +
                                               "determine a proper register failure to send. This is wrong. Fix your " +
                                               "error as soon as possible.");
                                server.Close(clientId);
                            }
                        });
                    });
                }
            }
        }
    }
}