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
        private float mSpeed = 2;
        [SerializeField]
        private float mRotation = 60;

        [Header("Object References")]
        [SerializeField]
        private Transform barrelPivot;
        [SerializeField]
        private Transform bulletSpawnPoint;
        [SerializeField]
        private GameObject myCamerasObject;
        private Camera myCamera;

        [Header("Class References")]
        [SerializeField]
        private NetworkIdentity mNetworkIdentity;

        private float lastRotation;

        //Shooting
        private BulletData bulletData;
        private Cooldown shootingCooldown;

        //Rotation
        private float mRotSpeed = 360.0f;

        public void Start()
        {
            SetInitialReferences();
        }

        private void SetInitialReferences()
        {
            shootingCooldown = new Cooldown(1);
            bulletData = new BulletData();
            bulletData.position = new Position();
            bulletData.direction = new Position();

            if (mNetworkIdentity.IsControlling())
            {
                enableCameras();
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

        public float GetLastRotation()
        {
            return lastRotation;
        }

        public void SetRotation(float Value)
        {
            transform.rotation = Quaternion.Euler(0, Value, 0);
        }

        private void checkMovement()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 input = new Vector3(horizontal, 0, vertical);
            Vector3 movementForward = transform.forward * vertical;
            Vector3 movementSide = transform.right * horizontal;

            transform.position += (movementSide + movementForward) * mSpeed * Time.deltaTime;

            //transform.position += transform.forward * mSpeed * Time.deltaTime;
        }

        private void checkAiming()
        {
            Vector3 mousePosition = myCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 dif = transform.position - mousePosition;
            dif.Normalize();
            float rot = Mathf.Atan2(dif.x, dif.z) * Mathf.Rad2Deg;

            lastRotation = rot;

            barrelPivot.rotation = Quaternion.Euler(0, rot + BARREL_PIVOT_OFFSET, 0);
        }

        private void checkRotation()
        {
            transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0) * Time.deltaTime * mRotSpeed);
        }

        private void checkShooting()
        {
            shootingCooldown.CooldownUpdate();

            if(Input.GetMouseButton(0) && !shootingCooldown.IsOnCooldown())
            {
                shootingCooldown.StartCooldown();

                //Define Bullet
                bulletData.activator = NetworkClient.ClientID;
                bulletData.position.x = bulletSpawnPoint.position.x.TwoDecimals();
                bulletData.position.y = bulletSpawnPoint.position.y.TwoDecimals();
                bulletData.position.z = bulletSpawnPoint.position.z.TwoDecimals();

                bulletData.direction.x = bulletSpawnPoint.up.x;
                bulletData.direction.y = bulletSpawnPoint.up.y;
                bulletData.direction.z = bulletSpawnPoint.up.z;

                //Send Bullet
                mNetworkIdentity.GetSocket().Emit("fireBullet", new JSONObject(JsonUtility.ToJson(bulletData)));
            }
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
    }

}