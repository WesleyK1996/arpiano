using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class MakeKeys : MonoBehaviour
{
    public static MakeKeys Instance;
    public float whiteKeyWidth, blackKeyWidth, whiteKeyHeight, blackKeyHeight;

    public int currentPos;

    public enum PianoKeys { A, AS, B, C, CS, D, DS, E, F, FS, G, GS, NUL }
    PianoKeys currentKey;
    static PianoKeys startKey;

    public List<GameObject> keys = new List<GameObject>();
    GameObject go;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(this);
        startKey = GetStartKey();
        if (!PlayerPrefs.HasKey("amountOfKeys"))
            PlayerPrefs.SetInt("amountOfKeys", 88);
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            SpawnKeys();
            Calibrate.LoadCalibration(transform);
        }
    }

    PianoKeys GetStartKey()
    {
        switch (PlayerPrefs.GetInt("startKey"))
        {
            case 0:
                return PianoKeys.A;
            case 1:
                return PianoKeys.B;
            case 2:
                return PianoKeys.C;
            case 3:
                return PianoKeys.D;
            case 4:
                return PianoKeys.E;
            case 5:
                return PianoKeys.F;
            case 6:
                return PianoKeys.G;

        }
        return PianoKeys.A;
    }

    void SpawnKeys()
    {
        keys.Clear();
        for (int i = 0; i < PlayerPrefs.GetInt("amountOfKeys"); i++)
        {
            if (currentKey.ToString().Length == 2)
            {
                go = Instantiate(Resources.Load(Path.Combine("Prefabs", "BlackOriginal")) as GameObject, transform);
                go.tag = "Key";
                go.name = IntToKeyString(i) + (Mathf.Floor(i / 12) + 1);
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

                keys.Add(go);
            }
            else
            {
                go = Instantiate(Resources.Load(Path.Combine("Prefabs", "WhiteOriginal")) as GameObject, transform);
                go.tag = "Key";
                go.name = IntToKeyString(i) + (Mathf.Floor(i / 12) + 1);
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

                keys.Add(go);
            }
            //go.SetActive(false);

            currentKey++;
            if (currentKey == PianoKeys.NUL)
                currentKey = 0;

            if (currentKey.ToString().Length != 2)
                currentPos++;
        }
        transform.position = new Vector3(-PlayerPrefs.GetInt("amountOfKeys") / 2, 0, 0);
        Calibrate.LoadCalibration(transform);
    }

    /// <summary>
    /// Transfers a string to a pianokey letter
    /// </summary>
    /// <param name="KeyName"> The name of the key, should be the same as one of the enum values</param>
    /// <returns></returns>
    public static PianoKeys StringToKeystroke(string KeyName)
    {
        KeyName = ReturnAllButNumber(KeyName);
        //print(KeyName);
        return (PianoKeys)System.Enum.Parse(typeof(PianoKeys), KeyName);
    }

    static string ReturnAllButNumber(string s)
    {
        string r = "";
        for (int i = 0; i < s.Length; i++)
        {
            if (!IsNumber(s[i]))
                if (s[i] == '#')
                    r += 'S';
                else
                    r += s[i];
        }
        return r;
    }

    private static bool IsNumber(char v)
    {
        return v >= '0' && v <= '9';
    }

    /// <summary>
    /// Transfers a int to a pianokey letter in string
    /// </summary>
    /// <param name="KeyName"> The number on the Keyboard</param>
    /// <returns></returns>
    public static string IntToKeyString(int KeyNumber)
    {
        return (KeyNumber % 12 + startKey).ToString().Replace('S', '#');
    }

    /// <summary>
    /// Returns the first given key of that value
    /// </summary>
    /// <param name="Key">Fill in your pianokey you want to have</param>
    /// <returns></returns>
    public static int GetMostLeftKey(PianoKeys Key)
    {
        if ((int)Key >= (int)startKey)
            return (int)Key - (int)startKey;
        else
            return (int)Key + 12 - (int)startKey;
    }

    /// <summary>
    /// Accumulates the centered key on your keyboard
    /// </summary>
    /// <param name="Key">Fill in your pianokey you want to have</param>
    /// <returns></returns>
    public static int GetCenteredKey(PianoKeys Key)
    {
        int AmountOfKeys = GetAmountOfKeys(Key);
        return (int)Mathf.Ceil(AmountOfKeys / 2f) * 12 - 12 + GetMostLeftKey(Key);
    }

    /// <summary>
    /// Finds out how many of the keys exists
    /// </summary>
    /// <param name="Key">Fill in your pianokey you want to have</param>
    /// <returns></returns>
    public static int GetAmountOfKeys(PianoKeys Key)
    {
        int StartKey = GetMostLeftKey(Key);
        return (int)Mathf.Ceil((PlayerPrefs.GetInt("amountOfKeys") - StartKey) / 12f); ;
    }
}
