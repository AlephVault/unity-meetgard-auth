using AlephVault.Unity.Meetgard.Auth.Protocols.Simple;
using AlephVault.Unity.Meetgard.Auth.Types;
using AlephVault.Unity.Meetgard.Types;
using AlephVault.Unity.Support.Generic.Authoring.Types;
using System;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Samples
    {
        /// <summary>
        ///   This sample login protocol has only one mean
        ///   of login: "Login:Sample" (Username, Password).
        /// </summary>
        public class SampleSimpleAuthProtocolServerSide : SimpleAuthProtocolServerSide<SampleSimpleAuthProtocolDefinition, Nothing, LoginFailed, Kicked, string, SampleAccountPreview, SampleAccount>
        {
            [Serializable]
            private class Accounts : Dictionary<string, SampleAccount> {}

#if UNITY_EDITOR
            [CustomPropertyDrawer(typeof(Accounts))]
            public class AccountsPropertyDrawer : DictionaryPropertyDrawer { }

#endif

            /// <summary>
            ///   The list of valid accounts.
            /// </summary>
            [SerializeField]
            private Accounts accounts = new Accounts();

            /// <summary>
            ///   The duplicate account management mode.
            /// </summary>
            [SerializeField]
            private AccountAlreadyLoggedManagementMode WhenAlreadyLogged = AccountAlreadyLoggedManagementMode.Reject;

            protected override async Task<SampleAccount> FindAccount(string id)
            {
                id = (id ?? "").Trim().ToLower();
                foreach (var pair in accounts)
                {
                    if (id.Trim().ToLower() == pair.Key.Trim().ToLower())
                    {
                        return pair.Value;
                    }
                }
                return null;
            }

            protected override AccountAlreadyLoggedManagementMode IfAccountAlreadyLoggedIn()
            {
                return WhenAlreadyLogged;
            }

            protected override async Task OnSessionError(ulong clientId, SessionStage stage, System.Exception error)
            {
                Debug.Log($"Exception on session stage {stage} for client id {clientId}: {error.GetType().FullName} - {error.Message}");
            }

            protected override void SetLoginMessageHandlers()
            {
                AddLoginMessageHandler<UserPass>("Sample", async (message) =>
                {
                    foreach(var pair in  accounts)
                    {
                        if ((message.Username ?? "").Trim().ToLower() == pair.Key.Trim().ToLower() && message.Password == pair.Value.Password)
                        {
                            return AcceptLogin(Nothing.Instance, message.Username);
                        }
                    }

                    return RejectLogin(new LoginFailed());
                });
            }

            public override async Task OnServerStarted()
            {
                Debug.Log($"SSAPServer :: Server started");
            }

            public override async Task OnServerStopped(System.Exception e)
            {
                Debug.Log($"SSAPServer :: Server stopped");
            }
        }
    }
}
