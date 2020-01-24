using UnityEngine;
using System.Collections;


namespace Project.Player
{
    [RequireComponent(typeof(PlayerManager))]
    public class Player_Camera : MonoBehaviour
    {
        private PlayerManager mMaster;

        private float mRotSpeed = 180.0f;

        private float mRot_Delta, mRot_Limit = 30, mRot_Initial;

        private Camera mCamera;

        private Transform mCamerasObject;

        public void SetInitialReferences()
        {
            mMaster = GetComponent<PlayerManager>();

            mCamerasObject = transform.Find("Cameras").transform;

            mRot_Initial = mCamerasObject.transform.localRotation.x;
        }

        public void runCheck()
        {
            float mCamRotation = -1f * Input.GetAxis("Mouse Y");

            //-.35 to .45
            if(mRot_Delta > -0.35f && mCamRotation > 0)
            {
                mCamerasObject.transform.Rotate(new Vector3(mCamRotation,0,0) * Time.deltaTime * mRotSpeed);
            }
            else if (mRot_Delta < 0.45f && mCamRotation < 0)
            {
                mCamerasObject.transform.Rotate(new Vector3(mCamRotation,0,0) * Time.deltaTime * mRotSpeed);
            }

            mRot_Delta = mRot_Initial - mCamerasObject.transform.localRotation.x;
        }
    }
}