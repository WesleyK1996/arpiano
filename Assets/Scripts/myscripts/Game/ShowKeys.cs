using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

public class ShowKeys : MonoBehaviour
{
    public static ShowKeys Instance;
    public MakeKeys.PianoKeys currentPianoKey, redPianoKey;
    public int currentKey, redKey, keyNumber;
    private bool filledFirst;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(this);
    }

    private void Update()
    {
    }

    public IEnumerator CheckKey(NoteInfo note)
    {
        // If the note name that has been played equals the current note name that has to be played, it will return true
        if (note.note == ReadClip.Instance.notes[keyNumber].note)
        {
            //print("good");
            ShowKey(Resources.Load(Path.Combine("Materials", "GreenKey")) as Material, currentKey);
            yield return new WaitForSecondsRealtime(0.5f);
            keyNumber++;
            print("SUPPOSED TO GO TO NEXT KEY NOW!!!!!!!!!");
            //GoToNextKey();
        }
        else
        {
            //print("wrong");
            // It first finds the current key in keyformat (A B C D etc) Then it stores the Number on the piano it needs to hit.
            redPianoKey = MakeKeys.StringToKeystroke(note.note);
            //print("redKey = " + redKey);
            // No have multiple octaves stored yet in the system. To get the center piano keys, I use +48
            redKey = MakeKeys.GetMostLeftKey(redPianoKey) + 48;
            ////print("redKey = " + redKey);
            //redKey = MakeKeys.GetMostLeftKey(redPianoKey) + 24;
            //redKey = MakeKeys.GetMostLeftKey(redPianoKey) + 24;

            ShowKey(Resources.Load(Path.Combine("Materials", "RedKey")) as Material, redKey);
            yield return new WaitForSecondsRealtime(0.5f);
            ResetKey(redKey);
        }
    }

    public void ShowKey(int keyNumber)
    {
        ShowKey(Resources.Load(Path.Combine("Materials", "Glow")) as Material, keyNumber);
    }
    public void ShowKey(string keyName)
    {
        ShowKey(Resources.Load(Path.Combine("Materials", "Glow")) as Material, keyName);
    }

    void ShowKey(Material Mat, int ShowNumber)
    {
        //print("SETTING TRUE");

        //MakeKeys.Instance.keys[ShowNumber].SetActive(true);
        //makeKeys.keys[ShowNumber].transform.GetChild(0).GetComponent<Renderer>().enabled = true;
        //makeKeys.keys[ShowNumber].transform.GetChild(0).GetComponent<Collider>().enabled = true;
        //makeKeys.keys[ShowNumber].transform.GetChild(0).GetComponent<Renderer>().material = Mat;
        //makeKeys.keys[ShowNumber].transform.GetChild(1).GetComponent<TextMeshPro>().enabled = true;
    }

    void ShowKey(Material Mat, string KeyName)
    {
        //print("SETTING TRUE");
        foreach (GameObject key in MakeKeys.Instance.keys)
        {
            if (key.name == KeyName)
            {
                key.SetActive(true);
                key.transform.GetChild(0).GetComponent<Renderer>().enabled = true;
                key.transform.GetChild(0).GetComponent<Collider>().enabled = true;
                key.transform.GetChild(0).GetComponent<Renderer>().material = Mat;
                key.transform.GetChild(1).GetComponent<TextMeshPro>().enabled = true;
            }
        }
    }

    void ResetKey(int IntKey)
    {
        // Set the last used Key back to the original color
        if (KeyStrokeMaker.IntToKeyString(IntKey).Length == 2)
        {
            MakeKeys.Instance.keys[IntKey].transform.GetChild(0).GetComponent<Renderer>().material = Resources.Load(Path.Combine("Materials", "BlackKey")) as Material;
        }
        else
        {
            MakeKeys.Instance.keys[IntKey].transform.GetChild(0).GetComponent<Renderer>().material = Resources.Load(Path.Combine("Materials", "WhiteKey")) as Material;
        }

        MakeKeys.Instance.keys[IntKey].transform.GetChild(0).GetComponent<Renderer>().enabled = false;
        MakeKeys.Instance.keys[IntKey].transform.GetChild(1).GetComponent<TextMeshPro>().enabled = false;
    }

    public void ShowFirstKey()
    {
        keyNumber = 0;

        // It first finds the current key in keyformat (A B C D etc) Then it stores the Number on the piano it needs to hit.
        currentPianoKey = MakeKeys.StringToKeystroke(ReadClip.Instance.notes[keyNumber].note);
        // Not have multiple octaves stored yet in the system. To get the center piano keys, I use +48
        //currentKey = MakeKeys.GetMostLeftKey(currentPianoKey) + 48;

        ShowKey(Resources.Load(Path.Combine("Materials", "WhiteKey")) as Material, SheetManager.Instance.notesToPlay[0].note);

        filledFirst = true;
    }

    public void GoToNextKey()
    {
        //if (readClip.notes.Count <= keyNumber)
        //{
        //    print("Song Finished");
        //    if (readClip.notes.Count == keyNumber)
        //    {
        //        if (readClip.notes[keyNumber].isLeft)
        //            leftSheet.MoveNote();
        //        else rightSheet.MoveNote();
        //    }
        //    return;
        //}
        //// Set the last used Key back to the original color
        //if (filledFirst)
        //{
        //    ResetKey(currentKey);
        //}
        //// It first finds the current key in keyformat (A B C D etc) Then it stores the Number on the piano it needs to hit.
        ////currentPianoKey = CreateKeys.StringToKeystroke(readClip.notes[keyNumber].note);
        ////// Not have multiple octaves stored yet in the system. To get the center piano keys, I use +24
        ////currentKey = CreateKeys.GetMostLeftKey(currentPianoKey) + 24;

        //if (readClip.notes.Count > keyNumber + SheetManager.Instance.AmountOfKeys - 1)
        //{
        //    if (readClip.notes[keyNumber + SheetManager.Instance.AmountOfKeys - 1].isLeft)
        //        StartCoroutine(leftSheet.LoadNextNote(readClip.notes[keyNumber + SheetManager.Instance.AmountOfKeys - 1].ToString()));
        //    else
        //        StartCoroutine(rightSheet.LoadNextNote(readClip.notes[keyNumber + SheetManager.Instance.AmountOfKeys - 1].ToString()));
        //}
        //else
        //{
        //    if (readClip.notes[keyNumber + SheetManager.Instance.AmountOfKeys - 1].isLeft)
        //        leftSheet.MoveNote();
        //    else
        //        rightSheet.MoveNote();

        //    ShowKey(Resources.Load(Path.Combine("Materials", "WhiteKey")) as Material, keyNumber);
        //}
    }
}
