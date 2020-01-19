//---NETWORK UPDATES---//
//---------This script handles updates between the Server and the client.

//-------------//
//---IMPORTS---//
//-------------//
using UnityEngine;
using System.Collections;
using SocketIO;
using Project.Utility;
using Project.Player;
using System.Collections.Generic;

namespace Project.Networking.Client
{
    public class NetworkUpdates : MonoBehaviour
    {
        private NetworkClient mMaster;

        /** <summary> Sets the initial references for this script</summary>
         * */
        public void SetInitialReferences()
        {
            mMaster = GetComponent<NetworkClient>();
        }

        //Put in updatePosition, updateRotation, playerDied, playerRespawn
        public void OnUpdatePosition(SocketIOEvent pEvent)
        {
            //Extract Data from Event
            string id = pEvent.data["id"].ToString().RemoveQuotes();
            float x = pEvent.data["position"]["x"].f;
            float y = pEvent.data["position"]["y"].f;
            float z = pEvent.data["position"]["z"].f;

            /** FIND AND UPDATE OBJECT
             * > Using the ID, find the matching <NetworkIdentity> from our 'serverObjects'
             * > Update the position of the found NI using our passed Data.
             * */
            if (mMaster.getServerObjects()[id] != null)
            {
                NetworkIdentity ni = mMaster.getServerObjects()[id];
                ni.transform.position = new Vector3(x, y, z);
            }
            else Debug.LogError("Could not find reference of the NI in our serverObjects.  ID is false or we did not store it.");
        }

        public void OnUpdateRotation(SocketIOEvent pEvent)
        {
            //Extract Data from Event
            string id = pEvent.data["id"].ToString().RemoveQuotes();
            float tankRotation = pEvent.data["tankRotation"].f;
            //float barrelRotation = pEvent.data["barrelRotation"].f;

            /** FIND AND UPDATE OBJECT
             * > Using the ID, find the matching <NetworkIdentity> from our 'serverObjects'
             * > Update the rotation of the found NI using our passed Data.
             * > Update the rotation of the actual prefab using our passed Data.
             * */
            if (mMaster.getServerObjects()[id] != null)
            {
                NetworkIdentity ni = mMaster.getServerObjects()[id];
                ni.transform.localEulerAngles = new Vector3(0, tankRotation, 0);        //Sets the network identity data.
                ni.GetComponent<PlayerManager>().getRotationScript().SetRotation(tankRotation);     //Sets the rotation of the actual player prefab.
            }
            else Debug.LogError("Could not find reference of the NI in our serverObjects.  ID is false or we did not store it.");
        }

        public void OnPlayerDied(SocketIOEvent pEvent)
        {
            //Extract Data from Event
            string id = pEvent.data["id"].ToString().RemoveQuotes();

            /** FIND AND DISABLE PLAYER
             * > Find NI matching the passed ID.
             * > Disable that gameobject.
             * */
            if (mMaster.getServerObjects()[id] != null)
            {
                NetworkIdentity ni = mMaster.getServerObjects()[id];

                ni.gameObject.SetActive(false);
            }
            else Debug.LogError("Could not find reference of the NI in our serverObjects.  ID is false or we did not store it.");
        }

        public void OnPlayerRespawn(SocketIOEvent pEvent)
        {
            //Extract Data from Event
            string id = pEvent.data["id"].ToString().RemoveQuotes();
            float x = pEvent.data["position"]["x"].f;        //Spawn position
            float y = pEvent.data["position"]["y"].f;        //Spawn position
            float z = pEvent.data["position"]["z"].f;        //Spawn position


            /** FIND AND 'SPAWN' PLAYER
             * > Find NI matching the passed ID.
             * > Set position to the passed spawn position.
             * > Enable that gameobject.
             * */
            if (mMaster.getServerObjects()[id] != null)
            {
                NetworkIdentity ni = mMaster.getServerObjects()[id];

                ni.transform.position = new Vector3(x, y, z);

                ni.gameObject.SetActive(true);
            }
            else Debug.LogError("Could not find reference of the NI in our serverObjects.  ID is false or we did not store it.");
        }
    }
}
