//---NETWORK MIGRATION---//
//---------Handles connections coming in and out, as well as spawning/unspawning.

//-------------//
//---IMPORTS---//
//-------------//
using UnityEngine;
using System.Collections;
using SocketIO;
using Project.Utility;

namespace Project.Networking.Client
{
    public class NetworkMigration : MonoBehaviour
    {
        private NetworkClient mMaster;

        [SerializeField]
        private GameObject playerPrefab;                        //The prefab of our player to be instantiated.

        //Include on : open, register, spawn, unspawn, disconnected

        /** <summary> Sets the initial references for this script</summary>
         * */
        public void SetInitialReferences()
        {
            mMaster = GetComponent<NetworkClient>();
        }

        public void OnOpen(SocketIOEvent pEvent)
        {
            Debug.Log("Connection made to the server");
        }

        public void OnRegister(SocketIOEvent pEvent)
        {
            mMaster.setClientID(pEvent.data["id"].ToString().RemoveQuotes());
            Debug.LogFormat("Our Client's ID ({0})", mMaster.getClientID());
        }

        public void OnSpawn(SocketIOEvent pEvent, NetworkClient ourClient)
        {
            //Extract Data from Event
            string id = pEvent.data["id"].ToString().RemoveQuotes();     //Get our ID from the data passed in.

            /** SPAWN OUR PLAYER
             * > Create the object as a child of our "networkContainer"
             * > Set the name of our player.
             * > Get the <NetworkIdentity> component off of the instantiated player, 'go'.
             * > Set the controller ID and socket reference of the NetworkIdentity on the player.
             * */
            GameObject go = Instantiate(playerPrefab, mMaster.getNetworkContainer());
            go.name = string.Format("Player ({0})", mMaster.mAccessToken.username);
            NetworkIdentity ni = go.GetComponent<NetworkIdentity>();
            ni.SetControllerID(id);
            ni.SetSocketReference(ourClient);

            //Store reference to this instantiated object on this 'NetworkClient'.
            mMaster.getServerObjects().Add(id, ni);
        }

        public void OnDisconnected(SocketIOEvent pEvent)
        {
            //Extract Data from Event
            string id = pEvent.data["id"].ToString().RemoveQuotes();     //Get our ID from the data passed in.

            /** FIND AND DESTROY PLAYER
             * > Using the ID, find the match in our Dictionary of 'serverObjects'.
             * > Destroy the found gameObject.
             * > Remove reference to the gameObject from our Dictionary.
             * */
            if (mMaster.getServerObjects()[id] != null)
            {
                GameObject go = mMaster.getServerObjects()[id].gameObject;
                Destroy(go);        //Remove from game
                mMaster.getServerObjects().Remove(id);       //Remove from memory
                Debug.Log("Player, " + id + ", disconnected");
            }
            else Debug.LogError("Could not find referenced ID in our serverObjects.  ID is false or we did not store it.");
        }
    }
}