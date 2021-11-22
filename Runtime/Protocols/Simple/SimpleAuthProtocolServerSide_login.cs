using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Auth.Types;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Server;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Protocols
    {
        namespace Simple
        {
            public abstract partial class SimpleAuthProtocolServerSide<
                Definition, LoginOK, LoginFailed, Kicked,
                AccountIDType, AccountPreviewDataType, AccountDataType
            > : ProtocolServerSide<Definition>
                where LoginOK : ISerializable, new()
                where LoginFailed : ISerializable, new()
                where Kicked : IKickMessage<Kicked>, new()
                where AccountPreviewDataType : ISerializable, new()
                where AccountDataType : IRecordWithPreview<AccountIDType, AccountPreviewDataType>
                where Definition : SimpleAuthProtocolDefinition<LoginOK, LoginFailed, Kicked>, new()
            {
                // This is a dict that will be used to track
                // the timeout of pending login connections.
                private ConcurrentDictionary<ulong, float> pendingLoginConnections = new ConcurrentDictionary<ulong, float>();

                // Adds a connection id to the pending login
                // connections.
                private bool AddPendingLogin(ulong connection)
                {
                    return pendingLoginConnections.TryAdd(connection, 0);
                }

                // Removes a connection id from the pending
                // login connections.
                private bool RemovePendingLogin(ulong connection)
                {
                    return pendingLoginConnections.TryRemove(connection, out _);
                }

                // Updates all of the pending connections.
                private async void UpdatePendingLogin(float delta)
                {
                    await Exclusive(async () =>
                    {
                        foreach (var pair in pendingLoginConnections.ToArray())
                        {
                            pendingLoginConnections.TryUpdate(pair.Key, pair.Value + delta, pair.Value);
                            if (pendingLoginConnections.TryGetValue(pair.Key, out float value) && value >= loginTimeout)
                            {
                                _ = SendTimeout(pair.Key);
                            }
                        }
                    });
                }

                /// <summary>
                ///   <para>
                ///     Adds a login handler for a specific method type.
                ///     The login handler returns a tuple with 3 elements:
                ///     whether the login was successful, the login success
                ///     message (or null if it was successful) and the login
                ///     failure message (or null if it was NOT successful).
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
                /// <typeparam name="T">The type of the login meesage</typeparam>
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
                            if (RemovePendingLogin(clientId))
                            {
                                Tuple<bool, LoginOK, LoginFailed, AccountIDType> result = await doLogin(message);
                                if (result.Item1)
                                {
                                    _ = SendLoginOK(clientId, result.Item2);
                                    await OnLoggedIn(clientId, result.Item4);
                                }
                                else
                                {
                                    _ = SendLoginFailed(clientId, result.Item3);
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