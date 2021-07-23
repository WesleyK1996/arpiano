//#define DEBUGPERF
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine.Events;
using MEC;

namespace MidiPlayerTK
{
    public partial class MidiStreamPlayer : MidiSynth
    {
        public bool MPTK_LogChord;

        private int currentGammeIndex = -1;
        private MPTKRangeLib range;

        /// <summary>
        /// Name of range selected
        /// </summary>
        public string MPTK_RangeName
        {
            get
            {
                return range != null ? range.Name : "Not set";
            }
        }

        /// <summary>
        /// Current selected range
        /// </summary>
        public int MPTK_RangeSelected
        {
            get { return currentGammeIndex; }
            set
            {
                if (currentGammeIndex != value)
                {
                    currentGammeIndex = value;
                    range = MPTKRangeLib.Range(currentGammeIndex, MPTK_LogChord);
                }
            }
        }

        /// <summary>
        /// [MPTK PRO] Play a chord from the current selected range (MPTK_RangeSelected), Tonic and Degree defined in parameter MPTKChord chord.
        /// Major range is selected if no range defined. See file GammeDefinition.csv in folder Resources/GeneratorTemplate
        /// </summary>
        /// <param name="chord">required: Tonic and Degree on top of the classical Midi parameters</param>
        /// <returns></returns>
        public MPTKChordBuilder MPTK_PlayChordFromRange(MPTKChordBuilder chord)
        {
            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    chord.Channel = Mathf.Clamp(chord.Channel, 0, Channels.Length - 1);

                    // Set a default range
                    if (MPTK_RangeSelected < 0) MPTK_RangeSelected = 0;

                    chord.MPTK_BuildFromRange(range);

                    if (!MPTK_CorePlayer)
                        Timing.RunCoroutine(TheadPlay(chord.Events));
                    else
                    {
                        lock (this) //V2.83
                        {
                            foreach (MPTKEvent evnt in chord.Events)
                                QueueSynthCommand.Enqueue(new SynthCommand() { Command = SynthCommand.enCmd.StartEvent, MidiEvent = evnt });
                        }
                    }
                }
                else
                    Debug.LogWarningFormat("SoundFont not yet loaded, Chord cannot be processed Tonic:{0} Degree:{1}", chord.Tonic, chord.Degree);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return chord;
        }
        /// <summary>
        /// [MPTK PRO] Play a chord from the chord library. See file ChordLib.csv in folder Resources/GeneratorTemplate. The Tonic is used to buid the chord
        /// </summary>
        /// <param name="chord">required: Tonic and FromLib on top of the classical Midi parameters</param>
        /// <returns></returns>
        public MPTKChordBuilder MPTK_PlayChordFromLib(MPTKChordBuilder chord)
        {
            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    chord.Channel = Mathf.Clamp(chord.Channel, 0, Channels.Length - 1);
                    chord.MPTK_BuildFromLib(chord.FromLib);

                    if (!MPTK_CorePlayer)
                        Timing.RunCoroutine(TheadPlay(chord.Events));
                    else
                    {
                        lock (this) //V2.83
                        {
                            foreach (MPTKEvent evnt in chord.Events)
                                QueueSynthCommand.Enqueue(new SynthCommand() { Command = SynthCommand.enCmd.StartEvent, MidiEvent = evnt });
                        }
                    }
                }
                else
                    Debug.LogWarningFormat("SoundFont not yet loaded, Chord cannot be processed Tonic:{0} Degree:{1}", chord.Tonic, chord.Degree);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return chord;
        }

        /// <summary>
        /// Stop playing the chord. All samples associated to the chord are stopped by sending a noteoff.
        /// </summary>
        /// <param name="chord"></param>
        public void MPTK_StopChord(MPTKChordBuilder chord)
        {
            if (chord.Events != null)
            {
                foreach (MPTKEvent evt in chord.Events)
                {
                    if (!MPTK_CorePlayer)
                        StopEvent(evt);
                    else
                        lock (this) //V2.83
                        {
                            QueueSynthCommand.Enqueue(new SynthCommand() { Command = SynthCommand.enCmd.StopEvent, MidiEvent = evt });
                        }
                }
            }
        }
    }
}

