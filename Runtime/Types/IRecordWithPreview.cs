using AlephVault.Unity.Binary;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Types
    {
        /// <summary>
        ///   A profile has two meaningful things: its ID
        ///   (which will only matter server-side) and the display
        ///   data (which will matter for both client and server
        ///   side, but primarily arranged for client side).
        /// </summary>
        /// <typeparam name="IDType">The type of the record id</typeparam>
        /// <typeparam name="ProfileDisplayDataType">The type of the record display data (which is also <see cref="ISerializable"/>)</typeparam>
        public interface IRecordWithPreview<IDType, ProfileDisplayDataType>
            where ProfileDisplayDataType : ISerializable
        {
            /// <summary>
            ///   Returns the id of the profile. Only meaningful
            ///   in the server side.
            /// </summary>
            public IDType GetID();

            /// <summary>
            ///   Returns the display data of the profile. Meaningful
            ///   mainly in the client side.
            /// </summary>
            public ProfileDisplayDataType GetProfileDisplayData();
        }
    }
}