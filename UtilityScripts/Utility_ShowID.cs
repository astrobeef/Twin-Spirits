using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Utility_ShowID : MonoBehaviour
{

    [SerializeField]
    Transform NetworkClient;
    Text mText;

    // Start is called before the first frame update
    void Start()
    {
        mText = GetComponent<Text>();

    }

    // Update is called once per frame
    void Update()
    {
        if(NetworkClient.childCount > 0 && NetworkClient.GetChild(0) != null)
        {
        Transform player1 = NetworkClient.GetChild(0);

        mText.text = player1.name;
        }

    }
}
