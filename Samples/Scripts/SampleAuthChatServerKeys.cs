using AlephVault.Unity.Meetgard.Auth.Protocols.Simple;
using AlephVault.Unity.Meetgard.Samples.Chat;
using AlephVault.Unity.Meetgard.Authoring.Behaviours.Server;
using AlephVault.Unity.Meetgard.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace AlephVault.Unity.Meetgard.Auth
{
    namespace Samples
    {
        /// <summary>
        ///   This behaviour lets manage the server side via
        ///   some keys in the inspector.
        /// </summary>
        [RequireComponent(typeof(SampleAuthChatProtocolServerSide))]
        public class SampleAuthChatServerKeys : MonoBehaviour
        {
            [SerializeField]
            private KeyCode startKey;

            [SerializeField]
            private KeyCode stopKey;

            private NetworkServer server;

            // Start is called before the first frame update
            void Awake()
            {
                server = GetComponent<NetworkServer>();
            }

            // Update is called once per frame
            void Update()
            {
                if (Input.GetKeyDown(startKey) && !server.IsListening)
                {
                    server.StartServer(IPAddress.Any, 6666);
                }
                else if (Input.GetKeyDown(stopKey) && server.IsListening)
                {
                    server.StopServer();
                }
            }
        }
    }
}
