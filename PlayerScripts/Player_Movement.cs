using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Player
{
    [RequireComponent(typeof(PlayerManager))]
    [RequireComponent(typeof(CharacterController))]
    public class Player_Movement : MonoBehaviour
    {
        [SerializeField]
        private PlayerManager mMaster;

        private CharacterController mController;

        [SerializeField]
        private float mSpeed = 2.0f;
        [SerializeField]
        private float mJumpSpeed = 8.0f;
        [SerializeField]
        private float mGravity = 20.0f;

        private Vector3 mMoveDirection = Vector3.zero;

        public void SetInitialReferences()
        {
            mMaster = GetComponent<PlayerManager>();

            mController = GetComponent<CharacterController>();
        }

        public void runCheck()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 movementForward = transform.forward * vertical;
            Vector3 movementSide = transform.right * horizontal;

            Vector3 movement = (movementSide + movementForward) * mSpeed;

            mMoveDirection = new Vector3(movement.x, mMoveDirection.y, movement.z);

            if (mController.isGrounded)
            {

                if (Input.GetButton("Jump"))
                {
                    Jump();
                }
            }

            mMoveDirection.y -= mGravity * Time.deltaTime;

            mController.Move(mMoveDirection * Time.deltaTime);
        }

        public void Jump()
        {
            Debug.Log("Jumping");

            mMoveDirection.y = mJumpSpeed;
        }
    }
}