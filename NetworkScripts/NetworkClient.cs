//---NETWORK CLIENT---//
//---------This script handles the network communications from the client to the server.

//-------------//
//---IMPORTS---//
//-------------//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System;
using Project.Utility;
using Project.Scriptable;
using Project.Gameplay;
using TMPro;
using Project.Networking.Client;

//-----------//
//---CLASS---//
//-----------//

namespace Project.Networking {
    //Inherit 'SocketIOComponent' so we may use features from SocketIO
    public class NetworkClient : SocketIOComponent
    {
        /*-------------*/
        /*---Scirpts---*/
        /*-------------*/
        private NetworkMessaging mMessagingScript;
        private NetworkMigration mMigrationScript;
        private NetworkUpdates mUpdatesScript;
        private NetworkServerSpawning mServerSpawningScript;

        /*---------------*/
        /*---Variables---*/
        /*---------------*/

        public const float SERVER_UPDATE_TIME = 10;     //10% of the server update (currently 100 ms)

        [Header("Network Client")]

        [SerializeField]
        private Transform networkContainer;                     //The parent transform of our network instantiations. (Spawning a player, bullet, etc.)


        public static string ClientID { get; private set; }     //The ID of our client.

        

        private Dictionary<string, NetworkIdentity> serverObjects;      //The network ID of the objects we have spawned.


        public AccessToken mAccessToken;                        //The user's access token, to be filled by the uer.

        /*---------------------*/
        /*---Primary Methods---*/
        /*---------------------*/

        public override void Start()
        {
            SetInitialReferences();
            SetFrameRate();
            base.Start();   //This calls the Start method off of the base class.
            SetupEvents();
        }

        public override void Update()
        {
            base.Update();
            mMessagingScript.runCheck();
        }

        private void SetFrameRate()
        {
            Debug.Log("Set frame rate to 30");
            Application.targetFrameRate = 30;
        }

        /*-------------------*/
        /*---Event Methods---*/
        /*-------------------*/

        /** Sets up the events for this client.  Ran on Start(). */
        private void SetupEvents()
        {
            //When open, log connection.
            On("open", (Event) =>
            {
                mMigrationScript.OnOpen(Event);
            });

            //When we have registered, set our client ID from the data passed in.
            On("register", (Event) =>
            {
                mMigrationScript.OnRegister(Event);
            });

            //When we spawn into the lobby...
            On("spawn", (Event) =>
            {
                mMigrationScript.OnSpawn(Event, this);
            });

            //When something disconnects from the lobby...
            On("disconnected", (Event) =>
            {
                mMigrationScript.OnDisconnected(Event);
            });

            //When a position is updated...
            On("updatePosition", (Event) => {
                mUpdatesScript.OnUpdatePosition(Event);
            });

            //When a rotation is updated...
            On("updateRotation", (Event) =>
            {
                mUpdatesScript.OnUpdateRotation(Event);
            });

            //When the server spawns an object...
            On("serverSpawn", (Event) =>
            {
                mServerSpawningScript.OnServerSpawn(Event, this);
            });

            //When the server unspawns the object...
            On("serverUnspawn", (Event) =>
            {
                mServerSpawningScript.OnServerUnspawn(Event);
            });

            //When any player dies...
            On("playerDied", (Event) =>
            {
                mUpdatesScript.OnPlayerDied(Event);
            });

            //When any player respawns...
            On("playerRespawn", (Event) =>
            {
                mUpdatesScript.OnPlayerRespawn(Event);
            });

            //When a user reference is found from MongoDB and sent back...
            On("sendUserFromToken", (Event) =>
            {
                mMessagingScript.OnSendUserFromToken(Event);
            });

            //When a new message is sent...
            On("newMessage", (Event) =>
            {
                //Get messages from server.
                Emit("getMessages");
            });

            //When we retrieve messages from the server...
            On("returnMessages", (Event) =>
            {
                mMessagingScript.OnReturnMessages(Event);
            });
        }

        /** <summary> Sets the initial references for this script</summary>
         * */
        private void SetInitialReferences()
        {
            //Scripts
            if (GetComponent<NetworkMessaging>() != null)
            {
                mMessagingScript = GetComponent<NetworkMessaging>();
                mMessagingScript.SetIntialReferences();
            }
            else Debug.LogError("Missing essential script");

            if(GetComponent<NetworkMigration>() != null)
            {
                mMigrationScript = GetComponent<NetworkMigration>();
                mMigrationScript.SetInitialReferences();
            }
            else Debug.LogError("Missing essential script");

            if (GetComponent<NetworkUpdates>() != null)
            {
                mUpdatesScript = GetComponent<NetworkUpdates>();
                mUpdatesScript.SetInitialReferences();
            }
            else Debug.LogError("Missing essential script");

            if (GetComponent<NetworkServerSpawning>() != null)
            {
                mServerSpawningScript = GetComponent<NetworkServerSpawning>();
                mServerSpawningScript.SetInitialReferences();
            }
            else Debug.LogError("Missing essential script");

            //Classes
            mAccessToken = new AccessToken();

            serverObjects = new Dictionary<string, NetworkIdentity>();
        }

        /// <summary>
        /// Attempt to join the lobby.
        /// </summary>
        public void AtemptToJoinLobby()
        {
            Emit("joinGame", new JSONObject(JsonUtility.ToJson(mAccessToken)));
        }

        //----------------------//
        //---ACCESSOR METHODS---//
        //----------------------//

        public string getClientID()
        {
            return ClientID;
        }

        public void setClientID(string ID)
        {
            ClientID = ID;
        }

        public Dictionary<string, NetworkIdentity> getServerObjects()
        {
            return serverObjects;
        }

        public Transform getNetworkContainer()
        {
            return networkContainer;
        }

    };

    //---------------------//
    //---MODULAR CLASSES---//
    //---------------------//

    [Serializable]
    public class Player
    {
        public string id;
        public Position position;
        public string username;
        public string accessToken;
    }

    public class AccessToken
    {
        public string accessToken;
        public string username;
    }

    public class Message
    {
        public string message;
    }

    [Serializable]
    public class Position
    {
        public float x;
        public float y;
        public float z;
    }

    [Serializable]
    public class PlayerRotation
    {
        public float tankRotation;
    }

    [Serializable]
    public class BulletData
    {
        public string id;
        public string activator;
        public Position position;
        public Position direction;
    }

    [Serializable]
    public class IDData
    {
        public string id;
    }


};