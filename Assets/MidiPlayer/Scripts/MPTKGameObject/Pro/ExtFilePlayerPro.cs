using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;
using UnityEngine.Events;
using MEC;

namespace MidiPlayerTK
{
    public partial class MidiFilePlayer : MidiSynth
    {
        /// <summary>
        /// [MPTK PRO] - Find a Midi in the Unity resources folder MidiDB which contains the name (case sensitive)
        /// Tips: Add Midi files to your project with the Unity menu MPTK or add it directly in the ressource folder and open Midi File Setup to automatically integrate Midi in MPTK.
        ///! @code
        /// midiFilePlayer.MPTK_SearchMidiToPlay("Adagio");
        /// midiFilePlayer.MPTK_Play();
        ///! @endcode
        /// </summary>
        public void MPTK_SearchMidiToPlay(string name)
        {
            int index = -1;
            try
            {
                if (!string.IsNullOrEmpty(name))
                {
                    if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null)
                    {
                        index = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s.Contains(name));
                        if (index >= 0)
                        {
                            MPTK_MidiIndex = index;
                            Debug.LogFormat("MPTK_SearchMidiToPlay: '{0}' selected", MPTK_MidiName);
                        }
                        else
                            Debug.LogWarningFormat("No Midi file found with '{0}' in name", name);
                    }
                    else
                        Debug.LogWarning(MidiPlayerGlobal.ErrorNoMidiFile);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// [MPTK PRO] - Play next or previous Midi from the MidiDB list.
        /// </summary>
        /// <param name="offset">Forward or backward count in the list. 1:the next, -1:the previous</param>
        public void MPTK_PlayNextOrPrevious(int offset)
        {
            try
            {
                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                {
                    int selectedMidi = MPTK_MidiIndex + offset;
                    if (selectedMidi < 0)
                        selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count - 1;
                    else if (selectedMidi >= MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count)
                        selectedMidi = 0;
                    MPTK_MidiIndex = selectedMidi;
                    if (offset < 0)
                        prevMidi = true;
                    else
                        nextMidi = true;
                    MPTK_RePlay();
                }
                else
                    Debug.LogWarning(MidiPlayerGlobal.ErrorNoMidiFile);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        //! @cond NODOC
        public void StopAndPlayMidi(int index, string name)
        {
            MPTK_Stop();
            if (!string.IsNullOrWhiteSpace(name))
                MPTK_SearchMidiToPlay(name);
            else
                MPTK_MidiIndex = index;
            MPTK_Play();
        }

        public void PlayAndPauseMidi(int index, string name, int pauseMs=-1)
        {
            MPTK_Stop();
            if (!string.IsNullOrWhiteSpace(name))
                MPTK_SearchMidiToPlay(name);
            else
                MPTK_MidiIndex = index;
            MPTK_Play();
            MPTK_Pause(pauseMs);
        }
        //! @endcond

    }
}

