using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Auth.Types;
using AlephVault.Unity.Meetgard.Protocols;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Server;
using System;
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
                //
                //
                //
                // Wrappers are functions that wrap a typed or untyped handler
                // with specific behaviour. Two of them are supported so far:
                //
                //
                //

                // Wraps a permission checker function to invoke the wrapped
                // function and catch any error.
                private Func<ulong, Task<bool>> WrapAllowedCheck(Func<ulong, Task<bool>> allowed)
                {
                    return async (clientId) =>
                    {
                        try
                        {
                            return await allowed(clientId);
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                await OnSessionError(clientId, SessionStage.PermissionCheck, e);
                            }
                            catch (Exception e2)
                            {
                                await Tasks.DefaultOnError(e);
                            }

                            return false;
                        }
                    };
                }
                
                /// <summary>
                ///   Wraps a handler to require the client to be logged in.
                ///   This is related to a custom target definition type and
                ///   protocol server side type.
                /// </summary>
                /// <typeparam name="TargetDefinitionType">The related protocol definition type</typeparam>
                /// <param name="allowed">The callback to check whether the client is allowed to perform the action</param>
                /// <param name="handler">The message handler</param>
                /// <returns>The wrapped message handler</returns>
                public Func<ProtocolServerSide<TargetDefinitionType>, ulong, Task> LoginRequired<TargetDefinitionType>(Func<ulong, Task<bool>> allowed, Func<ProtocolServerSide<TargetDefinitionType>, ulong, Task> handler)
                    where TargetDefinitionType : ProtocolDefinition, new()
                {
                    Func<ulong, Task<bool>> isAllowed = WrapAllowedCheck(allowed);
                    return async (proto, clientId) =>
                    {
                        if (SessionExists(clientId))
                        {
                            if (await isAllowed(clientId))
                            {
                                await handler(proto, clientId);
                            }
                            else
                            {
                                _ = SendForbidden(clientId);
                            }
                        }
                        else
                        {
                            _ = SendNotLoggedIn(clientId);
                        }
                    };
                }

                /// <summary>
                ///   Wraps a handler to require the client to be logged in.
                ///   This is related to a custom target definition type and
                ///   protocol server side type.
                /// </summary>
                /// <typeparam name="TargetDefinitionType">The related protocol definition type</typeparam>
                /// <typeparam name="TargetProtocolType">The related protocol server side type</typeparam>
                /// <typeparam name="T">The message type</typeparam>
                /// <param name="allowed">The callback to check whether the client is allowed to perform the action</param>
                /// <param name="handler">The message handler</param>
                /// <returns>The wrapped message handler</returns>
                public Func<ProtocolServerSide<TargetDefinitionType>, ulong, T, Task> LoginRequired<TargetDefinitionType, T>(Func<ulong, Task<bool>> allowed, Func<ProtocolServerSide<TargetDefinitionType>, ulong, T, Task> handler)
                    where T : ISerializable, new()
                    where TargetDefinitionType : ProtocolDefinition, new()
                {
                    Func<ulong, Task<bool>> isAllowed = WrapAllowedCheck(allowed);
                    return async (proto, clientId, message) =>
                    {
                        if (SessionExists(clientId))
                        {
                            if (await isAllowed(clientId))
                            {
                                await handler(proto, clientId, message);
                            }
                            else
                            {
                                _ = SendForbidden(clientId);
                            }
                        }
                        else
                        {
                            _ = SendNotLoggedIn(clientId);
                        }
                    };
                }

                /// <summary>
                ///   Wraps a handler to require the client to be logged in.
                ///   This is related to a custom target definition type and
                ///   protocol server side type.
                /// </summary>
                /// <typeparam name="TargetDefinitionType">The related protocol definition type</typeparam>
                /// <param name="handler">The message handler</param>
                /// <returns>The wrapped message handler</returns>
                public Func<ProtocolServerSide<TargetDefinitionType>, ulong, Task> LoginRequired<TargetDefinitionType>(Func<ProtocolServerSide<TargetDefinitionType>, ulong, Task> handler)
                    where TargetDefinitionType : ProtocolDefinition, new()
                {
                    return async (proto, clientId) =>
                    {
                        if (SessionExists(clientId))
                        {
                            await handler(proto, clientId);
                        }
                        else
                        {
                            SendNotLoggedIn(clientId);
                        }
                    };
                }

                /// <summary>
                ///   Wraps a handler to require the client to be logged in.
                ///   This is related to a custom target definition type and
                ///   protocol server side type.
                /// </summary>
                /// <typeparam name="TargetDefinitionType">The related protocol definition type</typeparam>
                /// <typeparam name="T">The message type</typeparam>
                /// <param name="handler">The message handler</param>
                /// <returns>The wrapped message handler</returns>
                public Func<ProtocolServerSide<TargetDefinitionType>, ulong, T, Task> LoginRequired<TargetDefinitionType, T>(Func<ProtocolServerSide<TargetDefinitionType>, ulong, T, Task> handler)
                    where T : ISerializable, new()
                    where TargetDefinitionType : ProtocolDefinition, new()
                {
                    return async (proto, clientId, message) =>
                    {
                        if (SessionExists(clientId))
                        {
                            await handler(proto, clientId, message);
                        }
                        else
                        {
                            _ = SendNotLoggedIn(clientId);
                        }
                    };
                }

                /// <summary>
                ///   Wraps a handler to require the client to be logged out.
                ///   This is related to a custom target definition type and
                ///   protocol server side type.
                /// </summary>
                /// <typeparam name="TargetDefinitionType">The related protocol definition type</typeparam>
                /// <param name="handler">The message handler</param>
                /// <returns>The wrapped message handler</returns>
                public Func<ProtocolServerSide<TargetDefinitionType>, ulong, Task> LogoutRequired<TargetDefinitionType>(Func<ProtocolServerSide<TargetDefinitionType>, ulong, Task> handler)
                    where TargetDefinitionType : ProtocolDefinition, new()
                {
                    return async (proto, clientId) =>
                    {
                        if (Handshake.IsConnectionHandshakePending(clientId))
                        {
                            await handler(proto, clientId);
                        }
                        else
                        {
                            _ = SendAlreadyLoggedIn(clientId);
                        }
                    };
                }

                /// <summary>
                ///   Wraps a handler to require the client to be logged out.
                ///   This is related to a custom target definition type and
                ///   protocol server side type.
                /// </summary>
                /// <typeparam name="TargetDefinitionType">The related protocol definition type</typeparam>
                /// <typeparam name="T">The message type</typeparam>
                /// <param name="handler">The message handler</param>
                /// <returns>The wrapped message handler</returns>
                public Func<ProtocolServerSide<TargetDefinitionType>, ulong, T, Task> LogoutRequired<TargetDefinitionType, T>(Func<ProtocolServerSide<TargetDefinitionType>, ulong, T, Task> handler)
                    where T : ISerializable, new()
                    where TargetDefinitionType : ProtocolDefinition, new()
                {
                    return async (proto, clientId, message) =>
                    {
                        if (Handshake.IsConnectionHandshakePending(clientId))
                        {
                            await handler(proto, clientId, message);
                        }
                        else
                        {
                            _ = SendAlreadyLoggedIn(clientId);
                        }
                    };
                }
            }
        }
    }
}