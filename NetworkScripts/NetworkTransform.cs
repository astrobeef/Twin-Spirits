using Project.Utility;
using Project.Utility.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Networking
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkTransform : MonoBehaviour
    {

        [SerializeField]
        [GreyOut]
        private Vector3 mOldPosition;

        private NetworkIdentity mNetworkIdentity;
        private NetworkClient mNetworkClient;
        private Player mPlayer;

        private float mStillCounter = 0;

        // Start is called before the first frame update
        public void Start()
        {
            mNetworkIdentity = GetComponent<NetworkIdentity>();
            mNetworkClient = GameObject.Find("[ Code - Networking ]").GetComponent<NetworkClient>();
            mOldPosition = transform.position;
            mPlayer = new Player();

            mPlayer.position = new Position();
            mPlayer.position.x = 0;
            mPlayer.position.y = 0;
            mPlayer.position.z = 0;

            if (mNetworkClient)
            {
                mPlayer.username = mNetworkClient.mAccessToken.username;
            }

            if (!mNetworkIdentity.IsControlling())
            {
                enabled = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (mNetworkIdentity.IsControlling())
            {
                if(mOldPosition != transform.position)      //I could restructure this to be an event
                {
                    mOldPosition = transform.position;
                    mStillCounter = 0;
                    sendData();
                }
                else
                {
                    mStillCounter += Time.deltaTime;

                    if(mStillCounter >= 1)                  //I could restructure this to be an event
                    {
                        mStillCounter = 0;
                        sendData();
                    }
                }
            }
        }

        //Update player information
        private void sendData()
        {
            mPlayer.position.x = transform.position.x.TwoDecimals();
            mPlayer.position.y = transform.position.y.TwoDecimals();
            mPlayer.position.z = transform.position.z.TwoDecimals();

            mNetworkIdentity.GetSocket().Emit("updatePosition", new JSONObject(JsonUtility.ToJson(mPlayer)));
        }


    }
}