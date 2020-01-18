using Project.Networking;
using Project.Utility;
using Project.Scriptable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Gameplay;

namespace Project.Player
{
    public class Player_Shooting : MonoBehaviour
    {
        [SerializeField]
        private PlayerManager mMaster;

        [SerializeField]
        private Transform bulletSpawnPoint;

        //Shooting
        private BulletData bulletData;
        private Cooldown shootingCooldown;

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

            shootingCooldown = new Cooldown(1);
            bulletData = new BulletData();
            bulletData.position = new Position();
            bulletData.direction = new Position();
        }

        public void runCheck()
        {
            shootingCooldown.CooldownUpdate();

            if (Input.GetMouseButton(0) && !shootingCooldown.IsOnCooldown())
            {
                fireBullet();
            }
        }

        public void fireBullet()
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
            
            //If we are connected to the server...
            if (mMaster.ourClientIsConnected)
            {
                mMaster.getNetworkIdentity().GetSocket().Emit("fireBullet", new JSONObject(JsonUtility.ToJson(bulletData)));
            }
            else
            {
                float speed = 0.5f;

                ServerObjectData sod = mMaster.OfflineSpawnables.GetObjectByName("Bullet");
                var spawnedObject = Instantiate(sod.Prefab, mMaster.OfflineContainer);
                spawnedObject.transform.position = new Vector3(bulletData.position.x, bulletData.position.y, bulletData.position.z);

                //Set parameters so that this bullet does not hit its creator.
                WhoActivatedMe whoActivatedMe = spawnedObject.GetComponent<WhoActivatedMe>();
                whoActivatedMe.SetActivator(transform.name);

                //Set the direction and speed of the projectile.
                Projectile projectile = spawnedObject.GetComponent<Projectile>();
                projectile.Direction = transform.forward;
                projectile.Speed = speed;
            }
        }
    }
}
