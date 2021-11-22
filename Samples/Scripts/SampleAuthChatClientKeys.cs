using AlephVault.Unity.Meetgard.Authoring.Behaviours.Client;
using UnityEngine;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Samples
    {
        /// <summary>
        ///   This behaviour lets manage the client side via
        ///   some keys in the inspector.
        /// </summary>
        [RequireComponent(typeof(SampleAuthChatProtocolClientSide))]
        public class SampleAuthChatClientKeys : MonoBehaviour
        {
            [SerializeField]
            private KeyCode connectKey;

            [SerializeField]
            private KeyCode disconnectKey;

            [SerializeField]
            private KeyCode sendMessageKey;

            private SampleSimpleAuthProtocolClientSide auth;
            private SampleAuthChatProtocolClientSide protocol;
            private NetworkClient client;

            // Start is called before the first frame update
            void Awake()
            {
                auth = GetComponent<SampleSimpleAuthProtocolClientSide>();
                protocol = GetComponent<SampleAuthChatProtocolClientSide>();
                client = GetComponent<NetworkClient>();
            }

            // Update is called once per frame
            void Update()
            {
                if (Input.GetKeyDown(connectKey) && !client.IsConnected)
                {
                    client.Connect("127.0.0.1", 6666);
                }
                else if (Input.GetKeyDown(disconnectKey) && client.IsConnected)
                {
                    Logout();
                }
                else if (Input.GetKeyDown(sendMessageKey) && client.IsConnected)
                {
                    protocol.Say("Lorem ipsum dolor sit amet");
                }
            }

            private async void Logout()
            {
                await auth.Logout();
            }
        }
    }
}
