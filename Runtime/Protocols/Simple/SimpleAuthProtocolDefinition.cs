using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Auth.Types;
using AlephVault.Unity.Meetgard.Protocols;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Protocols
    {
        namespace Simple
        {
            /// <summary>
            ///   This protocol provides authentication features
            ///   and session management (concretely: login, logout,
            ///   and kick). The login message will not be implemented,
            ///   so multiple login messages (each one for a different 
            ///   realm) can be implemented in a single auth protocol.
            /// </summary>
            /// <typeparam name="LoginOK">The message content type for when the login is accepted</typeparam>
            /// <typeparam name="LoginFailed">The message content type for when the login is rejected</typeparam>
            /// <typeparam name="Kicked">The message content type for when the user is kicked (e.g. including reason)</typeparam>
            public abstract class SimpleAuthProtocolDefinition<LoginOK, LoginFailed, Kicked> : ProtocolDefinition
                where LoginOK : ISerializable, new()
                where LoginFailed : ISerializable, new()
                where Kicked : IKickMessage<Kicked>, new()
            {
                protected override void DefineMessages()
                {
                    // No "login" message will be provided explicitly,
                    // so multiple realms can be implemented, each with
                    // a different type.
                    DefineLoginMessages();
                    DefineServerMessage("Welcome");
                    DefineServerMessage("Timeout");
                    DefineServerMessage<LoginOK>("OK");
                    DefineServerMessage<LoginFailed>("Failed");
                    DefineServerMessage<Kicked>("Kicked");
                    DefineClientMessage("Logout");
                    DefineServerMessage("LoggedOut");
                    // These messages are intended for action-wrapping
                    // requiring login (and perhaps a permission criterion),
                    // or requiring not login.
                    DefineServerMessage("NotLoggedIn");
                    DefineServerMessage("AccountAlreadyInUse");
                    DefineServerMessage("AlreadyLoggedIn");
                    DefineServerMessage("Forbidden");
                }

                /// <summary>
                ///   Implement this method with several calls to
                ///   <see cref="DefineLoginMessage{T}(string)"/>,
                ///   each one for each allowed login method.
                /// </summary>
                protected abstract void DefineLoginMessages();

                /// <summary>
                ///   Defines a login message, from client to server.
                /// </summary>
                /// <typeparam name="T">The type of the login meesage</typeparam>
                /// <param name="method">The name of the method to use</param>
                protected void DefineLoginMessage<T>(string method) where T : ISerializable, new()
                {
                    DefineClientMessage<T>("Login:" + method);
                }
            }
        }
    }
}
