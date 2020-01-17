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

namespace Project.Networking {
    public class NetworkClient : SocketIOComponent
    {
        public const float SERVER_UPDATE_TIME = 10; //10% of the server update (currently 100 ms)

        [Header("Network Client")]
        [SerializeField]
        private Transform networkContainer;
        [SerializeField]
        private GameObject playerPrefab;
        [SerializeField]
        private ServerObjects serverSpawnables;

        public static string ClientID { get; private set; }

        public TextMeshProUGUI usernameInput;
        public TextMeshProUGUI messageInput;
        public TextMeshProUGUI globalChatDisplay;
        public GameObject joinGameUI;
        public GameObject pleaseWaitUI;
        public GameObject globalChatUI;
        public AccessToken mAccessToken;

        private bool didSendFetch;
        private int mTimeToWait = 2;
        [SerializeField]
        private string mAccessToken_LastRequest;

        public string[] mGlobalMessages;
        public Message mMyMessage;

        private Dictionary<string, NetworkIdentity> serverObjects;
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

            if (usernameInput.text.Length > 2)
            {
                mAccessToken.accessToken = usernameInput.text.Trim(new char[] { '\r', '\n'});
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

        private void SetupEvents()
        {
            On("open", (Event) =>
            {
                Debug.Log("Connection made to the server");
            });

            On("register", (Event) =>
            {
                ClientID = Event.data["id"].ToString().RemoveQuotes();
                Debug.LogFormat("Our Client's ID ({0})", ClientID);
            });

            On("spawn", (Event) =>
            {

                Emit("getMessages");

                Debug.Log("Player spawned");
                string id = Event.data["id"].ToString().RemoveQuotes();

                GameObject go = Instantiate(playerPrefab, networkContainer);
                go.name = string.Format("Player ({0})", mAccessToken.username);
                NetworkIdentity ni = go.GetComponent<NetworkIdentity>();
                ni.SetControllerID(id);
                ni.SetSocketReference(this);

                serverObjects.Add(id, ni);
            });

            On("disconnected", (Event) =>
            {
                string id = Event.data["id"].ToString().RemoveQuotes();

                GameObject go = serverObjects[id].gameObject;
                Destroy(go);        //Remove from game
                serverObjects.Remove(id);       //Remove from memory
                Debug.Log("Player, " + id + ", disconnected");
            });

            On("updatePosition", (Event) => {
                string id = Event.data["id"].ToString().RemoveQuotes();
                float x = Event.data["position"]["x"].f;
                float y = Event.data["position"]["y"].f;
                float z = Event.data["position"]["z"].f;

                NetworkIdentity ni = serverObjects[id];
                ni.transform.position = new Vector3(x, y, z);
            });

            On("updateRotation", (Event) =>
            {
                string id = Event.data["id"].ToString().RemoveQuotes();
                float tankRotation = Event.data["tankRotation"].f;
                float barrelRotation = Event.data["barrelRotation"].f;

                NetworkIdentity ni = serverObjects[id];
                Debug.Log(ni.name);
                Debug.Log(ni.transform.localEulerAngles);
                Debug.Log("ni.transform.localEulerAngles");
                ni.transform.localEulerAngles = new Vector3(0, tankRotation, 0);        //Sets the network identity data.
                //ni.GetComponent<PlayerManager>().SetRotation(barrelRotation);
                ni.GetComponent<PlayerManager>().SetRotation(tankRotation);     //Sets the rotation of the actual player prefab.
            });

            On("serverSpawn", (Event) =>
            {
                string name = Event.data["name"].str;
                string id = Event.data["id"].ToString().RemoveQuotes();
                float x = Event.data["position"]["x"].f;
                float y = Event.data["position"]["y"].f;
                float z = Event.data["position"]["z"].f;

                if (!serverObjects.ContainsKey(id))
                {
                    ServerObjectData sod = serverSpawnables.GetObjectByName(name);
                    var spawnedObject = Instantiate(sod.Prefab, networkContainer);
                    spawnedObject.transform.position = new Vector3(x, y, z);
                    var ni = spawnedObject.GetComponent<NetworkIdentity>();
                    ni.SetControllerID(id);
                    ni.SetSocketReference(this);

                    //If bullet apply direction as well
                    if(name == "Bullet")
                    {
                        float directionX = Event.data["direction"]["x"].f;
                        float directionY = Event.data["direction"]["y"].f;
                        float directionZ = Event.data["direction"]["z"].f;
                        string activator = Event.data["activator"].ToString().RemoveQuotes();
                        float speed = Event.data["speed"].f;

                        float rot = Mathf.Atan2(directionX, directionZ) * Mathf.Rad2Deg;
                        Vector3 currentRotation = new Vector3(0, rot - 90, 0);
                        spawnedObject.transform.rotation = Quaternion.Euler(currentRotation);

                        WhoActivatedMe whoActivatedMe = spawnedObject.GetComponent<WhoActivatedMe>();
                        whoActivatedMe.SetActivator(activator);

                        Projectile projectile = spawnedObject.GetComponent<Projectile>();
                        projectile.Direction = new Vector3(directionX, directionY, directionZ);
                        projectile.Speed = speed;
                    }
                    else
                    {
                        Debug.Log("Not a bullet");
                    }

                    serverObjects.Add(id, ni);
                }
            });

            On("serverUnspawn", (Event) =>
            {
                string id = Event.data["id"].ToString().RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];
                serverObjects.Remove(id);
                DestroyImmediate(ni.gameObject);
            });

            On("playerDied", (Event) =>
            {
                string id = Event.data["id"].ToString().RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];

                ni.gameObject.SetActive(false);
            });

            On("playerRespawn", (Event) =>
            {
                string id = Event.data["id"].ToString().RemoveQuotes();
                float x = Event.data["position"]["x"].f;
                float y = Event.data["position"]["y"].f;
                float z = Event.data["position"]["z"].f;
                NetworkIdentity ni = serverObjects[id];

                ni.transform.position = new Vector3(x, y, z);

                ni.gameObject.SetActive(true);
            });

            On("sendUserFromToken", (Event) =>
            {
                if(Event.data["username"] != null)
                {
                    mAccessToken.username = Event.data["username"].str;

                    joinGameUI.SetActive(true);
                    joinGameUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Join now, " + mAccessToken.username;

                    globalChatUI.SetActive(true);

                    Emit("getMessages");
                }
                else
                {
                    Debug.LogWarning("Either we failed to fetch an existing user, or an error occured");

                    pleaseWaitUI.SetActive(true);
                    TextMeshProUGUI pleaseWaitUIText = pleaseWaitUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

                    mTimeToWait *= 3;

                    StartCoroutine(waitForNextFetch(mTimeToWait, pleaseWaitUIText));
                }

                Debug.Log("We have recieved the event to get user from access token");
            });

            On("newMessage", (Event) =>
            {
                Debug.Log("A user has sent a message");

                Emit("getMessages");
            });

            On("returnMessages", (Event) =>
            {
                Debug.Log("We have gotten our messages from our MongoDB");

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

        private void SetInitialReferences()
        {
            mAccessToken = new AccessToken();

            mMyMessage = new Message();

            serverObjects = new Dictionary<string, NetworkIdentity>();
        }

        public void AtemptToJoinLobby()
        {
            Emit("joinGame", new JSONObject(JsonUtility.ToJson(mAccessToken)));
        }

        public void FetchUserByToken()
        {
            Debug.Log("Attempting to fetch Access Token");

            Emit("fetchUserByToken", new JSONObject(JsonUtility.ToJson(mAccessToken)));
        }

        public void SendMessageToDB()
        {
            if(mMyMessage.message.Length > 0)
            {
                Emit("sendMessage", new JSONObject(JsonUtility.ToJson(mMyMessage)));
            }
        }

    };

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
        public float barrelRotation;
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