using Project.Player;
using Project.Utility;
using Project.Utility.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Project.Networking
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkRotation : MonoBehaviour
    {
        [Header("Referenced Values")]
        [SerializeField]
        [GreyOut]
        private float oldTankRotation;

        [Header("Class References")]
        [SerializeField]
        private PlayerManager playerManager;

        private NetworkIdentity networkIdentity;
        private PlayerRotation player;

        private float stillCounter = 0;

        // Start is called before the first frame update
        void Start()
        {
            networkIdentity = GetComponent<NetworkIdentity>();

            player = new PlayerRotation();
            player.tankRotation = 0;

            if (!networkIdentity.IsControlling())
            {
                enabled = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (networkIdentity.IsControlling())
            {
                if (oldTankRotation != transform.localEulerAngles.y)
                {
                    oldTankRotation = transform.localEulerAngles.y;

                    stillCounter = 0;
                    sendData();
                }
                else
                {
                    stillCounter += Time.deltaTime;

                    if(stillCounter >= 1)
                    {
                        stillCounter = 0;
                        sendData();
                    }
                }
            }
        }

        private void sendData()
        {
            player.tankRotation = transform.localEulerAngles.y.TwoDecimals();

            networkIdentity.GetSocket().Emit("updateRotation", new JSONObject(JsonUtility.ToJson(player)));
        }
    }
}