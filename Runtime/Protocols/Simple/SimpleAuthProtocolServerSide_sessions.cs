using System;
using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Auth.Types;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Server;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
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
                // This section defines public methods to interact with
                // the session data, and some private methods to add or
                // remove sessions for a given collection.

                // ****************************************************

                // The session data is just a mapping of keys and values.
                // An additional feature of this class is that it allows
                // clearing everything but the keys starting with __AV:MEETGARD:AUTH__.
                private class SessionData : Dictionary<string, object>
                {
                    /// <summary>
                    ///   Clears the user-defined keys.
                    /// </summary>
                    public void ClearUserEntries()
                    {
                        List<string> keys = (from key in Keys where !key.StartsWith("__AV:MEETGARD:AUTH__") select key).ToList();
                        foreach (string key in keys)
                        {
                            Remove(key);
                        }
                    }
                }

                // A session holds session data, a connection id, and an
                // account id. The data will only be accessible here -in
                // the authenticator- but also indirectly (via wrapper
                // methods) to the outside world.
                private class Session : Tuple<ulong, AccountIDType, SessionData>
                {
                    /// <summary>
                    ///   Instantiates the tuple with a default value: an empty,
                    ///   yet not null, session data dictionary.
                    /// </summary>
                    /// <param name="connectionId">The ID of the connection</param>
                    /// <param name="accountId">The full ID/Realm of the account</param>
                    public Session(ulong connectionId, AccountIDType accountId) : base(connectionId, accountId, new SessionData()) { }
                }

                // A session mapped by its connection id.
                private class SessionByConnectionId : ConcurrentDictionary<ulong, Session> { }

                // All the sessions mapped by a single account id.
                private class SessionByAccountId : ConcurrentDictionary<AccountIDType, HashSet<Session>> { }

                //
                //
                //
                // Data starts here...
                //
                //
                //

                // Sessions will be tracked by the connection they belong to.
                private SessionByConnectionId sessionByConnectionId = new SessionByConnectionId();

                // Sessions will be tracked by the account id they belong to.
                private SessionByAccountId sessionByAccountId = new SessionByAccountId();

                //
                //
                //
                // Methods start here...
                //
                //
                //

                // Creates a new session (and adds it to the internal dictionary).
                private Session AddSession(ulong clientId, AccountIDType accountId)
                {
                    Session session = new Session(clientId, accountId);
                    sessionByConnectionId.TryAdd(clientId, session);
                    sessionByAccountId.TryAdd(accountId, new HashSet<Session>());
                    sessionByAccountId[accountId].Add(session);
                    return session;
                }

                // Removes a session by its connection id.
                private bool RemoveSession(ulong clientId)
                {
                    if (sessionByConnectionId.TryRemove(clientId, out Session session))
                    {
                        sessionByAccountId[session.Item2].Remove(session);
                        if (sessionByAccountId[session.Item2].Count == 0) sessionByAccountId.TryRemove(session.Item2, out _);
                        return true;
                    }
                    return false;
                }

                /// <summary>
                ///   Tells whether a session exists for a given connection.
                /// </summary>
                /// <param name="clientId">The connection whose session is told to exist or not</param>
                /// <returns>Whether the session exists or not</returns>
                public bool SessionExists(ulong clientId)
                {
                    return sessionByConnectionId.ContainsKey(clientId);
                }

                /// <summary>
                ///   Sets a given data value in the session for a given connection.
                /// </summary>
                /// <param name="clientId">The connection whose session is to be affected</param>
                /// <param name="key">The in-session key</param>
                /// <param name="value">The new value</param>
                public void SetSessionData(ulong clientId, string key, object value)
                {
                    try
                    {
                        sessionByConnectionId[clientId].Item3[key] = value;
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new Exception("Trying to access a missing session");
                    }
                }

                /// <summary>
                ///   Gets a given data value in the session from a given connection.
                /// </summary>
                /// <param name="clientId">The connection whose session is to be queried</param>
                /// <param name="key">The in-session key</param>
                /// <returns>The session value</returns>
                /// <remarks>Throws a KeyNotFound error for a missing session key</remarks>
                public object GetSessionData(ulong clientId, string key)
                {
                    Session session;

                    try
                    {
                        session = sessionByConnectionId[clientId];
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new Exception("Trying to access a missing session");
                    }

                    return session.Item3[key];
                }

                /// <summary>
                ///   Tries to get a given data value in the session for a given connection.
                /// </summary>
                /// <param name="clientId">The connection whose session is to be queried</param>
                /// <param name="key">The in-session key</param>
                /// <param name="data">The data to be retrieved</param>
                /// <returns>Whether the key existed and data was retrieved</returns>
                public bool TryGetSessionData(ulong clientId, string key, out object data)
                {
                    try
                    {
                        return sessionByConnectionId[clientId].Item3.TryGetValue(key, out data);
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new Exception("Trying to access a missing session");
                    }
                }

                /// <summary>
                ///   Removes a given data value in the session for a given connection.
                /// </summary>
                /// <param name="clientId">The connection whose session is to be affected</param>
                /// <param name="key">The in-session key to be removed</param>
                /// <returns>Whether that key was removed or not</returns>
                public bool RemoveSessionData(ulong clientId, string key)
                {
                    try
                    {
                        return sessionByConnectionId[clientId].Item3.Remove(key);
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new Exception("Trying to access a missing session");
                    }
                }

                /// <summary>
                ///   Clears all the session entries in its data.
                /// </summary>
                /// <param name="clientId">The connection whose session is to be affected</param>
                /// <param name="userDataOnly">Whether to remove only the user-defined entries, or the whole session data</param>
                public void ClearSessionUserData(ulong clientId, bool userDataOnly = true)
                {
                    Session session;

                    try
                    {
                        session = sessionByConnectionId[clientId];
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new Exception("Trying to access a missing session");
                    }

                    if (userDataOnly)
                    {
                        session.Item3.ClearUserEntries();
                    }
                    else
                    {
                        session.Item3.Clear();
                    }
                }

                /// <summary>
                ///   Tells whether the session data contains a particular key.
                /// </summary>
                /// <param name="clientId">The connection whose session is to be queried</param>
                /// <param name="key">The in-session key</param>
                /// <returns></returns>
                public bool SessionContainsKey(ulong clientId, string key)
                {
                    try
                    {
                        return sessionByConnectionId[clientId].Item3.ContainsKey(key);
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new Exception("Trying to access a missing session");
                    }
                }
            }
        }
    }
}