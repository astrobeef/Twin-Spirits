using UnityEngine;
using System.Collections;


namespace Project.Player
{
    [RequireComponent(typeof(PlayerManager))]
    public class Player_Camera : MonoBehaviour
    {
        private PlayerManager mMaster;

        private float mRotSpeed = 360.0f;

        private float mRot;

        private Camera mCamera;

        private Transform mCamerasObject;

        public void SetInitialReferences()
        {
            mMaster = GetComponent<PlayerManager>();

            mCamerasObject = transform.Find("Cameras").transform;
        }

        public void runCheck()
        {
            mCamerasObject.transform.Rotate(new Vector3(Input.GetAxis("Mouse Y"), 0, 0) * Time.deltaTime * mRotSpeed);
        }
    }
}