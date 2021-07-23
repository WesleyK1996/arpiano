using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;

namespace MidiPlayerTK
{
    public class TestMidiListPlayer : MonoBehaviour
    {
        /// <summary>
        /// MPTK component able to play a Midi list. This PreFab must be present in your scene.
        /// </summary>
        public MidiListPlayer midiListPlayer;

        private void Start()
        {
            if (!HelperDemo.CheckSFExists()) return;

            // Find the Midi external component 
            if (midiListPlayer == null)
            {
                Debug.Log("No MidiListPlayer defined with the editor inspector, try to find one");
                MidiListPlayer fp = FindObjectOfType<MidiListPlayer>();
                if (fp == null)
                    Debug.Log("Can't find a MidiListPlayer in the Hierarchy. No music will be played");
                else
                {
                    midiListPlayer = fp;
                }
            }

            //! [Example OnEventStartPlayMidi]
            // Event trigger when midi file start playing
            if (!midiListPlayer.OnEventStartPlayMidi.HasEvent())
            {
                // Set event by scripit
                Debug.Log("OnEventStartPlayMidi defined by script");
                midiListPlayer.OnEventStartPlayMidi.AddListener(StartPlay);
            }
            else
                Debug.Log("OnEventStartPlayMidi defined by Unity editor");
            //! [Example OnEventStartPlayMidi]

            // Event trigger when midi file end playing
            if (!midiListPlayer.OnEventEndPlayMidi.HasEvent())
            {
                // Set event by script
                Debug.Log("OnEventEndPlayMidi defined by script");
                midiListPlayer.OnEventEndPlayMidi.AddListener(EndPlay);
            }
            else
                Debug.Log("OnEventEndPlayMidi defined by Unity editor");
        }

        /// <summary>
        /// Event fired by MidiFilePlayer when a midi is started (set by Unity Editor in MidiFilePlayer_1 Inspector)
        /// </summary>
        public void StartPlay(string name)
        {
            Debug.Log("Start Play Midi '" + name);
        }

        /// <summary>
        /// Event fired by MidiFilePlayer_1 when a midi is ended when reach end or stop by MPTK_Stop or Replay with MPTK_Replay
        /// The parameter reason give the origin of the end
        /// </summary>
        public void EndPlay(string name, EventEndMidiEnum reason)
        {
            Debug.LogFormat("End playing midi {0} reason:{1}", name, reason);
        }

        /// <summary>
        /// This method is fired from button to play the next midi in the list
        /// See canvas/button.
        /// </summary>
        public void Next()
        {
            midiListPlayer.MPTK_Next();
        }

        /// <summary>
        /// This method is fired from button to play the previous midi in the list
        /// See canvas/button.
        /// </summary>
        public void Previous()
        {
            midiListPlayer.MPTK_Previous();
        }

        public void CreateList()
        {
            midiListPlayer.MPTK_Stop();
            midiListPlayer.MPTK_NewList();
            midiListPlayer.MPTK_AddMidi("Baez Joan - Plaisir D'Amour", 10000, 20000);
            midiListPlayer.MPTK_AddMidi("Satie - Gnossienne", 15000, 30000);
            midiListPlayer.MPTK_PlayIndex=0;
        }
        public void UpdateList()
        {
            midiListPlayer.MPTK_Stop();
            midiListPlayer.MPTK_RemoveMidi("Baez Joan - Plaisir D'Amour");
            midiListPlayer.MPTK_AddMidi("Louis Armstrong - What A Wonderful World", 25000, 40000);
            midiListPlayer.MPTK_PlayIndex = 0;
        }

		public void Quit()
		{
			Application.Quit();
		}
    }
}