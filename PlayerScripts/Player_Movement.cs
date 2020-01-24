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
        private float mSpeed_Initial = 2.0f, mSpeed;
        [SerializeField]
        private float mJumpSpeed = 8.0f;
        [SerializeField]
        private float mGravity = 20.0f;

        private Vector3 mMoveDirection = Vector3.zero;

        private bool mSpeedAltered = false;

        public void SetInitialReferences()
        {
            mMaster = GetComponent<PlayerManager>();

            mController = GetComponent<CharacterController>();

            mSpeed = mSpeed_Initial;
        }

        public void runCheck()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 movementForward = transform.forward * vertical;
            Vector3 movementSide = transform.right * horizontal;

            Vector3 movement = (movementSide + movementForward) * mSpeed;

            if(Mathf.Abs(horizontal) > 0.2f && Mathf.Abs(vertical) > 0.2f)
            {
                movement *= 0.72f;
            }

            mMoveDirection = new Vector3(movement.x, mMoveDirection.y, movement.z);

            if (mController.isGrounded)
            {
                if (Input.GetButton("Jump"))
                {
                    Jump(Mathf.Abs(horizontal), Mathf.Abs(vertical));
                }
            }

            mMoveDirection.y -= mGravity * Time.deltaTime;

            mController.Move(mMoveDirection * Time.deltaTime);
        }

        public void Jump(float hor, float vert)
        {
            if(hor > 0.2f || vert > 0.2f)
            {
                mMoveDirection.y = mJumpSpeed * 0.8f;

                if (!mSpeedAltered)
                {
                    StartCoroutine(temporaryAlterMoveSpeed(1.6f, 0.8f));
                }
            }
            else
            {
                mMoveDirection.y = mJumpSpeed * 1.4f;

                if (!mSpeedAltered)
                {
                    StartCoroutine(temporaryAlterMoveSpeed(0.7f, 0.8f));
                }
            }
        }

        IEnumerator temporaryAlterMoveSpeed(float pMultiplier, float pTime)
        {
            mSpeedAltered = true;

            mSpeed = mSpeed_Initial * pMultiplier;

            yield return new WaitForSeconds(pTime);

            mSpeed = mSpeed_Initial;

            mSpeedAltered = false;
        }
    }
}