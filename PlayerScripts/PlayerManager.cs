using Project.Networking;
using Project.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Player
{

    public class PlayerManager : MonoBehaviour
    {
        const float BARREL_PIVOT_OFFSET = 90.0f;

        [Header("Data")]
        [SerializeField]
        private float mRotation = 60;

        [Header("Object References")]
        [SerializeField]
        private GameObject myCamerasObject;
        private Camera myCamera;

        [Header("Class References")]
        [SerializeField]
        private NetworkIdentity mNetworkIdentity;
        [SerializeField]
        private Player_Movement mMovementScript;
        [SerializeField]
        private Player_Shooting mShootingScript;
        [SerializeField]
        private Player_Rotation mRotationScript;

        public void Start()
        {
            SetInitialReferences();
        }

        private void SetInitialReferences()
        {
            if (mNetworkIdentity.IsControlling())
            {
                enableCameras();
            }

            if(mMovementScript == null)
            {
                if(GetComponent<Player_Movement>() != null)
                {
                    mMovementScript = GetComponent<Player_Movement>();
                    mMovementScript.SetInitialReferences();
                }
                else
                {
                    Debug.LogError("Missing essential script");
                }
            }

            if(mShootingScript == null)
            {
                if(GetComponent<Player_Shooting>() != null)
                {
                    mShootingScript = GetComponent<Player_Shooting>();
                    mShootingScript.SetInitialReferences();
                }
                else
                {
                    Debug.LogError("Missing essential script");
                }
            }

            if(mRotationScript == null)
            {
                if(GetComponent<Player_Rotation>() != null)
                {
                    mRotationScript = GetComponent<Player_Rotation>();
                    mRotationScript.SetInitialReferences();
                }
                else
                {
                    Debug.LogError("Missing essential script");
                }
            }
        }

        void Update()
        {
            if (mNetworkIdentity.IsControlling())
            {
                checkMovement();
                //checkAiming();
                checkShooting();
                checkRotation();
            }
        }

        private void checkMovement()
        {
            mMovementScript.runCheck();
        }

        //private void checkAiming()
        //{
        //    Vector3 mousePosition = myCamera.ScreenToWorldPoint(Input.mousePosition);
        //    Vector3 dif = transform.position - mousePosition;
        //    dif.Normalize();
        //    float rot = Mathf.Atan2(dif.x, dif.z) * Mathf.Rad2Deg;

        //    lastRotation = rot;
        //}

        private void checkRotation()
        {
            mRotationScript.runCheck();
        }

        private void checkShooting()
        {
            mShootingScript.runCheck();
        }

        private void enableCameras()
        {
            myCamerasObject = transform.Find("Cameras").gameObject;
            myCamerasObject.SetActive(true);

            myCamera = myCamerasObject.transform.Find("Render Camera").GetComponent<Camera>();

            if(myCamera.transform.GetComponent<AudioListener>() != null)
            {
                myCamera.transform.GetComponent<AudioListener>().enabled = true;
            }

        }

        /*--------------------------*/
        /*-----ACCESSOR METHODS-----*/
        /*--------------------------*/

        public NetworkIdentity getNetworkIdentity()
        {
            return mNetworkIdentity;
        }

        public Player_Rotation getRotationScript()
        {
            return mRotationScript;
        }
    }

}