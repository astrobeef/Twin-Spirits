//---NETWORK MESSAGING---//
//---------This script handles the network messaging events and related methods.


//-------------//
//---IMPORTS---//
//-------------//
using UnityEngine;
using System.Collections;
using TMPro;
using SocketIO;

namespace Project.Networking.Client
{

    public class NetworkMessaging : MonoBehaviour
    {
        private NetworkClient mMaster;

        [Header("UI")]
        private TextMeshProUGUI accessTokenInput;                //The input field for the user's access token.
        private TextMeshProUGUI messageInput;                    //The input field for the user's message.
        private TextMeshProUGUI globalChatDisplay;               //The text field of the global chat.
        private GameObject joinGameUI;                           //The gameobject which contains all 'join game' UI.
        private GameObject pleaseWaitUI;                         //The gameobject which contains all 'please wait' UI.
        private GameObject globalChatUI;                         //The gameobject which contains all 'global chat' UI.

        private bool didSendFetch;                              //Have we attempted to check our access token?
        private int mTimeToWait = 2;                            //How long to wait after a fetch.
        private string mAccessToken_LastRequest;                //The previous token requested.  Used to compare to our new token.

        private string[] mGlobalMessages;                        //The messages to be displayed to global chat.
        private Message mMyMessage;                              //The message constructed by the client user.

        /** <summary> Sets the initial references for this script</summary>
         * */
        public void SetIntialReferences()
        {
            mMaster = GetComponent<NetworkClient>();

            mMyMessage = new Message();

            //TextMeshProGUI Components
            if (GameObject.Find("AccessKey - Text") != null)
            {
                accessTokenInput = GameObject.Find("AccessKey - Text").GetComponent<TextMeshProUGUI>();
            }
            else Debug.LogError("Could not find Access Token Input");

            if (GameObject.Find("userMessage - Text") != null)
            {
                messageInput = GameObject.Find("userMessage - Text").GetComponent<TextMeshProUGUI>();
            }
            else Debug.LogError("Could not find userMessage Input");

            if (GameObject.Find("Global Chat - Text") != null)
            {
                globalChatDisplay = GameObject.Find("Global Chat - Text").GetComponent<TextMeshProUGUI>();
            }
            else Debug.LogError("Could not find globalChatDisplay");

            //UI GameObjects
            if (GameObject.Find("Join Now - Button") != null)
            {
                joinGameUI = GameObject.Find("Join Now - Button");
                joinGameUI.SetActive(false);
            }
            else Debug.LogError("Could not find Join Game Button");

            if (GameObject.Find("Please Wait - Field") != null)
            {
                pleaseWaitUI = GameObject.Find("Please Wait - Field");
                pleaseWaitUI.SetActive(false);
            }
            else Debug.LogError("Could not find Please wait field");

            if (GameObject.Find("[ Chat ]") != null)
            {
                globalChatUI = GameObject.Find("[ Chat ]");
                globalChatUI.SetActive(false);
            }
            else Debug.LogError("Could not find chat panel");

        }

        /** <summary> Runs any checks for the messaging.  Should be run on update.</summary>
         * */
        public void runCheck()
        {

            mMyMessage.message = messageInput.text.ToString();

            if (accessTokenInput.text.Length > 2)
            {
                mMaster.mAccessToken.accessToken = accessTokenInput.text.Trim(new char[] { '\r', '\n' });
                if (!didSendFetch && mAccessToken_LastRequest != mMaster.mAccessToken.accessToken)
                {
                    didSendFetch = true;
                    EmitFetchForUser();
                }
            }
            else
            {
                joinGameUI.SetActive(false);
            }
        }

        //-------------------------//
        //-----ON SOCKET EVENT-----//
        //-------------------------//

        /** <summary>Fetch a user based on an access token fetch</summary>
         * <param name="pEvent">The Event sent by the Server</param>
         * */
        public void OnSendUserFromToken(SocketIOEvent pEvent)
        {
            //Check if a user was found.
            if (pEvent.data["username"] != null)
            {
                //Extract Data from Event
                mMaster.mAccessToken.username = pEvent.data["username"].str;

                //Allow player to join the game.
                joinGameUI.SetActive(true);
                joinGameUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Join now, " + mMaster.mAccessToken.username;

                //Enable global chat.
                globalChatUI.SetActive(true);

                //Get the global chat messages.
                mMaster.Emit("getMessages");
            }
            //ELSE, a user was NOT found
            else
            {
                Debug.LogWarning("Either we failed to fetch an existing user, or an error occured");

                //Tell the player to wait.
                pleaseWaitUI.SetActive(true);
                TextMeshProUGUI pleaseWaitUIText = pleaseWaitUI.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

                mTimeToWait *= 3;

                //Begin waiting
                StartCoroutine(waitForNextFetch(mTimeToWait, pleaseWaitUIText));
            }
        }

        /** <summary>Gets the messages from the server and displays them</summary>
         * <param name="pEvent">The socket event fired from the server</param>
         * */
        public void OnReturnMessages(SocketIOEvent pEvent)
        {
            /** GET AND DISPLAY MESSAGES
             * > Check if messages exist.
             * > Set a maximum of 5 messages, with the newest messages displayed.
             * > Set the text of our global chat.
             * */
            if (pEvent.data["messages"] && pEvent.data["messages"].IsArray)
            {
                globalChatDisplay.text = "";

                if (pEvent.data["messages"].Count > 5)
                {
                    mGlobalMessages = new string[5];

                    mGlobalMessages[0] = pEvent.data["messages"][pEvent.data["messages"].Count - 1].str;
                    mGlobalMessages[1] = pEvent.data["messages"][pEvent.data["messages"].Count - 2].str;

                    for (int i = 0; i < 5; i++)
                    {
                        mGlobalMessages[i] = pEvent.data["messages"][pEvent.data["messages"].Count - 5 + i].str;
                        globalChatDisplay.text += mGlobalMessages[i] += "\n";
                    }
                }
                else
                {
                    mGlobalMessages = new string[pEvent.data["messages"].Count];

                    for (int i = 0; i < mGlobalMessages.Length; i++)
                    {
                        mGlobalMessages[i] = pEvent.data["messages"][1].str;

                        globalChatDisplay.text += mGlobalMessages[i] += "\n";
                    }
                }
            }
        }

        //---------------------------//
        //-----EMIT SOCKET EVENT-----//
        //---------------------------//

        /// <summary>
        /// Send a message to the server.
        /// </summary>
        public void EmitMessageToServer()
        {
            if (mMyMessage.message.Length > 0)
            {
                mMaster.Emit("sendMessage", new JSONObject(JsonUtility.ToJson(mMyMessage)));
            }
        }

        /// <summary>
        /// Attempt to get user based on access token.
        /// </summary>
        public void EmitFetchForUser()
        {
            mMaster.Emit("fetchUserByToken", new JSONObject(JsonUtility.ToJson(mMaster.mAccessToken)));
        }

        //-------------------------//
        //-----UTILITY METHODS-----//
        //-------------------------//


        /** <summary> Waits for the specified duration to allow the user to send a new fetch.</summary>
         * <param name="pTimeToWait">The amount of time to wait before allowing a new fetch</param>
         * <param name="pleaseWaitUIText">The UI text component to be modified</param>
         * */
        private IEnumerator waitForNextFetch(int pTimeToWait, TextMeshProUGUI pleaseWaitUIText)
        {
            mAccessToken_LastRequest = mMaster.mAccessToken.accessToken;
            Debug.Log("Lsat request : " + mAccessToken_LastRequest);

            for (int iTimeToWait = pTimeToWait; iTimeToWait > 0; iTimeToWait--)
            {
                pleaseWaitUIText.text = "Please wait " + (iTimeToWait - 1) + " seconds to send another Access Key";

                yield return new WaitForSeconds(1);
            }

            didSendFetch = false;

            pleaseWaitUI.SetActive(false);
        }
    }

}