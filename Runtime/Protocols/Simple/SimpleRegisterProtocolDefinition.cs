using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Protocols;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Protocols
    {
        namespace Simple
        {
            /// <summary>
            ///   This protocol provides user registration features
            ///   only. The register message will not be implemented,
            ///   so multiple register messages (each for a different 
            ///   realm) can be implemented in a single protocol.
            /// </summary>
            /// <typeparam name="RegisterOK">The message content type for when the register is accepted</typeparam>
            /// <typeparam name="RegisterFailed">The message content type for when the register is rejected</typeparam>
            public abstract class SimpleRegisterProtocolDefinition<RegisterOK, RegisterFailed> : MandatoryHandshakeProtocolDefinition
                where RegisterOK : ISerializable, new()
                where RegisterFailed : ISerializable, new()
            {
                protected override void DefineMessages()
                {
                    // No "register" message will be provided explicitly,
                    // so multiple realms can be implemented, each with
                    // a different type.
                    DefineRegisterMessages();
                    base.DefineMessages();
                    DefineServerMessage<RegisterOK>("OK");
                    DefineServerMessage<RegisterFailed>("Failed");
                }
                
                /// <summary>
                ///   Implement this method with several calls to
                ///   <see cref="DefineRegisterMessage{T}(string)"/>,
                ///   each one for each allowed login method.
                /// </summary>
                protected abstract void DefineRegisterMessages();

                /// <summary>
                ///   Defines a register message, from client to server.
                ///   Typically, only ONE register method will be
                ///   invoked by this mean.
                /// </summary>
                /// <typeparam name="T">The type of the register message</typeparam>
                /// <param name="method">The name of the method to use</param>
                protected void DefineRegisterMessage<T>(string method) where T : ISerializable, new()
                {
                    DefineClientMessage<T>("Register:" + method);
                }
            }
        }
    }
}