using UnityEngine;
using System.Collections;

namespace Project.Player
{
    public class Player_Rotation : MonoBehaviour
    {
        //Rotation
        private float mRotSpeed = 360.0f;

        public void SetInitialReferences()
        {
        }

        public void runCheck()
        {
            transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0) * Time.deltaTime * mRotSpeed);
        }

        public void SetRotation(float Value)
        {
            transform.rotation = Quaternion.Euler(0, Value, 0);
        }
    }
}