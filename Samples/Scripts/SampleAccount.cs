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
        [Serializable]
        public class SampleAccount : IRecordWithPreview<string, SampleAccountPreview>
        {
            public string Username;
            public string Password;

            public string GetID()
            {
                return Username;
            }

            public SampleAccountPreview GetProfileDisplayData()
            {
                return new SampleAccountPreview() { Username = Username };
            }
        }
    }
}
