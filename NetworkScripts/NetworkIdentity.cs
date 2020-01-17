using Project.Utility.Attributes;
using SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Project.Networking
{
    public class NetworkIdentity : MonoBehaviour

    {
        [Header("Helpful Values")]
        [SerializeField]
        [GreyOut]
        private string mID;
        [GreyOut]
        [SerializeField]
        private bool mIsControlling;

        private SocketIOComponent mSocket;

        // Start is called before the first frame update
        public void Awake()
        {
            mIsControlling = false;
        }

        // Update is called once per frame
        public void SetControllerID(string pID)
        {
            mID = pID;
            mIsControlling = (NetworkClient.ClientID == pID) ? true : false;     //Check incoming id against the one saved from the server.
        }

        public void SetSocketReference(SocketIOComponent pSocket)
        {
            mSocket = pSocket;
        }

        public string GetID()
        {
            return mID;
        }

        public bool IsControlling()
        {
            return mIsControlling;
        }

        public SocketIOComponent GetSocket()
        {
            return mSocket;
        }
    }
}