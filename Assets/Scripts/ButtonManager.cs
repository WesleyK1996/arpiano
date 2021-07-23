using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhythmTool.Examples;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ButtonManager : MonoBehaviour
{
    //public ClipReader clipReader;

    //public List<AudioClip> CurrentSongs;

    //int StartSongNumber = 0;

    //// I do this way because in the future it can easily change the pdfs to another one.
    //public Texture TempTex;
    //public GameObject StaticSheet;

    //// Now make an event and give it the name AllEvents, exactly as we make new bools, ints and strings
    //// Going to use every Event for a function of the button, so that can easily change functions of the button and add text
    //public List<Event> AllEvents;

    //public List<vbButton> AllButtons;

    //// This makes a new container that holds information (it is like a very advanced bool or int or string etc), this time it is multiple information.
    //[System.Serializable]
    //public class Event
    //{
    //    public string ButtonTitle;
    //    public UnityEvent MyEvent = new UnityEvent();
    //    public bool OneTimeEvent;
    //}

    //// Start is called before the first frame update
    //void Start()
    //{
    //    SetStartButtons();
    //}

    //public void ShowSongs()
    //{
    //    ResetAllButtons();
    //    for (int i = 0; i < 3; i++)
    //    {
    //        if (clipReader.songs.Count > i + StartSongNumber)
    //        {
    //            Event E = new Event();
    //            // In here i make a new button with a title, 3 events and connect it to the right button
    //            E.ButtonTitle = "Play:\n" + clipReader.songs[i + StartSongNumber].name;
    //            // This should have show 3 events in the bar
    //            int SongNumber = i + StartSongNumber;
    //            E.MyEvent.AddListener(() => { ResetAllButtons(); UpdateNewButton("45"); StartCoroutine(clipReader.LoadSong()); LoadTextureToStatic.T = TempTex; StaticSheet.SetActive(true); });
    //            E.OneTimeEvent = true;
    //            AllButtons[i].ChangeButton(E);
    //        }
    //    }

    //    Event Ev = new Event();
    //    // Make previous songs button
    //    if (StartSongNumber - 3 >= 0)
    //    {
    //        Ev.ButtonTitle = "Previous";
    //        Ev.MyEvent.AddListener(() => { ShowNextSongs(false); });
    //        AllButtons[3].ChangeButton(Ev);
    //    }

    //    // Make back button
    //    UpdateNewButton("43");

    //    // Make next songs button
    //    if (clipReader.songs.Count > StartSongNumber + 3)
    //    {
    //        Ev.ButtonTitle = "Next";
    //        Ev.MyEvent.AddListener(() => { ShowNextSongs(true); });
    //        AllButtons[5].ChangeButton(Ev);
    //    }
    //}

    //public void ShowNextSongs(bool IsNext)
    //{
    //    StartSongNumber += IsNext ? 3 : -3;
    //    ShowSongs();
    //}

    //// I also want to change buttons in the event system itself
    //public void UpdateNewButton(string DoubleNumber)
    //{
    //    try
    //    {
    //        int Number = int.Parse(DoubleNumber);
    //    }
    //    catch
    //    {
    //        Debug.LogError("This is no Integer");
    //        return;
    //    }

    //    int FirstNumber = int.Parse(DoubleNumber.Substring(0, 1));
    //    int SecondNumber;

    //    try
    //    {
    //        SecondNumber = int.Parse(DoubleNumber.Substring(1, DoubleNumber.Length - 1));
    //    }
    //    catch
    //    {
    //        Debug.LogError("String must be at least 2 numbers long");
    //        return;
    //    }
    //    AllButtons[FirstNumber].ChangeButton(AllEvents[SecondNumber]);
    //}

    //// Now it can easily change the start settings for the buttons without having to change all buttons
    //public void SetStartButtons()
    //{
    //    ResetTopRow();
    //    AllButtons[1].ChangeButton(AllEvents[7]);
    //    AllButtons[3].ChangeButton(AllEvents[0]);
    //    AllButtons[4].ChangeButton(AllEvents[4]);
    //    AllButtons[5].TurnOffButton();
    //    UpdateNewButton("56");
    //}

    //public void ResetTopRow()
    //{
    //    for (int i = 0; i < 3; i++)
    //    {
    //        AllButtons[i].TurnOffButton();
    //    }
    //}

    //public void ResetBottomRow()
    //{
    //    for (int i = 3; i < 6; i++)
    //    {
    //        AllButtons[i].TurnOffButton();
    //    }
    //}

    //public void ResetAllButtons()
    //{
    //    for (int i = 0; i < AllButtons.Count; i++)
    //    {
    //        AllButtons[i].TurnOffButton();
    //    }
    //}

    //public void ResetScene()
    //{
    //    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    //}
}
