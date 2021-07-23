﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;
using UnityEngine.Events;

namespace MidiPlayerTK
{
    public class TestMidiInputScripting : MonoBehaviour
    {
        /// <summary>
        /// MPTK component able to read from a Midi device connected yo your desktop (Windows and MacOS).
        /// This PreFab must be present in your scene.
        /// </summary>
        public MidiInReader midiInReader;

        private CustomStyle myStyle;

        private Vector2 scrollerWindow = Vector2.zero;
        private int buttonWidth = 250;
        //private int heightLyrics = 10;

        private string infoMidi = "";
        private string infoNothing = "Nothing for now ...\nConnect your keyboard and play!";
        private Vector2 scrollPos1 = Vector2.zero;

        private void Start()
        {
            if (!HelperDemo.CheckSFExists()) return;

            // Warning: when defined by script, this event is not triggered at first load of MPTK 
            // because MidiPlayerGlobal is loaded before any other gamecomponent
            if (!MidiPlayerGlobal.OnEventPresetLoaded.HasEvent())
            {
                // To be done in Start event (not Awake)
                MidiPlayerGlobal.OnEventPresetLoaded.AddListener(EndLoadingSF);
            }


            if (midiInReader == null)
            {
                Debug.Log("No MidiInReader defined with the editor inspector, try to find one");
                MidiInReader midiIn = FindObjectOfType<MidiInReader>();
                if (midiIn == null)
                    Debug.Log("Can't find a MidiInReader in the Hierarchy. No events will be read");
                else
                {
                    midiInReader = midiIn;
                }
            }

            if (midiInReader != null)
            {
                // There is two methods to trigger event: 
                //      1) in inpector from the Unity editor 
                //      2) by script, see below
                // ------------------------------------------

                // Event trigger when midi file end playing
                if (!midiInReader.OnEventInputMidi.HasEvent())
                {
                    // Set event by script
                    Debug.Log("OnEventInputMidi defined by script");
                    //midiInReader.OnEventInputMidi.AddListener(MidiReadEvents);
                    midiInReader.OnEventInputMidi.AddListener((MPTKEvent evt) => 
                    {
                        infoMidi += evt.ToString();
                        if (infoMidi.Length > 10000) infoMidi = infoMidi.Substring(5000, infoMidi.Length - 5000);
                        scrollPos1 = new Vector2(0, 99999999999999f);
                    });
                }
                else
                    Debug.Log("OnEventInputMidi defined with inspector");
            }
        }

        /// <summary>
        /// This call is defined from MidiPlayerGlobal event inspector. Run when SF is loaded.
        /// Warning: not triggered at first load of MPTK because MidiPlayerGlobal id load before any other gamecomponent
        /// </summary>
        public void EndLoadingSF()
        {
            Debug.Log("End loading SF, MPTK is ready to play");
            Debug.Log("Load statistique");
            Debug.Log("   Time To Load SoundFont: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadSoundFont.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Time To Load Samples: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadWave.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Presets Loaded: " + MidiPlayerGlobal.MPTK_CountPresetLoaded);
            Debug.Log("   Samples Loaded: " + MidiPlayerGlobal.MPTK_CountWaveLoaded);
        }

        /// <summary>
        /// Event fired by MidiFilePlayer when a midi notes are available 
        /// Use this method if you want defined the event from Unity Editor in MidiInReader Inspector.
        /// </summary>
        public void MidiReadEvents(MPTKEvent midievent)
        {
            infoMidi += midievent.ToString();
            if (infoMidi.Length > 10000) infoMidi = infoMidi.Substring(5000, infoMidi.Length - 5000);
            scrollPos1 = new Vector2(0, 99999999999999f);
        }

        void OnGUI()
        {
            int spaceV = 10;
            if (!HelperDemo.CheckSFExists()) return;

            // Set custom Style. Good for background color 3E619800
            if (myStyle == null)
                myStyle = new CustomStyle();

            if (midiInReader != null)
            {
                scrollerWindow = GUILayout.BeginScrollView(scrollerWindow, false, false, GUILayout.Width(Screen.width));

                MainMenu.Display("Test Midi In Reader - Connect Midi input device to the MPTK Synth", myStyle);

                GUISelectSoundFont.Display(scrollerWindow, myStyle);

                // Horizontal: 2 columns
                GUILayout.BeginHorizontal();

                //
                // Left column: Midi action
                // ------------------------

                GUILayout.BeginVertical(myStyle.BacgDemos, GUILayout.Width(450));

                // Define the global volume
                GUILayout.Space(spaceV);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Global Volume: " + Math.Round(midiInReader.MPTK_Volume, 2), myStyle.TitleLabel3, GUILayout.Width(220));
                midiInReader.MPTK_Volume = GUILayout.HorizontalSlider(midiInReader.MPTK_Volume * 100f, 0f, 100f, GUILayout.Width(buttonWidth)) / 100f;
                GUILayout.EndHorizontal();

                // Transpose each note
                GUILayout.Space(spaceV);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Note Transpose: " + midiInReader.MPTK_Transpose, myStyle.TitleLabel3, GUILayout.Width(220));
                midiInReader.MPTK_Transpose = (int)GUILayout.HorizontalSlider((float)midiInReader.MPTK_Transpose, -24f, 24f, GUILayout.Width(buttonWidth));
                GUILayout.EndHorizontal();

                GUILayout.Space(spaceV);
                GUILayout.BeginHorizontal(GUILayout.Width(350));
                GUILayout.Label("Voices Statistics ", myStyle.TitleLabel3, GUILayout.Width(220));
                GUILayout.Label(string.Format("Played:{0}   Free:{1}   Active:{2}   Reused:{3} %",
                    midiInReader.MPTK_StatVoicePlayed, midiInReader.MPTK_StatVoiceCountFree, midiInReader.MPTK_StatVoiceCountActive, Mathf.RoundToInt(midiInReader.MPTK_StatVoiceRatioReused)),
                    myStyle.TitleLabel3, GUILayout.Width(320));

                GUILayout.EndHorizontal();

                GUILayout.Space(spaceV);
                if (GUILayout.Button(new GUIContent("Clear", ""), GUILayout.Width(buttonWidth)))
                    infoMidi = "";

                // Enable or disable channel
                GUILayout.Space(spaceV);
                GUILayout.Label("Channel / Preset, enable or disable channel: ", myStyle.TitleLabel3, GUILayout.Width(400));

                GUILayout.BeginHorizontal();
                for (int channel = 0; channel < midiInReader.MPTK_ChannelCount(); channel++)
                {
                    bool state = GUILayout.Toggle(midiInReader.MPTK_ChannelEnableGet(channel), string.Format("{0} / {1}", channel + 1, midiInReader.MPTK_ChannelPresetGetIndex(channel)), GUILayout.Width(65));
                    if (state != midiInReader.MPTK_ChannelEnableGet(channel))
                    {
                        midiInReader.MPTK_ChannelEnableSet(channel, state);
                        Debug.LogFormat("Channel {0} state:{1}, preset:{2}", channel + 1, state, midiInReader.MPTK_ChannelPresetGetName(channel) ?? "not set");
                    }

                    if (channel == 7)
                    {
                        // Create a new line ...
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                //
                // Right Column: midi events
                // ------------------------------------------
                GUILayout.BeginVertical(myStyle.BacgDemos);
                scrollPos1 = GUILayout.BeginScrollView(scrollPos1, false, true);//, GUILayout.Height(heightLyrics));
                string info = string.IsNullOrEmpty(infoMidi) ? infoNothing : infoMidi;
                GUILayout.Label(info, myStyle.TextFieldMultiLine);
                GUILayout.EndScrollView();
                GUILayout.EndVertical();

                // End Horizontal: 2 columns
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(myStyle.BacgDemos);
                GUILayout.Label("Go to your Hierarchy, select GameObject MidiInReader: inspector contains a lot of parameters to control the sound.", myStyle.TitleLabel2);
                GUILayout.EndHorizontal();

                GUILayout.EndScrollView();

            }

        }
    }
}