using Project.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Project.Gameplay
{
    public class Projectile : MonoBehaviour
    {
        private Vector3 direction;
        private float speed;

        public Vector3 Direction
        {
            set
            {
                direction = value;
            }
        }

        public float Speed
        {
            set
            {
                speed = value;
            }
        }

        void Update()
        {
            Vector3 pos = direction * speed * NetworkClient.SERVER_UPDATE_TIME * Time.deltaTime;
            transform.position += new Vector3(pos.x, pos.y, pos.z);
        }
    }
}
