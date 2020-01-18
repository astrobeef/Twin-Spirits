//---NETWORK MIGRATION---//
//---------Handles server spawned objects.

//-------------//
//---IMPORTS---//
//-------------//
using UnityEngine;
using System.Collections;
using SocketIO;
using Project.Utility;
using Project.Scriptable;
using Project.Gameplay;

namespace Project.Networking.Client
{
    public class NetworkServerSpawning : MonoBehaviour
    {
        private NetworkClient mMaster;

        [SerializeField]
        private ServerObjects serverSpawnables;                 //Objects spawned by the server which are always identical.  (Bullets, but not players).

        public void SetInitialReferences()
        {
            mMaster = GetComponent<NetworkClient>();
        }

        /** <summary>Spawns an object from the server</summary>
         * <param name="pEvent">The socket event fired from the server</param>
         * <param name="ourClient">The socket reference of our NetworkClient</param>
         * */
        public void OnServerSpawn(SocketIOEvent pEvent, NetworkClient ourClient)
        {
            //Extract Data from Event
            string name = pEvent.data["name"].str;
            string id = pEvent.data["id"].ToString().RemoveQuotes();
            float x = pEvent.data["position"]["x"].f;
            float y = pEvent.data["position"]["y"].f;
            float z = pEvent.data["position"]["z"].f;

            /** SPAWN SERVER OBJECT
             * > Check if we have a reference to that object.
             * > Find the object we are spawning from our 'serverSpawnables' class.
             * > Set the position of the object to the server's passed position.
             * > Get the <NetworkIdentity> of the spawned object.
             * > Set reference on that NI for its ID and Socket Reference.
             * */
            if (!mMaster.getServerObjects().ContainsKey(id))
            {
                ServerObjectData sod = serverSpawnables.GetObjectByName(name);      //Find the server object by the name of the object.
                var spawnedObject = Instantiate(sod.Prefab, mMaster.getNetworkContainer());      //Instantiate the object's prefab as a child of our 'networkContainer'
                spawnedObject.transform.position = new Vector3(x, y, z);
                var ni = spawnedObject.GetComponent<NetworkIdentity>();
                ni.SetControllerID(id);
                ni.SetSocketReference(ourClient);

                //If bullet apply direction as well
                if (name == "Bullet")
                {
                    handleBulletSpawn(pEvent, spawnedObject);
                }

                //Add reference of this object to our 'serverObjects'
                mMaster.getServerObjects().Add(id, ni);
            }
        }

        public void OnServerUnspawn(SocketIOEvent pEvent)
        {
            //Extract Data from Event
            string id = pEvent.data["id"].ToString().RemoveQuotes();

            /** FIND AND REMOVE SERVER OBJECT
             * > Find the NI matching the passed ID.
             * > Remove the ref.
             * > Destroy the object.
             * */
            if (mMaster.getServerObjects()[id] != null)
            {
                NetworkIdentity ni = mMaster.getServerObjects()[id];
                mMaster.getServerObjects().Remove(id);
                DestroyImmediate(ni.gameObject);
            }
            else Debug.LogError("Could not find reference of the NI in our serverObjects.  ID is false or we did not store it.");

        }

        /*---------------------*/
        /*---Handler Methods---*/
        /*---------------------*/

        /** <summary>Performs additional functionality for any spawned bullet object.</summary>
         * <param name="pEvent">The socket event fired from the server</param>
         * <param name="pSpawnedObject">The bullet we have spawned</param>
         * */
        public void handleBulletSpawn(SocketIOEvent pEvent, GameObject pSpawnedObject)
        {
            //Extract Data from Event
            float directionX = pEvent.data["direction"]["x"].f;
            float directionY = pEvent.data["direction"]["y"].f;
            float directionZ = pEvent.data["direction"]["z"].f;
            string activator = pEvent.data["activator"].ToString().RemoveQuotes();
            float speed = pEvent.data["speed"].f;

            //Calculate and apply directional data.
            float rot = Mathf.Atan2(directionX, directionZ) * Mathf.Rad2Deg;
            Vector3 currentRotation = new Vector3(0, rot - 90, 0);
            pSpawnedObject.transform.rotation = Quaternion.Euler(currentRotation);

            //Set parameters so that this bullet does not hit its creator.
            WhoActivatedMe whoActivatedMe = pSpawnedObject.GetComponent<WhoActivatedMe>();
            whoActivatedMe.SetActivator(activator);

            //Set the direction and speed of the projectile.
            Projectile projectile = pSpawnedObject.GetComponent<Projectile>();
            projectile.Direction = new Vector3(directionX, directionY, directionZ);
            projectile.Speed = speed;
        }
    }
}