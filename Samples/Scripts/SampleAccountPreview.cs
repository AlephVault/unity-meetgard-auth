using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Auth.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Samples
    {
        /// <summary>
        ///   This sample account preview includes only the username.
        /// </summary>
        public class SampleAccountPreview : ISerializable
        {
            public string Username;

            public void Serialize(Serializer serializer)
            {
                serializer.Serialize(ref Username);
            }
        }
    }
}
