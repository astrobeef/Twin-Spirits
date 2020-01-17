using Project.Networking;
using Project.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                mMaster.getNetworkIdentity().GetSocket().Emit("fireBullet", new JSONObject(JsonUtility.ToJson(bulletData)));
            }
        }
    }
}
