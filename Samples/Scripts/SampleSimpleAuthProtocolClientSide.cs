using AlephVault.Unity.Meetgard.Auth.Protocols.Simple;
using AlephVault.Unity.Meetgard.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Samples
    {
        /// <summary>
        ///   This sample login protocol has only one mean
        ///   of login: "Login:Sample" (Username, Password).
        /// </summary>
        public class SampleSimpleAuthProtocolClientSide : SimpleAuthProtocolClientSide<SampleSimpleAuthProtocolDefinition, Nothing, LoginFailed, Kicked>
        {
            /// <summary>
            ///   This is a sample login method with
            ///   username and password.
            /// </summary>
            private Func<UserPass, Task> SendSampleLogin;

            /// <summary>
            ///   The sample username.
            /// </summary>
            [SerializeField]
            private string Username;

            /// <summary>
            ///   The sample password.
            /// </summary>
            [SerializeField]
            private string Password;

            protected new void Awake()
            {
                base.Awake();
                OnWelcome += AuthProtocol_OnWelcome;
                OnTimeout += AuthProtocol_OnTimeout;
                OnLoginOK += AuthProtocol_OnLoginOK;
                OnLoginFailed += AuthProtocol_OnLoginFailed;
                OnKicked += AuthProtocol_OnKicked;
                OnForbidden += AuthProtocol_OnForbidden;
                OnLoggedOut += AuthProtocol_OnLoggedOut;
                OnNotLoggedIn += AuthProtocol_OnNotLoggedIn;
                OnAlreadyLoggedIn += AuthProtocol_OnAlreadyLoggedIn;
                OnAccountAlreadyInUse += AuthProtocol_OnAccountAlreadyInUse;
            }

            private async Task AuthProtocol_OnWelcome()
            {
                Debug.Log($"SSAPClient({Username}) :: welcome");
                _ = SendSampleLogin(new UserPass() { Username = Username, Password = Password });
            }

            private async Task AuthProtocol_OnTimeout()
            {
                Debug.Log($"SSAPClient({Username}) :: timeout");
            }

            private async Task AuthProtocol_OnLoginOK(Nothing arg)
            {
                Debug.Log($"SSAPClient({Username}) :: login ok");
            }

            private async Task AuthProtocol_OnLoginFailed(LoginFailed arg)
            {
                Debug.Log($"SSAPClient({Username}) :: login failed: {arg}");
            }

            private async Task AuthProtocol_OnKicked(Kicked arg)
            {
                Debug.Log($"SSAPClient({Username}) :: Kicked: {arg}");
            }

            private async Task AuthProtocol_OnForbidden()
            {
                Debug.Log($"SSAPClient({Username}) :: forbidden");
            }

            private async Task AuthProtocol_OnLoggedOut()
            {
                Debug.Log($"SSAPClient({Username}) :: logged out");
            }

            private async Task AuthProtocol_OnNotLoggedIn()
            {
                Debug.Log($"SSAPClient({Username}) :: not logged in");
            }

            private async Task AuthProtocol_OnAlreadyLoggedIn()
            {
                Debug.Log($"SSAPClient({Username}) :: already logged in");
            }

            private async Task AuthProtocol_OnAccountAlreadyInUse()
            {
                Debug.Log($"SSAPClient({Username}) :: account already in use");
            }

            void OnDestroy()
            {
                OnWelcome -= AuthProtocol_OnWelcome;
                OnTimeout -= AuthProtocol_OnTimeout;
                OnLoginOK -= AuthProtocol_OnLoginOK;
                OnLoginFailed -= AuthProtocol_OnLoginFailed;
                OnKicked -= AuthProtocol_OnKicked;
                OnForbidden -= AuthProtocol_OnForbidden;
                OnLoggedOut -= AuthProtocol_OnLoggedOut;
                OnNotLoggedIn -= AuthProtocol_OnNotLoggedIn;
                OnAlreadyLoggedIn -= AuthProtocol_OnAlreadyLoggedIn;
                OnAccountAlreadyInUse -= AuthProtocol_OnAccountAlreadyInUse;
            }

            protected override void MakeLoginRequestSenders()
            {
                SendSampleLogin = MakeLoginRequestSender<UserPass>("Sample");
            }
        }
    }
}
