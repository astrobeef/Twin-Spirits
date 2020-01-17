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
using Project.Player;
using Project.Scriptable;
using Project.Gameplay;
using TMPro;

//-----------//
//---CLASS---//
//-----------//

namespace Project.Networking {
    //Inherit 'SocketIOComponent' so we may use features from SocketIO
    public class NetworkClient : SocketIOComponent
    {
        /*---------------*/
        /*---Variables---*/
        /*---------------*/

        public const float SERVER_UPDATE_TIME = 10;     //10% of the server update (currently 100 ms)

        [Header("Network Client")]

        [SerializeField]
        private Transform networkContainer;                     //The parent transform of our network instantiations. (Spawning a player, bullet, etc.)
        [SerializeField]
        private GameObject playerPrefab;                        //The prefab of our player to be instantiated.
        [SerializeField]
        private ServerObjects serverSpawnables;                 //Objects spawned by the server which are always identical.  (Bullets, but not players).

        public static string ClientID { get; private set; }     //The ID of our client.

        [Header("UI")]

        public TextMeshProUGUI accessTokenInput;                //The input field for the user's access token.
        public TextMeshProUGUI messageInput;                    //The input field for the user's message.
        public TextMeshProUGUI globalChatDisplay;               //The text field of the global chat.
        public GameObject joinGameUI;                           //The gameobject which contains all 'join game' UI.
        public GameObject pleaseWaitUI;                         //The gameobject which contains all 'please wait' UI.
        public GameObject globalChatUI;                         //The gameobject which contains all 'global chat' UI.
        public AccessToken mAccessToken;                        //The user's access token, to be filled by the uer.

        private bool didSendFetch;                              //Have we attempted to check our access token?
        private int mTimeToWait = 2;                            //How long to wait after a fetch.
        [SerializeField]
        private string mAccessToken_LastRequest;                //The previous token requested.  Used to compare to our new token.

        public string[] mGlobalMessages;                        //The messages to be displayed to global chat.
        public Message mMyMessage;                              //The message constructed by the client user.

        private Dictionary<string, NetworkIdentity> serverObjects;      //The network ID of the objects we have spawned.

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

            mMyMessage.message = messageInput.text.ToString();

            if (accessTokenInput.text.Length > 2)
            {
                mAccessToken.accessToken = accessTokenInput.text.Trim(new char[] { '\r', '\n'});
                //joinGameUI.SetActive(true);
                if (!didSendFetch && mAccessToken_LastRequest != mAccessToken.accessToken)
                {
                    didSendFetch = true;
                    FetchUserByToken();
                }
            }
            else
            {
                joinGameUI.SetActive(false);
            }
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
                Debug.Log("Connection made to the server");
            });

            //When we have registered, set our client ID from the data passed in.
            On("register", (Event) =>
            {
                ClientID = Event.data["id"].ToString().RemoveQuotes();
                Debug.LogFormat("Our Client's ID ({0})", ClientID);
            });

            //When we spawn into the lobby...
            On("spawn", (Event) =>
            {
                //Extract Data from Event
                string id = Event.data["id"].ToString().RemoveQuotes();     //Get our ID from the data passed in.

                Emit("getMessages");        //Send call to get global messages.

                /** SPAWN OUR PLAYER
                 * > Create the object as a child of our "networkContainer"
                 * > Set the name of our player.
                 * > Get the <NetworkIdentity> component off of the instantiated player, 'go'.
                 * > Set the controller ID and socket reference of the NetworkIdentity on the player.
                 * */
                GameObject go = Instantiate(playerPrefab, networkContainer);
                go.name = string.Format("Player ({0})", mAccessToken.username);     
                NetworkIdentity ni = go.GetComponent<NetworkIdentity>();
                ni.SetControllerID(id);
                ni.SetSocketReference(this);

                //Store reference to this instantiated object on this 'NetworkClient'.
                serverObjects.Add(id, ni);
            });

            //When something disconnects from the lobby...
            On("disconnected", (Event) =>
            {
                //Extract Data from Event
                string id = Event.data["id"].ToString().RemoveQuotes();     //Get our ID from the data passed in.

                /** FIND AND DESTROY PLAYER
                 * > Using the ID, find the match in our Dictionary of 'serverObjects'.
                 * > Destroy the found gameObject.
                 * > Remove reference to the gameObject from our Dictionary.
                 * */
                if(serverObjects[id] != null)
                {
                    GameObject go = serverObjects[id].gameObject;
                    Destroy(go);        //Remove from game
                    serverObjects.Remove(id);       //Remove from memory
                    Debug.Log("Player, " + id + ", disconnected");
                }
                else Debug.LogError("Could not find referenced ID in our serverObjects.  ID is false or we did not store it.");
            });

            //When a position is updated...
            On("updatePosition", (Event) => {
                //Extract Data from Event
                string id = Event.data["id"].ToString().RemoveQuotes();
                float x = Event.data["position"]["x"].f;
                float y = Event.data["position"]["y"].f;
                float z = Event.data["position"]["z"].f;

                /** FIND AND UPDATE OBJECT
                 * > Using the ID, find the matching <NetworkIdentity> from our 'serverObjects'
                 * > Update the position of the found NI using our passed Data.
                 * */
                if (serverObjects[id] != null)
                {
                    NetworkIdentity ni = serverObjects[id];
                    ni.transform.position = new Vector3(x, y, z);
                }
                else Debug.LogError("Could not find reference of the NI in our serverObjects.  ID is false or we did not store it.");
            });

            //When a rotation is updated...
            On("updateRotation", (Event) =>
            {
                //Extract Data from Event
                string id = Event.data["id"].ToString().RemoveQuotes();
                float tankRotation = Event.data["tankRotation"].f;
                float barrelRotation = Event.data["barrelRotation"].f;

                /** FIND AND UPDATE OBJECT
                 * > Using the ID, find the matching <NetworkIdentity> from our 'serverObjects'
                 * > Update the rotation of the found NI using our passed Data.
                 * > Update the rotation of the actual prefab using our passed Data.
                 * */
                if (serverObjects[id] != null)
                {
                    NetworkIdentity ni = serverObjects[id];
                    ni.transform.localEulerAngles = new Vector3(0, tankRotation, 0);        //Sets the network identity data.
                    ni.GetComponent<PlayerManager>().getRotationScript().SetRotation(tankRotation);     //Sets the rotation of the actual player prefab.
                }
                else Debug.LogError("Could not find reference of the NI in our serverObjects.  ID is false or we did not store it.");
            });

            //When the server spawns an object...
            On("serverSpawn", (Event) =>
            {
                //Extract Data from Event
                string name = Event.data["name"].str;
                string id = Event.data["id"].ToString().RemoveQuotes();
                float x = Event.data["position"]["x"].f;
                float y = Event.data["position"]["y"].f;
                float z = Event.data["position"]["z"].f;

                /** SPAWN SERVER OBJECT
                 * > Check if we have a reference to that object.
                 * > Find the object we are spawning from our 'serverSpawnables' class.
                 * > Set the position of the object to the server's passed position.
                 * > Get the <NetworkIdentity> of the spawned object.
                 * > Set reference on that NI for its ID and Socket Reference.
                 * */
                if (!serverObjects.ContainsKey(id))
                {
                    ServerObjectData sod = serverSpawnables.GetObjectByName(name);      //Find the server object by the name of the object.
                    var spawnedObject = Instantiate(sod.Prefab, networkContainer);      //Instantiate the object's prefab as a child of our 'networkContainer'
                    spawnedObject.transform.position = new Vector3(x, y, z);             
                    var ni = spawnedObject.GetComponent<NetworkIdentity>();
                    ni.SetControllerID(id);
                    ni.SetSocketReference(this);

                    //If bullet apply direction as well
                    if(name == "Bullet")
                    {
                        //Extract Data from Event
                        float directionX = Event.data["direction"]["x"].f;
                        float directionY = Event.data["direction"]["y"].f;
                        float directionZ = Event.data["direction"]["z"].f;
                        string activator = Event.data["activator"].ToString().RemoveQuotes();
                        float speed = Event.data["speed"].f;

                        //Calculate and apply directional data.
                        float rot = Mathf.Atan2(directionX, directionZ) * Mathf.Rad2Deg;
                        Vector3 currentRotation = new Vector3(0, rot - 90, 0);
                        spawnedObject.transform.rotation = Quaternion.Euler(currentRotation);

                        //Set parameters so that this bullet does not hit its creator.
                        WhoActivatedMe whoActivatedMe = spawnedObject.GetComponent<WhoActivatedMe>();
                        whoActivatedMe.SetActivator(activator);

                        //Set the direction and speed of the projectile.
                        Projectile projectile = spawnedObject.GetComponent<Projectile>();
                        projectile.Direction = new Vector3(directionX, directionY, directionZ);
                        projectile.Speed = speed;
                    }
                    else
                    {
                        Debug.Log("Not a bullet");
                    }

                    //Add reference of this object to our 'serverObjects'
                    serverObjects.Add(id, ni);
                }
            });

            //When the server unspawns the object...
            On("serverUnspawn", (Event) =>
            {
                //Extract Data from Event
                string id = Event.data["id"].ToString().RemoveQuotes();

                /** FIND AND REMOVE SERVER OBJECT
                 * > Find the NI matching the passed ID.
                 * > Remove the ref.
                 * > Destroy the object.
                 * */
                if (serverObjects[id] != null)
                {
                    NetworkIdentity ni = serverObjects[id];
                    serverObjects.Remove(id);
                    DestroyImmediate(ni.gameObject);
                }
                else Debug.LogError("Could not find reference of the NI in our serverObjects.  ID is false or we did not store it.");

            });

            //When any player dies...
            On("playerDied", (Event) =>
            {
                //Extract Data from Event
                string id = Event.data["id"].ToString().RemoveQuotes();

                /** FIND AND DISABLE PLAYER
                 * > Find NI matching the passed ID.
                 * > Disable that gameobject.
                 * */
                if (serverObjects[id] != null)
                {
                    NetworkIdentity ni = serverObjects[id];

                    ni.gameObject.SetActive(false);
                }
                else Debug.LogError("Could not find reference of the NI in our serverObjects.  ID is false or we did not store it.");
            });

            //When any player respawns...
            On("playerRespawn", (Event) =>
            {
                //Extract Data from Event
                string id = Event.data["id"].ToString().RemoveQuotes();
                float x = Event.data["position"]["x"].f;        //Spawn position
                float y = Event.data["position"]["y"].f;        //Spawn position
                float z = Event.data["position"]["z"].f;        //Spawn position


                /** FIND AND 'SPAWN' PLAYER
                 * > Find NI matching the passed ID.
                 * > Set position to the passed spawn position.
                 * > Enable that gameobject.
                 * */
                if (serverObjects[id] != null)
                {
                    NetworkIdentity ni = serverObjects[id];

                    ni.transform.position = new Vector3(x, y, z);

                    ni.gameObject.SetActive(true);
                }
                else Debug.LogError("Could not find reference of the NI in our serverObjects.  ID is false or we did not store it.");
            });

            //When a user reference is found from MongoDB and sent back...
            On("sendUserFromToken", (Event) =>
            {
                //Check if a user was found.
                if(Event.data["username"] != null)
                {
                    //Extract Data from Event
                    mAccessToken.username = Event.data["username"].str;

                    //Allow player to join the game.
                    joinGameUI.SetActive(true);
                    joinGameUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Join now, " + mAccessToken.username;

                    //Enable global chat.
                    globalChatUI.SetActive(true);

                    //Get the global chat messages.
                    Emit("getMessages");
                }
                //ELSE, a user was NOT found
                else
                {
                    Debug.LogWarning("Either we failed to fetch an existing user, or an error occured");

                    //Tell the player to wait.
                    pleaseWaitUI.SetActive(true);
                    TextMeshProUGUI pleaseWaitUIText = pleaseWaitUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

                    mTimeToWait *= 3;

                    StartCoroutine(waitForNextFetch(mTimeToWait, pleaseWaitUIText));
                }
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
                /** GET AND DISPLAY MESSAGES
                 * > Check if messages exist.
                 * > Set a maximum of 5 messages, with the newest messages displayed.
                 * > Set the text of our global chat.
                 * */
                if (Event.data["messages"] && Event.data["messages"].IsArray)
                {
                    globalChatDisplay.text = "";

                    if(Event.data["messages"].Count > 5)
                    {
                        mGlobalMessages = new string[5];

                        mGlobalMessages[0] = Event.data["messages"][Event.data["messages"].Count - 1].str;
                        mGlobalMessages[1] = Event.data["messages"][Event.data["messages"].Count - 2].str;

                        for(int i = 0; i < 5; i++)
                        {
                            mGlobalMessages[i] = Event.data["messages"][Event.data["messages"].Count - 5 + i].str;
                            globalChatDisplay.text += mGlobalMessages[i] += "\n";
                        }
                    }
                    else
                    {
                        mGlobalMessages = new string[Event.data["messages"].Count];

                        for (int i = 0; i < mGlobalMessages.Length; i++)
                        {
                            mGlobalMessages[i] = Event.data["messages"][1].str;

                            globalChatDisplay.text += mGlobalMessages[i] += "\n";
                        }
                    }
                }
            });
        }

        /** <summary> Waits for the specified duration to allow the user to send a new fetch.</summary>
         * <param name="pTimeToWait">The amount of time to wait before allowing a new fetch</param>
         * <param name="pleaseWaitUIText">The UI text component to be modified</param>
         * */
        private IEnumerator waitForNextFetch(int pTimeToWait, TextMeshProUGUI pleaseWaitUIText)
        {
            mAccessToken_LastRequest = mAccessToken.accessToken;
            Debug.Log("Lsat request : " + mAccessToken_LastRequest);

            for(int iTimeToWait = pTimeToWait; iTimeToWait > 0; iTimeToWait--)
            {
                pleaseWaitUIText.text = "Please wait " + (iTimeToWait - 1) + " seconds to send another Access Key";

                yield return new WaitForSeconds(1);
            }

            didSendFetch = false;

            pleaseWaitUI.SetActive(false);
        }

        /** <summary> Sets the initial references for this script</summary>
         * */
        private void SetInitialReferences()
        {
            mAccessToken = new AccessToken();

            mMyMessage = new Message();

            serverObjects = new Dictionary<string, NetworkIdentity>();
        }

        /// <summary>
        /// Attempt to join the lobby.
        /// </summary>
        public void AtemptToJoinLobby()
        {
            Emit("joinGame", new JSONObject(JsonUtility.ToJson(mAccessToken)));
        }

        /// <summary>
        /// Attempt to get user based on access token.
        /// </summary>
        public void FetchUserByToken()
        {
            Emit("fetchUserByToken", new JSONObject(JsonUtility.ToJson(mAccessToken)));
        }

        /// <summary>
        /// Send a message to the server.
        /// </summary>
        public void sendMessageToServer()
        {
            if(mMyMessage.message.Length > 0)
            {
                Emit("sendMessage", new JSONObject(JsonUtility.ToJson(mMyMessage)));
            }
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