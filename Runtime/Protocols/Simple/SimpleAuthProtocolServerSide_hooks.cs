using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Auth.Types;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlephVault.Unity.Support.Utils;

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
                ///   Tells when an account was not found by its ID.
                /// </summary>
                public class AccountNotFound : Exception {
                    public readonly AccountIDType ID;

                    public AccountNotFound(AccountIDType id) : base() { ID = id; }
                    public AccountNotFound(AccountIDType id, string message) : base(message) { ID = id; }
                    public AccountNotFound(AccountIDType id, string message, Exception cause) : base(message, cause) { ID = id; }
                }

                /// <summary>
                ///   The current stage of the session. This stage
                ///   involves load / unload session operations,
                ///   and not the gameplay itself.
                /// </summary>
                protected enum SessionStage
                {
                    AccountLoad,
                    Initialization,
                    PermissionCheck,
                    Termination
                }

                //
                //
                //
                // Session hooks start here. They are related to loading
                // account data and preparing the session appropriately,
                // and also what to do when the session was told to
                // terminate (logout of kick).
                //
                //
                //

                // This function is invoked when a client was successfully
                // logged in. The full account data will be prepared and
                // the session will start. An unhandled error will become
                // a deal breaker and the client will be kicked and told
                // it was due to an unexpected error.
                private async Task OnLoggedIn(ulong clientId, AccountIDType accountId)
                {
                    AccountDataType accountData = default;
                    // 1. Get the account data.
                    // 2. On error:
                    //   2.1. Handle the error appropriately.
                    //   2.2. Send a kick message with "unexpected error on account load".
                    //   2.3. Close the connection.
                    try
                    {
                        switch(IfAccountAlreadyLoggedIn())
                        {
                            case AccountAlreadyLoggedManagementMode.Reject:
                                // Reject the new, if already logged in.
                                if (sessionByAccountId.ContainsKey(accountId))
                                {
                                    _ = SendAccountAlreadyInUse(clientId);
                                    server.Close(clientId);
                                    return;
                                }
                                break;
                            case AccountAlreadyLoggedManagementMode.Ghost:
                                // Kick any previous connections with the same account id.
                                await DoKick(accountId, new Kicked().WithGhostedReason());
                                break;
                            default:
                                // Do nothing.
                                break;
                        }
                        accountData = await FindAccount(accountId);
                        if (EqualityComparer<AccountDataType>.Default.Equals(accountData, default(AccountDataType)))
                        {
                            throw new AccountNotFound(accountId);
                        }
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            await OnSessionError(clientId, SessionStage.AccountLoad, e);
                        }
                        catch (Exception e2)
                        {
                            await Tasks.DefaultOnError(e2);
                        }
                        _ = SendKicked(clientId, new Kicked().WithAccountLoadErrorReason());
                        server.Close(clientId);
                        return;
                    }
                    // 3. Add the session.
                    // 4. Invoke the "session initializing" hook, considering account data.
                    // 5. On error:
                    //   5.1. Handle the error appropriately.
                    //   5.2. Send a kick message with "unexpected error on session start".
                    //   5.3. Close the connection.
                    AddSession(clientId, accountId);
                    await (OnSessionStarting?.InvokeAsync(clientId, accountData, async (e) =>
                    {
                        try
                        {
                            await OnSessionError(clientId, SessionStage.Initialization, e);
                        }
                        catch (Exception e2)
                        {
                            await Tasks.DefaultOnError(e);
                        }
                        _ = SendKicked(clientId, new Kicked().WithSessionInitializationErrorReason());
                        server.Close(clientId);
                    }) ?? Task.CompletedTask);
                }

                // This function is invoked when a client was logged out due
                // to a logout or kick command, and the session was already
                // established and loaded before that. The session must end
                // and the connection must be closed.
                private async Task OnLoggedOut(ulong clientId, Kicked reason)
                {
                    // 1. The session will still exist.
                    //   1.1. But the client already received the kick/logged-out message.
                    //   1.2. "reason" will be default(Kicked) on graceful logout.
                    // 2. Invoke the "session terminating" hook, considering kick reason.
                    // 3. On error:
                    //   3.1. Handle the error appropriately.
                    // 4. Remove the session.
                    // 5. Close the connection.
                    await (OnSessionTerminating?.InvokeAsync(clientId, reason, async (e) =>
                    {
                        try
                        {
                            await OnSessionError(clientId, SessionStage.Termination, e);
                        }
                        catch (Exception e2)
                        {
                            await Tasks.DefaultOnError(e);
                        }
                    }) ?? Task.CompletedTask);
                    RemoveSession(clientId);
                    try
                    {
                        server.Close(clientId);
                    }
                    catch (Exception) {}
                }

                /// <summary>
                ///   Gets the account data for a certain id. It will return
                ///   <code>default(AccountDataType)</code> if it is not
                ///   found, and will raise an error if something wrong goes
                ///   on while fetching. This method is asynchronous, but
                ///   nevertheless any error occuring here will cause the
                ///   session halt and the client will be kicked from the
                ///   game due to the internal error.
                /// </summary>
                /// <param name="id">The id of the account to get the data from</param>
                /// <returns>The account data</returns>
                protected abstract Task<AccountDataType> FindAccount(AccountIDType id);

                /// <summary>
                ///   This is the current management mode when the logged
                ///   account is already logged in another connection.
                /// </summary>
                /// <returns>The operation mode when the session is already logged in</returns>
                protected abstract AccountAlreadyLoggedManagementMode IfAccountAlreadyLoggedIn();

                /// <summary>
                ///   Starts the session for a connection id with the given account
                ///   data. The session was already added to the mapping, but this
                ///   method is the one that initializes the actual player in the
                ///   actual game: objects, interactions, and whatever is needed.
                ///   Any error raised here will cause a session halt and the client
                ///   will be kicked from the game due to the internal error.
                /// </summary>
                /// <param name="clientId">The id of the connection whose session is being initialized</param>
                /// <param name="accountDta">The account data to initialize the session for</param>
                public event Func<ulong, AccountDataType, Task> OnSessionStarting;

                /// <summary>
                ///   Terminates the session for a connection id with a given reason.
                ///   The connection already received a kick message, so far, but the
                ///   session still exists for now. The cleanup to do here involves
                ///   removing all the game objects and logic related to this session.
                ///   This is: removing the client from the game, effectively.
                /// </summary>
                /// <param name="clientId">The id of the connection whose session is being terminated</param>
                /// <param name="reason">The kick reason. It will be null if it was graceful logout</param>
                public event Func<ulong, Kicked, Task> OnSessionTerminating;

                /// <summary>
                ///   Handles any error, typically logging it, at a given session handling
                ///   stage, and for a particular client. This will not cause the error to
                ///   be forgiven: the connection will be closed anyway. For the termination
                ///   stage, the session will still exist.
                /// </summary>
                /// <param name="clientId">The id of the connection whose session handling caused the error</param>
                /// <param name="stage">The stage where the error occurred</param>
                /// <param name="error">The error itself</param>
                protected abstract Task OnSessionError(ulong clientId, SessionStage stage, Exception error);
            }
        }
    }
}
