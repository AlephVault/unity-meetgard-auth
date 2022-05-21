using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Auth.Types;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlephVault.Unity.Meetgard.Protocols.Simple;
using UnityEngine;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Protocols
    {
        namespace Simple
        {
            /// <summary>
            ///   This is the server-side implementation of a simple
            ///   authentication protocol. The same server doing the
            ///   authentication, is the server mounting the game.
            ///   This client is the counterpart and connects to a
            ///   single server (it may be used in more complex
            ///   login interactions, though). The server side also
            ///   offers some helpers to wrap handlers to make them
            ///   require login or logout on clients.
            /// </summary>
            /// <typeparam name="Definition">A subclass of <see cref="SimpleAuthProtocolDefinition{LoginOK, LoginFailed, Kicked}"/></typeparam>
            /// <typeparam name="LoginOK">The type of the "successful login" message</typeparam>
            /// <typeparam name="LoginFailed">The type of the "failed login" message</typeparam>
            /// <typeparam name="Kicked">The type of the "kicked" message</typeparam>
            /// <typeparam name="AccountIDType">The type of the account id</typeparam>
            public abstract partial class SimpleAuthProtocolServerSide<
                Definition, LoginOK, LoginFailed, Kicked,
                AccountIDType, AccountPreviewDataType, AccountDataType
            > : MandatoryHandshakeProtocolServerSide<Definition>
                where LoginOK : ISerializable, new()
                where LoginFailed : ISerializable, new()
                where Kicked : IKickMessage<Kicked>, new()
                where AccountPreviewDataType : ISerializable, new()
                where AccountDataType : IRecordWithPreview<AccountIDType, AccountPreviewDataType>
                where Definition : SimpleAuthProtocolDefinition<LoginOK, LoginFailed, Kicked>, new()
            {
                /// <summary>
                ///   Typically, in this Start callback function
                ///   all the Send* shortcuts will be instantiated.
                ///   This time, also the timeout coroutine is
                ///   spawned immediately.
                /// </summary>
                protected override void Initialize()
                {
                    base.Initialize();
                    MakeSenders();
                }
                
                // The only client-side messages that will be set are:
                // 1. Login:* (as much as needed).
                // 2. Logout.
                protected override void SetIncomingMessageHandlers()
                {
                    SetLoginMessageHandlers();
                    AddIncomingMessageHandler("Logout", LoginRequired<Definition>(async (proto, clientId) =>
                    {
                        await Exclusive(async () =>
                        {
                            _ = SendLoggedOut(clientId);
                            await OnLoggedOut(clientId, default(Kicked));
                        });
                    }));
                }

                /// <summary>
                ///   Implement this method with several calls to
                ///   <see cref="AddLoginMessageHandler{T}(string)"/>,
                ///   each one for each allowed login method.
                /// </summary>
                protected abstract void SetLoginMessageHandlers();
                
                /// <summary>
                ///   Removes the connection from pending login and
                ///   also removes the session, if any. Only one of
                ///   them will, in practice, be executed.
                /// </summary>
                /// <param name="clientId">The just-disconnected client id</param>
                /// <param name="reason">The exception which is the disconnection reason, if abrupt</param>
                public override async Task OnDisconnected(ulong clientId, Exception reason)
                {
                    await Exclusive(async () =>
                    {
                        await base.OnDisconnected(clientId, reason);
                        if (SessionExists(clientId))
                        {
                            await OnLoggedOut(clientId, new Kicked().WithNonGracefulDisconnectionErrorReason(reason));
                        }
                    });
                }

                /// <summary>
                ///   Kick an account by its ID.
                /// </summary>
                /// <param name="accountId">The account id to kick</param>
                /// <param name="reason">The reason to kick the account</param>
                public async Task Kick(AccountIDType accountId, Kicked reason)
                {
                    await Exclusive(() => DoKick(accountId, reason));
                }

                // Internal implementation of account kicking.
                private async Task DoKick(AccountIDType accountId, Kicked reason)
                {
                    if (sessionByAccountId.TryGetValue(accountId, out HashSet<Session> sessions))
                    {
                        foreach (Session session in sessions.ToArray())
                        {
                            _ = SendKicked(session.Item1, reason);
                            await OnLoggedOut(session.Item1, reason);
                        }
                    }
                }
            }
        }
    }
}