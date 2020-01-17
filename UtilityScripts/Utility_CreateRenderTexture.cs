using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility_CreateRenderTexture : MonoBehaviour
{

    public RenderTexture rt;

    public Camera renderCamera;
    public Camera displayCamera;

    public Material displayMaterial;

    public float aliasMod = 0.24f;

    // Start is called before the first frame update
    void Start()
    {
        SetIntialReferences();

        rt = new RenderTexture(Mathf.RoundToInt(1920 * aliasMod), Mathf.RoundToInt(1080 * aliasMod), 16, RenderTextureFormat.ARGB32);
        rt.Create();

        renderCamera.targetTexture = rt;

        displayMaterial.mainTexture = rt;

        displayCamera.gameObject.SetActive(true);
    }

    void SetIntialReferences()
    {
        if(displayCamera == null)
        {
            displayCamera = GameObject.Find("Display Camera").GetComponent<Camera>();
        }
    }
}
