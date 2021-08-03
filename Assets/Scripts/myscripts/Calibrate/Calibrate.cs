using Michsky.UI.ModernUIPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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

    public GameObject piano;
    public Button xUp, xDown, yUp, yDown, zUp, zDown;
    public TextMeshProUGUI xVal, yVal, zVal;
    public Text pos, rot, scale;

    private void OnEnable()
    {
        if (!PlayerPrefs.HasKey("amountOfKeys"))
            PlayerPrefs.SetInt("amountOfKeys", 88);
        if (!PlayerPrefs.HasKey("startKey"))
            PlayerPrefs.SetInt("startKey", 0);
    }

    private void Start()
    {
        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return new WaitUntil(() => GameObject.FindGameObjectWithTag("Piano") != null);
        piano = GameObject.FindGameObjectWithTag("Piano");
        SetTextValues();
        SpawnKeys(piano.transform.GetChild(0));
        SetTriggers();
    }

    private void SetTriggers()
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();

        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener(delegate { StartCoroutine(RotXUp()); });
        xUp.GetComponent<EventTrigger>().triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener(delegate { StartCoroutine(RotXDown()); });
        xDown.GetComponent<EventTrigger>().triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener(delegate { StartCoroutine(RotYUp()); });
        yUp.GetComponent<EventTrigger>().triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener(delegate { StartCoroutine(RotYDown()); });
        yDown.GetComponent<EventTrigger>().triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener(delegate { StartCoroutine(RotZUp()); });
        zUp.GetComponent<EventTrigger>().triggers.Add(entry);

        entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener(delegate { StartCoroutine(RotZDown()); });
        zDown.GetComponent<EventTrigger>().triggers.Add(entry);
    }

    private void SetTextValues()
    {
        if (PlayerPrefs.HasKey("XRot"))
        {
            xVal.text = "" + PlayerPrefs.GetFloat("XRot");
            yVal.text = "" + PlayerPrefs.GetFloat("YRot");
            zVal.text = "" + PlayerPrefs.GetFloat("ZRot");
        }
    }

    private void SpawnKeys(Transform parent)
    {
        for (int i = 0; i < PlayerPrefs.GetInt("amountOfKeys"); i++)
        {
            if (currentKey.ToString().Length == 2)
            {

                go = Instantiate(Resources.Load(Path.Combine("Prefabs", "BlackOriginal")) as GameObject, parent);
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
                go = Instantiate(Resources.Load(Path.Combine("Prefabs", "WhiteOriginal")) as GameObject, parent);
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
        pos.text = piano.transform.localScale.x.ToString();
        rot.text = piano.transform.GetChild(0).localScale.x.ToString();
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

    public IEnumerator RotXUp()
    {
        while (Input.GetMouseButton(0))
        {
            float v = float.Parse(xVal.text);
            print(v);
            if (v >= 360)
                xVal.text = (v % 360).ToString();
            else
            {
                xVal.text = (v + .5f).ToString();
            }
            SetPianoRot();
            yield return new WaitForSeconds(.1f);
        }
    }

    public IEnumerator RotXDown()
    {
        while (Input.GetMouseButton(0))
        {
            float v = float.Parse(xVal.text);
            if (v < 0)
                xVal.text = (360 - v).ToString();
            else
                xVal.text = (v - .5f).ToString();
            SetPianoRot();
            yield return new WaitForSeconds(.1f);
        }
    }
    public IEnumerator RotYUp()
    {
        while (Input.GetMouseButton(0))
        {
            float v = float.Parse(yVal.text);
            if (v >= 360)
                yVal.text = (v % 360).ToString();
            else
                yVal.text = (v + .5f).ToString();
            SetPianoRot();
            yield return new WaitForSeconds(.1f);
        }
    }

    public IEnumerator RotYDown()
    {
        while (Input.GetMouseButton(0))
        {
            float v = float.Parse(yVal.text);
            if (v < 0)
                yVal.text = (360 - v).ToString();
            else
                yVal.text = (v - .5f).ToString();
            SetPianoRot();
            yield return new WaitForSeconds(.1f);
        }
    }
    public IEnumerator RotZUp()
    {
        while (Input.GetMouseButton(0))
        {
            float v = float.Parse(zVal.text);
            if (v >= 360)
                zVal.text = (v % 360).ToString();
            else
                zVal.text = (v + .5f).ToString();
            SetPianoRot();
            yield return new WaitForSeconds(.1f);
        }
    }

    public IEnumerator RotZDown()
    {
        while (Input.GetMouseButton(0))
        {
            float v = float.Parse(zVal.text);
            if (v < 0)
                zVal.text = (v % 360).ToString();
            else
                zVal.text = (v - .5f).ToString();
            SetPianoRot();
            yield return new WaitForSeconds(.1f);
        }
    }

    void SetPianoRot()
    {
        piano.transform.eulerAngles = new Vector3(float.Parse(xVal.text), float.Parse(yVal.text), float.Parse(zVal.text));
        print(piano.transform.eulerAngles);
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
