using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MusicKeyShower : MonoBehaviour
{
    public SheetManager leftSheet, rightSheet;
    public RhythmTool.Examples.ClipReader clipReader;
    public KeyStrokeMaker Keys;
    public KeyStrokeMaker.PianoKeys CurrentKey, RedKey;
    public int CurrentPianoNumber, RedPianoNumber, KeyNumberInSong;
    public Material GlowWhite, Black, White, GlowGreen, GlowRed;
    bool filledFirst;

    bool EverythingIsHidden;

    bool TestSwitch;

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    ShowKey(GlowGreen, CurrentPianoNumber);
        //    KeyNumberInSong++;
        //    GoToNextKey();
        //}

        //if (Input.GetKeyDown(KeyCode.Y))
        //{
        //    ShowCMinor();
        //}
        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    if (!TestSwitch)
        //    {
        //        ShowKey(KeyStrokeMaker.GetCenteredKey(KeyStrokeMaker.StringToKeystroke("C")));
        //        TestSwitch = true;
        //    }
        //    else
        //    {
        //        HideKey(KeyStrokeMaker.GetCenteredKey(KeyStrokeMaker.StringToKeystroke("C")));
        //        TestSwitch = false;
        //    }
        //}
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    if (!TestSwitch)
        //    {
        //        ShowKey(KeyStrokeMaker.GetCenteredKey(KeyStrokeMaker.StringToKeystroke("A")));
        //        TestSwitch = true;
        //    }
        //    else
        //    {
        //        HideKey(KeyStrokeMaker.GetCenteredKey(KeyStrokeMaker.StringToKeystroke("A")));
        //        TestSwitch = false;
        //    }
        //}
    }

    // C, Dis, G
    public void ShowCMinor()
    {
        GetComponent<KeyStrokeMaker>().ResetKeys();
        // This turns all keys off
        foreach (GameObject Key in Keys.AllKeys)
        {
            EverythingIsHidden = true;
            Key.transform.GetChild(0).GetComponent<Renderer>().enabled = false;
            Key.transform.GetChild(0).GetComponent<Collider>().enabled = false;
            Key.transform.GetChild(1).GetComponent<TextMeshPro>().enabled = false;
        }
        // Highlight the specific Accord
        ShowKey(GlowWhite, 27);
        ShowKey(GlowWhite, 30);
        ShowKey(GlowWhite, 34);
    }

    // Highlights a specific key
    public void ShowKey(int KeyNumber)
    {
        ShowKey(GlowWhite, KeyNumber);
    }

    // Hides a specific key
    public void HideKey(int KeyNumber)
    {
        ResetKey(KeyNumber);
    }

    public IEnumerator CheckKey(string Note)
    {
        // If the note name that has been played equals the current note name that has to be played, it will return true
        if (Note == clipReader.Notes[KeyNumberInSong])
        {
            print(Note + "Is good");
            ShowKey(GlowGreen, CurrentPianoNumber);
            yield return new WaitForSecondsRealtime(0.5f);
            KeyNumberInSong++;
            GoToNextKey();
        }
        else
        {
            print(Note + "Is wrong");
            // It first finds the current key in keyformat (A B C D etc) Then it stores the Number on the piano it needs to hit.
            //RedKey = KeyStrokeMaker.StringToKeystroke(Note);
            // No have multiple octaves stored yet in the system. To get the center piano keys, I use +48
            RedPianoNumber = KeyStrokeMaker.GetMostLeftKey(RedKey) + 24;
            RedPianoNumber = KeyStrokeMaker.GetMostLeftKey(RedKey) + 24;

            ShowKey(GlowRed, RedPianoNumber);
            yield return new WaitForSecondsRealtime(0.5f);
            ResetKey(RedPianoNumber);
        }
    }

    void ShowKey(Material Mat, int ShowNumber)
    {
        print("theres ducks in my dick");
        Keys.AllKeys[ShowNumber].transform.GetChild(0).GetComponent<Renderer>().enabled = true;
        Keys.AllKeys[ShowNumber].transform.GetChild(0).GetComponent<Collider>().enabled = true;
        Keys.AllKeys[ShowNumber].transform.GetChild(0).GetComponent<Renderer>().material = Mat;
        Keys.AllKeys[ShowNumber].transform.GetChild(1).GetComponent<TextMeshPro>().enabled = true;
    }

    void ResetKey(int IntKey)
    {
        // Set the last used Key back to the original color
        if (KeyStrokeMaker.IntToKeyString(IntKey).Length == 2)
        {
            Keys.AllKeys[IntKey].transform.GetChild(0).GetComponent<Renderer>().material = Black;
            if (EverythingIsHidden)
            {
                Keys.AllKeys[IntKey].transform.GetChild(0).GetComponent<Renderer>().enabled = false;
                Keys.AllKeys[IntKey].transform.GetChild(1).GetComponent<TextMeshPro>().enabled = false;
            }
        }
        else
        {
            Keys.AllKeys[IntKey].transform.GetChild(0).GetComponent<Renderer>().material = White;
            if (EverythingIsHidden)
            {
                Keys.AllKeys[IntKey].transform.GetChild(0).GetComponent<Renderer>().enabled = false;
                Keys.AllKeys[IntKey].transform.GetChild(1).GetComponent<TextMeshPro>().enabled = false;
            }
        }
    }

    public void ShowFirstKey()
    {
        KeyNumberInSong = 0;

        // This turns all keys off
        foreach (GameObject Key in Keys.AllKeys)
        {
            EverythingIsHidden = true;
            Key.transform.GetChild(0).GetComponent<Renderer>().enabled = false;
            Key.transform.GetChild(0).GetComponent<Collider>().enabled = false;
            Key.transform.GetChild(1).GetComponent<TextMeshPro>().enabled = false;
        }

        // It first finds the current key in keyformat (A B C D etc) Then it stores the Number on the piano it needs to hit.
        //CurrentKey = KeyStrokeMaker.StringToKeystroke(clipReader.Notes[KeyNumberInSong].ToString());
        // Not have multiple octaves stored yet in the system. To get the center piano keys, I use +48
        CurrentPianoNumber = KeyStrokeMaker.GetMostLeftKey(CurrentKey) + 24;

        ShowKey(GlowWhite, CurrentPianoNumber);

        filledFirst = true;
    }

    public void GoToNextKey()
    {
    //    if (clipReader.Notes.Count <= KeyNumberInSong)
    //    {
    //        print("Song Finished");
    //        if (clipReader.Notes.Count == KeyNumberInSong)
    //        {
    //            if (clipReader.Notes[clipReader.Notes.Count][1] != '#')
    //                if (clipReader.Notes[clipReader.Notes.Count][1] < 3)
    //                {
    //                    leftSheet.MoveNote();
    //                }
    //                else if (clipReader.Notes[clipReader.Notes.Count][1] > 3)
    //                {
    //                    rightSheet.MoveNote();
    //                }
    //                else
    //                {
    //                    if (clipReader.Notes[clipReader.Notes.Count][0] >= 'C')
    //                    {
    //                        rightSheet.MoveNote();
    //                    }
    //                    else
    //                        leftSheet.MoveNote();
    //                }
    //        }
    //        return;
    //    }
    //    // Set the last used Key back to the original color
    //    if (filledFirst)
    //    {
    //        ResetKey(CurrentPianoNumber);
    //    }
        // It first finds the current key in keyformat (A B C D etc) Then it stores the Number on the piano it needs to hit.
        //CurrentKey = KeyStrokeMaker.StringToKeystroke(clipReader.Notes[KeyNumberInSong].ToString());
        // Not have multiple octaves stored yet in the system. To get the center piano keys, I use +24
        //CurrentPianoNumber = KeyStrokeMaker.GetMostLeftKey(CurrentKey) + 24;

        //if (clipReader.Notes.Count > KeyNumberInSong + MusicSheetManager.AmountOfKeys - 1)
        //{
        //    StartCoroutine(SheetManager.LoadNextNote(clipReader.Notes[KeyNumberInSong + MusicSheetManager.AmountOfKeys - 1].ToString()));
        //}
        //else
        //{
        //    SheetManager.MoveNote();
        //}

        //ShowKey(GlowWhite, CurrentPianoNumber);
    }
}
