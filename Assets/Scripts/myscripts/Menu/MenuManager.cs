using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject main;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void test()
    {
        print(1);
    }

    public void TapToStartPressed()
    {
        transform.Find("TapToStart").gameObject.SetActive(false);
        main.SetActive(true);
    }
}
