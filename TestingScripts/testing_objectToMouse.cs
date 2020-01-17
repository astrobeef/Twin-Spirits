using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testing_objectToMouse : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        transform.position = new Vector3(mousePosition.x, transform.position.y, mousePosition.z);
    }
}
