using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadTextureToStatic : MonoBehaviour
{
    public static Texture T;

    // Start is called before the first frame update
    void OnEnable()
    {
        transform.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", T);
    }

    public void GiveTexture(Texture t)
    {
        T = t;
        transform.GetComponentInChildren<Renderer>().material.SetTexture("_MainTex", T);
    }
}
