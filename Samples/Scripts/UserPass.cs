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
        public class UserPass : ISerializable
        {
            public string Username;
            public string Password;

            public void Serialize(Serializer serializer)
            {
                serializer.Serialize(ref Username);
                serializer.Serialize(ref Password);
            }
        }
    }
}
