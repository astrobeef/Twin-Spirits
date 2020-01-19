using Project.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Gameplay
{
public class CollisionDestroy : MonoBehaviour
{
        [SerializeField]
        private NetworkIdentity networkIdentity;
        [SerializeField]
        private WhoActivatedMe whoActivatedMe;

        public void OnCollisionEnter(Collision collision)
        {
            NetworkIdentity ni = collision.gameObject.GetComponent<NetworkIdentity>();

            if(ni == null || ni.GetID() != whoActivatedMe.GetActivator())
            {
                if(networkIdentity.GetSocket() != null)
                {
                    networkIdentity.GetSocket().Emit("collisionDestroy", new JSONObject(JsonUtility.ToJson(new IDData()
                    {
                        id = networkIdentity.GetID()
                    })));
                }
                else
                {
                    Debug.LogWarning("Failed to get socket.  We are offline or there is an error");
                    Debug.LogWarning("We are not damaging anything.  We are just destroying the bullet");
                    Destroy(gameObject);
                }
            }
        }
    }
}
