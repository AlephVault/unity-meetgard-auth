using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Auth.Types;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Server;
using System;
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
            > {
                /// <summary>
                ///   This is a sender for the Welcome message.
                /// </summary>
                private Func<ulong, Task> SendWelcome;

                /// <summary>
                ///   This is a sender for the Timeout message.
                /// </summary>
                private Func<ulong, Task> SendTimeout;

                /// <summary>
                ///   This is a sender for the LoginOK message.
                /// </summary>
                private Func<ulong, LoginOK, Task> SendLoginOK;

                /// <summary>
                ///   This is a sender for the LoginFailed message.
                /// </summary>
                private Func<ulong, LoginFailed, Task> SendLoginFailed;

                /// <summary>
                ///   This is a sender for the Kicked message.
                /// </summary>
                private Func<ulong, Kicked, Task> SendKicked;

                /// <summary>
                ///   This is a sender of the LoggedOut message.
                /// </summary>
                private Func<ulong, Task> SendLoggedOut;

                /// <summary>
                ///   This is a sender for the NotLoggedIn message.
                /// </summary>
                private Func<ulong, Task> SendNotLoggedIn;

                /// <summary>
                ///   This is a sender for the Forbidden message.
                /// </summary>
                private Func<ulong, Task> SendForbidden;

                /// <summary>
                ///   This is a sender for the AccountAlreadyInUse message.
                /// </summary>
                private Func<ulong, Task> SendAccountAlreadyInUse;

                /// <summary>
                ///   This is a sender for the AlreadyLoggedIn message.
                /// </summary>
                private Func<ulong, Task> SendAlreadyLoggedIn;

                // This function is called on Start() to initialize
                // all of the server side message senders.
                private void MakeSenders()
                {
                    SendLoginOK = MakeSender<LoginOK>("OK");
                    SendLoginFailed = MakeSender<LoginFailed>("Failed");
                    SendKicked = MakeSender<Kicked>("Kicked");
                    SendLoggedOut = MakeSender("LoggedOut");
                    SendNotLoggedIn = MakeSender("NotLoggedIn");
                    SendForbidden = MakeSender("Forbidden");
                    SendAlreadyLoggedIn = MakeSender("AlreadyLoggedIn");
                    SendAccountAlreadyInUse = MakeSender("AccountAlreadyInUse");
                }
            }
        }
    }
}