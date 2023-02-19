using AlephVault.Unity.Binary;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Protocols
    {
        namespace Simple
        {
            public abstract partial class SimpleAuthProtocolServerSide<
                Definition, LoginOK, LoginFailed, Kicked,
                AccountIDType, AccountPreviewDataType, AccountDataType
            > {
                /// <summary>
                ///   A shortcut function to reject a login.
                /// </summary>
                /// <param name="failure">The failure to use</param>
                /// <returns>An appropriate result tuple</returns>
                protected Tuple<bool, LoginOK, LoginFailed, AccountIDType> RejectLogin(LoginFailed failure)
                {
                    return new Tuple<bool, LoginOK, LoginFailed, AccountIDType>(false, default, failure, default);
                }

                /// <summary>
                ///   A shortcut function to accept a login.
                /// </summary>
                /// <param name="ok">The success to use (usually trivial)</param>
                /// <param name="id">The account id</param>
                /// <returns>An appropriate result tuple</returns>
                protected Tuple<bool, LoginOK, LoginFailed, AccountIDType> AcceptLogin(LoginOK ok, AccountIDType id)
                {
                    return new Tuple<bool, LoginOK, LoginFailed, AccountIDType>(true, ok, default, id);
                }
                
                /// <summary>
                ///   <para>
                ///     Adds a login handler for a specific method type.
                ///     The login handler returns a tuple with 3 elements:
                ///     whether the login was successful, the login success
                ///     message (or null if it was unsuccessful) and the
                ///     login failure message (or null if it was successful).
                ///     As a fourth parameter, the account id will be given
                ///     (or its default value) when the login is successful.
                ///   </para>
                ///   <para>
                ///     The login handler is responsible of logging. In
                ///     case of success, the session will start for that
                ///     account (each session type is differently handled),
                ///     which will be implemented in a different component.
                ///   </para>
                /// </summary>
                /// <typeparam name="T">The type of the login message</typeparam>
                /// <param name="method">The name of the method to use</param>
                /// <param name="doLogin">The handler to use to perform the login</param>
                protected void AddLoginMessageHandler<T>(string method, Func<T, Task<Tuple<bool, LoginOK, LoginFailed, AccountIDType>>> doLogin) where T : ISerializable, new()
                {
                    AddIncomingMessageHandler<T>("Login:" + method, LogoutRequired<Definition, T>(async (proto, clientId, message) => {
                        // 1. Receive the message.
                        // 2. Process the message.
                        // 3. On success: trigger the success.
                        // 4. On failure: trigger the failure.
                        await Exclusive(async () => {
                            if (Handshake.RemoveHandshakePending(clientId))
                            {
                                Tuple<bool, LoginOK, LoginFailed, AccountIDType> result = await doLogin(message);
                                if (result.Item1)
                                {
                                    if (!EqualityComparer<LoginFailed>.Default.Equals(result.Item3, default))
                                    {
                                        Debug.LogWarning($"Login was successful but a {typeof(LoginFailed).FullName} argument " +
                                                         $"is specified: {result.Item3}");
                                    }
                                    if (EqualityComparer<LoginOK>.Default.Equals(result.Item2, default))
                                    {
                                        Debug.LogWarning($"Login was successful but a {typeof(LoginOK).FullName} argument " +
                                                         "is not specified");
                                    }
                                    if (EqualityComparer<AccountIDType>.Default.Equals(result.Item4, default))
                                    {
                                        Debug.LogWarning($"Login was successful but a {typeof(AccountIDType).FullName} argument " +
                                                         "is not specified");
                                    }
                                    await SendLoginOK(clientId, result.Item2);
                                    await OnLoggedIn(clientId, result.Item4);
                                }
                                else
                                {
                                    if (EqualityComparer<LoginFailed>.Default.Equals(result.Item3, default))
                                    {
                                        Debug.LogWarning($"Login was unsuccessful but a {typeof(LoginFailed).FullName} argument " +
                                                         "is not specified");
                                    }
                                    if (!EqualityComparer<LoginOK>.Default.Equals(result.Item2, default))
                                    {
                                        Debug.LogWarning($"Login was unsuccessful but a {typeof(LoginOK).FullName} argument " +
                                                         $"is specified: {result.Item2}");
                                    }
                                    if (!EqualityComparer<AccountIDType>.Default.Equals(result.Item4, default))
                                    {
                                        Debug.LogWarning($"Login was unsuccessful but a {typeof(AccountIDType).FullName} argument " +
                                                         $"is specified: {result.Item3}");
                                    }
                                    await SendLoginFailed(clientId, result.Item3);
                                    server.Close(clientId);
                                }
                            }
                        });
                    }));
                }
            }
        }
    }
}