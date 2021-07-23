using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

[ExecuteInEditMode]
public class KeyStrokeMaker : MonoBehaviour
{
    // The width of each key //
    public float WhiteKeyWidth, BlackKeyWidth, WhiteKeyHeight, BlackKeyHeight;
    // The position of your piano
    public Vector3 StartPos, StartRot;
    // The amount of keys
    //public int TotalKeys;
    public int CurrentPos;
    public static int TotalStaticKeys;

    // Calibration info.
    Transform Parent;
    public static bool IsCalibrating;

    // Current Placeholder for showing if the keys are correctly placed
    public GameObject KeyWhite, KeyBlack, KeyRed, KeyGreen;

    //float _WhiteKeyWidth, _BlackKeyWidth, _WhiteKeyHeight, _BlackKeyHeight;
    public enum PianoKeys { A, AS, B, C, CS, D, DS, E, F, FS, G, GS, NUL }

    //public PianoKeys StartKey; value was A
    public static PianoKeys StartKeyStatic;
    PianoKeys CurrentKey;

    public bool KeysSpawned;

    // I use this for storing all the keys so I can easily manage which one to use
    public List<GameObject> AllKeys = new List<GameObject>();

    public void OnEnable()
    {
        //Parent = transform.parent;
        //LoadCalibration();
        StartKeyStatic = GetStartKey();
        if (!PlayerPrefs.HasKey("amountOfKeys"))
            PlayerPrefs.SetInt("amountOfKeys", 88);
        TotalStaticKeys = PlayerPrefs.GetInt("amountOfKeys");
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            SpawnAllKeys();
            //if (!IsCalibrating && SceneManager.GetActiveScene() == SceneManager.GetSceneByName("SampleScene"))
            //    Calibrate();
            //else
            //    FinishCalibrating();
        }
    }

    PianoKeys GetStartKey()
    {
        if (!PlayerPrefs.HasKey("startKey"))
            PlayerPrefs.SetInt("startKey", 0);
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

    public void ResetKeys()
    {
        DeleteAllKeys();
        SpawnAllKeys();
    }

    ///// <summary>
    /////  Starts calibrating the piano by attaching it to the camera.
    ///// </summary>
    //public void Calibrate()
    //{
    //    if (!IsCalibrating)
    //    {
    //        if (!KeysSpawned)
    //            SpawnAllKeys();
    //        transform.parent = Camera.main.transform;
    //        IsCalibrating = true;
    //        GetComponent<MusicKeyShower>().ShowKey(GetCenteredKey(StringToKeystroke("C")));
    //    }
    //}

    ///// <summary>
    /////  Ends calibrating the piano by attaching it to the ImageTarget.
    ///// </summary>
    //public void FinishCalibrating()
    //{
    //    if (IsCalibrating)
    //    {
    //        //DeleteAllKeys();
    //        transform.parent = Parent;
    //        IsCalibrating = false;
    //        GetComponent<MusicKeyShower>().HideKey(GetCenteredKey(StringToKeystroke("C")));
    //        //SaveCalibration(transform);
    //    }
    //}

    ///// <summary>
    ///// Saves the calibration data
    ///// </summary>
    ///// <param name = "T" > Give transform to save the calibration data</param>
    //public void SaveCalibration(Transform T)
    //{
    //    PlayerPrefs.SetFloat("CalXPos", T.localPosition.x);
    //    PlayerPrefs.SetFloat("CalYPos", T.localPosition.y);
    //    PlayerPrefs.SetFloat("CalZPos", T.localPosition.z);
    //    PlayerPrefs.SetFloat("CalXRot", T.localEulerAngles.x);
    //    PlayerPrefs.SetFloat("CalYRot", T.localEulerAngles.y);
    //    PlayerPrefs.SetFloat("CalZRot", T.localEulerAngles.z);
    //    StartPos = T.localPosition;
    //    StartRot = T.localEulerAngles;
    //}

    ///// <summary>
    ///// Load the calibration data
    ///// </summary>
    //public void LoadCalibration()
    //{
    //    StartPos.x = PlayerPrefs.GetFloat("XPos");
    //    StartPos.y = PlayerPrefs.GetFloat("YPos");
    //    StartPos.z = PlayerPrefs.GetFloat("ZPos");
    //    StartRot.x = PlayerPrefs.GetFloat("XRot");
    //    StartRot.y = PlayerPrefs.GetFloat("YRot");
    //    StartRot.z = PlayerPrefs.GetFloat("ZRot");
    //}

    public void SpawnAllKeys()
    {
        print(0);
        AllKeys.Clear();
        KeysSpawned = true;
        CurrentPos = 0;
        CurrentKey = StartKeyStatic;
        for (int i = 0; i < TotalStaticKeys; i++)
        {
            if (CurrentKey.ToString().Length == 2)
            {
                GameObject Go = Instantiate(KeyBlack, transform);
                Go.tag = "Key";
                Go.transform.localPosition = new Vector3(0, 0, ((CurrentPos + 1) * 10 * WhiteKeyWidth - BlackKeyWidth * 5) * -1);
                Vector3 SizeScale = Go.transform.Find("Plane").localScale;
                SizeScale.x = BlackKeyHeight;
                SizeScale.z = BlackKeyWidth;
                Go.transform.Find("Plane").localScale = SizeScale;

                Transform Letter = Go.transform.Find("Letter");
                Letter.localScale = new Vector3(SizeScale.z, SizeScale.z, SizeScale.z) * 1.5f;
                Vector3 Pos = Letter.localPosition;
                Pos.x = -5 * SizeScale.x;
                Letter.localPosition = Pos;
                Letter.GetComponent<TextMeshPro>().text = CurrentKey.ToString();

                AllKeys.Add(Go);
            }
            else
            {
                GameObject Go = Instantiate(KeyWhite, transform);
                Go.tag = "Key";
                Go.transform.localPosition = new Vector3(0, 0, (CurrentPos * 10 * WhiteKeyWidth) * -1);
                Vector3 SizeScale = Go.transform.Find("Plane").localScale;
                SizeScale.x = WhiteKeyHeight;
                SizeScale.z = WhiteKeyWidth;
                Go.transform.Find("Plane").localScale = SizeScale;

                Transform Letter = Go.transform.Find("Letter");
                Letter.localScale = new Vector3(SizeScale.z, SizeScale.z, SizeScale.z);
                Vector3 Pos = Letter.localPosition;
                Pos.x = -5 * SizeScale.x;
                Letter.localPosition = Pos;
                Letter.GetComponent<TextMeshPro>().text = CurrentKey.ToString();

                AllKeys.Add(Go);
            }
            CurrentKey++;
            if (CurrentKey == PianoKeys.NUL)
                CurrentKey = 0;

            if (CurrentKey.ToString().Length != 2)
                CurrentPos++;
        }

        Calibrate.LoadCalibration(transform);
    }

    public void DeleteAllKeys()
    {
        foreach (Transform Child in transform)
        {
            Destroy(Child.gameObject);
            AllKeys.RemoveRange(0, AllKeys.Count);
        }
        KeysSpawned = false;
    }


    /// <summary>
    /// Transfers a string to a pianokey letter
    /// </summary>
    /// <param name="KeyName"> The name of the key, should be the same as one of the enum values</param>
    /// <returns></returns>
    public static PianoKeys StringToKeystroke(string KeyName)
    {
        KeyName = ReturnAllButNumber(KeyName);
        return (PianoKeys)System.Enum.Parse(typeof(PianoKeys), KeyName);
    }

    static string ReturnAllButNumber(string s)
    {
        string r = "";
        for (int i = 0; i < s.Length; i++)
        {
            if (IsNotNumber(s[i]))
                r += s[i];
        }
        return r;
    }

    private static bool IsNotNumber(char v)
    {
        return v >= 0 && v <= 9;
    }

    /// <summary>
    /// Transfers a int to a pianokey letter in string
    /// </summary>
    /// <param name="KeyName"> The number on the Keyboard</param>
    /// <returns></returns>
    public static string IntToKeyString(int KeyNumber)
    {
        return (KeyNumber % 12 + StartKeyStatic).ToString();
    }

    /// <summary>
    /// Returns the first given key of that value
    /// </summary>
    /// <param name="Key">Fill in your pianokey you want to have</param>
    /// <returns></returns>
    public static int GetMostLeftKey(PianoKeys Key)
    {
        if ((int)Key >= (int)StartKeyStatic)
            return (int)Key - (int)StartKeyStatic;
        else
            return (int)Key + 12 - (int)StartKeyStatic;
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
        return (int)Mathf.Ceil((TotalStaticKeys - StartKey) / 12f); ;
    }
}
