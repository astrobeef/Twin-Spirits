using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Player
{
    public class Player_Movement : MonoBehaviour
    {
        [SerializeField]
        private PlayerManager mMaster;

        [SerializeField]
        private float mSpeed = 2;

        public void SetInitialReferences()
        {
            if (GetComponent<PlayerManager>() != null)
            {
                mMaster = GetComponent<PlayerManager>();
            }
            else
            {
                Debug.Log("Missing essential script.  Deleting this");
                Destroy(this);
            }
        }

        public void runCheck()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 input = new Vector3(horizontal, 0, vertical);
            Vector3 movementForward = transform.forward * vertical;
            Vector3 movementSide = transform.right * horizontal;

            transform.position += (movementSide + movementForward) * mSpeed * Time.deltaTime;
        }
    }
}