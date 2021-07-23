using Michsky.UI.ModernUIPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Calibrate : MonoBehaviour
{
    GameObject go;
    int currentPos = 0;
    KeyStrokeMaker.PianoKeys currentKey;
    float whiteKeyWidth = 2.1f;
    float whiteKeyHeight = 15;
    float blackKeyWidth = .9f;
    float blackKeyHeight = 9.5f;

    public RadialSlider x, y, z;

    private void OnEnable()
    {
        if (!PlayerPrefs.HasKey("amountOfKeys"))
            PlayerPrefs.SetInt("amountOfKeys", 88);
        if (!PlayerPrefs.HasKey("startKey"))
            PlayerPrefs.SetInt("startKey", 0);
    }

    private void Start()
    {
        SetSliderValues();
        SpawnPiano(transform);
    }


    private void Update()
    {
        transform.rotation = Quaternion.Euler(x.currentValue, y.currentValue, z.currentValue);
    }

    private void SetSliderValues()
    {
        if (PlayerPrefs.HasKey("XRot"))
        {
            x.currentValue = PlayerPrefs.GetFloat("XRot");
            y.currentValue = PlayerPrefs.GetFloat("YRot");
            z.currentValue = PlayerPrefs.GetFloat("ZRot");
        }
    }

    private void SpawnPiano(Transform parent)
    {
        for (int i = 0; i < PlayerPrefs.GetInt("amountOfKeys"); i++)
        {
            if (currentKey.ToString().Length == 2)
            {
                go = Instantiate(Resources.Load(Path.Combine("Prefabs", "BlackOriginal")) as GameObject, transform);
                go.tag = "Key";
                go.transform.localPosition = new Vector3(0, 0, ((currentPos + 1) * 10 * whiteKeyWidth - blackKeyWidth * 5) * -1);
                Vector3 SizeScale = go.transform.Find("Plane").localScale;
                SizeScale.x = blackKeyHeight;
                SizeScale.z = blackKeyWidth;
                go.transform.Find("Plane").localScale = SizeScale;

                Transform Letter = go.transform.Find("Letter");
                Letter.localScale = new Vector3(SizeScale.z, SizeScale.z, SizeScale.z) * 1.5f;
                Vector3 Pos = Letter.localPosition;
                Pos.x = -5 * SizeScale.x;
                Letter.localPosition = Pos;
                Letter.GetComponent<TextMeshPro>().text = currentKey.ToString();
            }
            else
            {
                go = Instantiate(Resources.Load(Path.Combine("Prefabs", "WhiteOriginal")) as GameObject, transform);
                go.tag = "Key";
                go.transform.localPosition = new Vector3(0, 0, (currentPos * 10 * whiteKeyWidth) * -1);
                Vector3 SizeScale = go.transform.Find("Plane").localScale;
                SizeScale.x = whiteKeyHeight;
                SizeScale.z = whiteKeyWidth;
                go.transform.Find("Plane").localScale = SizeScale;

                Transform Letter = go.transform.Find("Letter");
                Letter.localScale = new Vector3(SizeScale.z, SizeScale.z, SizeScale.z);
                Vector3 Pos = Letter.localPosition;
                Pos.x = -5 * SizeScale.x;
                Letter.localPosition = Pos;
                Letter.GetComponent<TextMeshPro>().text = currentKey.ToString();
            }
            currentKey++;
            if (currentKey == KeyStrokeMaker.PianoKeys.NUL)
                currentKey = 0;

            if (currentKey.ToString().Length != 2)
                currentPos++;

            transform.position = new Vector3(-PlayerPrefs.GetInt("amountOfKeys") / 2, 0, 0);
        }
        LoadCalibration(transform);
    }

    public void MovePiano(string direction)
    {
        switch (direction)
        {
            case "Up":
                transform.localPosition += Vector3.up * -.5f;
                break;
            case "Down":
                transform.localPosition += Vector3.up * .5f;
                break;
            case "Left":
                transform.localPosition += Vector3.right * -.5f;
                break;
            case "Right":
                transform.localPosition += Vector3.right * .5f;
                break;
            case "High":
                transform.localPosition += Vector3.forward * .5f;
                break;
            case "Low":
                transform.localPosition += Vector3.forward * -.5f;
                break;
        }
    }

    /// <summary>
    /// Saves the calibration data
    /// </summary>
    public void SaveCalibration()
    {
        PlayerPrefs.SetFloat("XPos", transform.localPosition.x);
        PlayerPrefs.SetFloat("YPos", transform.localPosition.y);
        PlayerPrefs.SetFloat("ZPos", transform.localPosition.z);
        PlayerPrefs.SetFloat("XRot", transform.eulerAngles.x);
        PlayerPrefs.SetFloat("YRot", transform.eulerAngles.y);
        PlayerPrefs.SetFloat("ZRot", transform.eulerAngles.z);
        SceneManager.LoadScene("Menu");
    }

    /// <summary>
    /// Load the calibration data
    /// </summary>
    public static void LoadCalibration(Transform t)
    {
        Vector3 v = Vector3.zero;
        Vector3 q = Vector3.zero;
        if (PlayerPrefs.HasKey("XPos"))
        {
            v.x = PlayerPrefs.GetFloat("XPos");
            v.y = PlayerPrefs.GetFloat("YPos");
            v.z = PlayerPrefs.GetFloat("ZPos");
            q.x = PlayerPrefs.GetFloat("XRot");
            q.y = PlayerPrefs.GetFloat("YRot");
            q.z = PlayerPrefs.GetFloat("ZRot");
        }
        t.localPosition = v;
        t.eulerAngles = q;
    }
}
