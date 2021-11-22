using AlephVault.Unity.Binary;
using AlephVault.Unity.Meetgard.Auth.Types;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Server;
using System;
using System.Threading;
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
            > : ProtocolServerSide<Definition>
                where LoginOK : ISerializable, new()
                where LoginFailed : ISerializable, new()
                where Kicked : IKickMessage<Kicked>, new()
                where AccountPreviewDataType : ISerializable, new()
                where AccountDataType : IRecordWithPreview<AccountIDType, AccountPreviewDataType>
                where Definition : SimpleAuthProtocolDefinition<LoginOK, LoginFailed, Kicked>, new()
            {
                // A mutex for protocol-exclusive actions handling the sessions
                private SemaphoreSlim mutex = new SemaphoreSlim(1, 1);

                // Wraps this as an exclusive execution action
                private async Task Exclusive(Func<Task> action)
                {
                    try
                    {
                        await mutex.WaitAsync();
                        await action();
                    }
                    finally
                    {
                        mutex.Release();
                    }
                }
            }
        }
    }
}