using AlephVault.Unity.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Samples
    {
        public class LoginFailed : ISerializable
        {
            public string Reason;

            public void Serialize(Serializer serializer)
            {
                serializer.Serialize(ref Reason);
            }
        }
    }
}
