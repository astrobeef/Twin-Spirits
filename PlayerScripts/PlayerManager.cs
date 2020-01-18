using Project.Networking;
using Project.Scriptable;
using Project.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Player
{

    public class PlayerManager : MonoBehaviour
    {
        const float BARREL_PIVOT_OFFSET = 90.0f;

        private NetworkClient ourClient;
        public bool ourClientIsConnected = false;
        public Transform OfflineContainer;
        public ServerObjects OfflineSpawnables;

        [Header("Data")]
        [SerializeField]
        private float mRotation = 60;

        [Header("Object References")]
        [SerializeField]
        private GameObject myCamerasObject;
        private Camera myCamera;

        [Header("Class References")]
        [SerializeField]
        private NetworkIdentity mNetworkIdentity;
        [SerializeField]
        private Player_Movement mMovementScript;
        [SerializeField]
        private Player_Shooting mShootingScript;
        [SerializeField]
        private Player_Rotation mRotationScript;

        public void Start()
        {
            StartCoroutine(DelayedStart());
        }

        private IEnumerator DelayedStart()
        {
            yield return new WaitForEndOfFrame();

            SetInitialReferences();
        }

        private void SetInitialReferences()
        {
            /** CHECK IF OUR CLIENT IS CONNECTED TO NETWORK
             * > Find our client object.
             * > Check if our client is connected.
             * > Establish offline container if disconnected.
             * */
            ourClient = GameObject.Find("[ Code - Networking ]").GetComponent<NetworkClient>();
            ourClientIsConnected = ourClient.clientIsConnected;
            if (!ourClientIsConnected)
            {
                Debug.LogWarning("We are not connected to the server");
                OfflineContainer = GameObject.Find("[ Offline Spawned Objects ]").GetComponent<Transform>();
                OfflineSpawnables = ourClient.mServerSpawningScript.getServerSpawnables();
            }

            //IF we are the current client OR we are NOT connected, then...
            if (mNetworkIdentity.IsControlling() || !ourClientIsConnected)
            {
                enableCameras();
            }

            //-----------------//
            //---Script Refs---//
            //-----------------//
            if (GetComponent<Player_Movement>() != null)
            {
                mMovementScript = GetComponent<Player_Movement>();
                mMovementScript.SetInitialReferences();
            }
            else
            {
                Debug.LogError("Missing essential script");
            }

            if(GetComponent<Player_Shooting>() != null)
            {
                mShootingScript = GetComponent<Player_Shooting>();
                mShootingScript.SetInitialReferences();
            }
            else
            {
                Debug.LogError("Missing essential script");
            }

            if(GetComponent<Player_Rotation>() != null)
            {
                mRotationScript = GetComponent<Player_Rotation>();
                mRotationScript.SetInitialReferences();
            }
            else
            {
                Debug.LogError("Missing essential script");
            }
        }

        void Update()
        {
            if (mNetworkIdentity.IsControlling() || !ourClientIsConnected)
            {
                checkMovement();
                checkShooting();
                checkRotation();
            }
        }

        private void checkMovement()
        {
            if(mMovementScript != null)
            {
                mMovementScript.runCheck();
            }
        }

        private void checkRotation()
        {
            if (mRotationScript != null)
            {
                mRotationScript.runCheck();
            }
        }

        private void checkShooting()
        {
            if (mShootingScript != null)
            {
                mShootingScript.runCheck();
            }
        }

        private void enableCameras()
        {
            myCamerasObject = transform.Find("Cameras").gameObject;
            myCamerasObject.SetActive(true);

            myCamera = myCamerasObject.transform.Find("Render Camera").GetComponent<Camera>();

            if(myCamera.transform.GetComponent<AudioListener>() != null)
            {
                myCamera.transform.GetComponent<AudioListener>().enabled = true;
            }

        }

        /*--------------------------*/
        /*-----ACCESSOR METHODS-----*/
        /*--------------------------*/

        public NetworkIdentity getNetworkIdentity()
        {
            return mNetworkIdentity;
        }

        public Player_Rotation getRotationScript()
        {
            return mRotationScript;
        }
    }

}