using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MidiPlayerTK
{
    /// <summary>
    /// Example of implementation of spatialization by channel. This script must be adapted to your need.
    /// Here, the goal is to represent each Midi channel by a sphere. The vertical position of the sphere
    /// depends on the note playing on the channel. If there is no note, the sphere is not visible.
    /// Important notes:
    ///     - This script is attached to the prefab MidiSpatializer on the scene. 
    ///     - At start, this prefab instanciates 16 identical gameComponents, one by midi channel, with all the hierarchy components attached
    ///         - a sphere, only for decoration
    ///         - a text above the sphere
    ///         - a script TestSpatializerFly (this one) to be adapted to your need
    ///         - and a script MidiSynth dedicated to a channel, MPTK_DedicatedChannel contains the channel.
    ///     - the main prefab MidiSpatializer remains activ, but with this specific role:
    ///         - sphere: disabled
    ///         - text above the sphere: disabled
    ///         - TestSpatializerFly (this one): used only when user interacts with the interface (buttons). See bellow ArrangeRandom() for example.
    ///         - MidiSynth dedicated to reading the current Midi, no playing, MPTK_DedicatedChannel is set to -1. The Midi events read are redirected to the dedicated MidiSynth/Channel
    ///     
    /// 
    /// </summary>
    public class TestSpatializerFly : MonoBehaviour
    {
        /// <summary>
        /// MPTK component able to read and play a Midi file. Inherits from MidiSynth.
        /// </summary>
        public MidiFilePlayer midiFilePlayer;

        /// <summary>
        /// Text to display above the sphere, the instrument name.
        /// </summary>
        public TextMesh textPlayer;

        /// <summary>
        /// Text to display at the bottom of the screen: midi title + help
        /// </summary>
        public Text textInfo;

        /// <summary>
        /// The sphere!
        /// </summary>
        public Transform sphere;

        /// <summary>
        /// Display more info above the sphere
        /// </summary>
        public bool DisplayInfo;

        /// <summary>
        /// Current vertical position of the sphere
        /// </summary>
        public Vector3 PosSynth;

        /// <summary>
        /// Count of notes on this channel since the beguinning of the play.
        /// </summary>
        public int countNote;

        /// <summary>
        /// Current midi name playing
        /// </summary>
        public string CurrentMidi;

        public bool IsPositionByInstrument = false;

        /// <summary>
        /// Call when this component is started. 17 TestSpatializerFly will be started, not so obvious to understand. See notes above.
        /// </summary>
        private void Start()
        {
            //Debug.Log($"Start TestSpatializerFly {midiFilePlayer.MPTK_DedicatedChannel}");
            if (midiFilePlayer.MPTK_DedicatedChannel < 0)
            {
                // Start of the main TestSpatializerFly, disable the sphere
                sphere.gameObject.SetActive(false);
            }
            else
            {
                // Start of each MidiSynth when instaciated, one by channel (16)
                if (MidiFilePlayer.SpatialSynths == null)
                {
                    Debug.LogWarning($"IsMidiSpatializer must be set to true");
                    return;
                }

                // Initial position of the sphere. This event could be set with Unity Editor.
                PosSynth = sphere.position;

                // Find a random position on the scene for this gameObject
                RandomPosition(midiFilePlayer);

                // Event trigger for each group of notes read from midi file. When the main MidiSynth reads a group of notes, 
                // the notes are dispatched to each of the 16 midi synth, MidiReadEvents is called.
                if (!midiFilePlayer.OnEventNotesMidi.HasEvent())
                {
                    // Set event by script
                    midiFilePlayer.OnEventNotesMidi.AddListener(MidiReadEvents);
                }
            }

            // Event triggers when a Midi playing is started. This event could be set with Unity Editor.
            // Set for the main MidiSynth and each of the 16 midi synth, MidiStartPlay is called.
            if (!midiFilePlayer.OnEventStartPlayMidi.HasEvent())
            {
                // Set event by script
                midiFilePlayer.OnEventStartPlayMidi.AddListener(MidiStartPlay);
            }
        }

        /// <summary>
        /// Set random position of the MidiSynths from the UI. This is call only from the main MidiSynth (the reader). 
        /// The position must be applied to the 16 others MidiSynth (midi players)
        /// </summary>
        public void ArrangeRandom()
        {
            IsPositionByInstrument = false;
            //Debug.Log($"ArrangeRandom {midiFilePlayer.MPTK_DedicatedChannel}");
            // for each MidiSynth / Channel
            foreach (MidiFilePlayer mfp in MidiFilePlayer.SpatialSynths)
                RandomPosition(mfp);
        }

        /// <summary>
        /// Set random position for a MidiSynth. This is called only from the main MidiSynth (the reader) which applied to the 16 others MidiSynth (midi players).
        /// </summary>
        private int RandomPosition(MidiFilePlayer mfp)
        {
            float range = 950f;
            int maxTry = 0;
            TestSpatializerFly tsf = mfp.gameObject.GetComponent<TestSpatializerFly>();
            // Find random position with a distance with each others, social distancing ;-)
            while (maxTry < 100) // avoid infinite loop !
            {
                Vector3 posTry = new Vector3(UnityEngine.Random.Range(-range, range), tsf.PosSynth.y, UnityEngine.Random.Range(0, range));
                bool posOk = CheckOtherMFPPosition(posTry);

                if (posOk || maxTry >= 99)
                {
                    //if (!posOk) Debug.Log($"Force position {midiFilePlayer.DedicatedChannel}");
                    tsf.PosSynth = posTry;
                    break;
                }
                maxTry++;
            }

            return maxTry;
        }

        private static bool CheckOtherMFPPosition(Vector3 posTry)
        {
            bool posOk = true;
            foreach (MidiFilePlayer mfp in MidiFilePlayer.SpatialSynths)
            {
                if (Vector3.Distance(posTry, mfp.transform.position) < 200f)
                {
                    // Position too close another object
                    posOk = false;
                    break;
                }
            }

            return posOk;
        }

        /// <summary>
        /// Arrange the players in a line from channel 0 to 15 
        /// </summary>
        /// <param name="fromUI"></param>
        public void ArrangeInLine(bool fromUI)
        {
            IsPositionByInstrument = false;
            //Debug.Log($"ArrangeInLine {midiFilePlayer.MPTK_DedicatedChannel}");
            if (!fromUI)
            {
                // Useful if called at start for each TestSpatializerFly instanciated
                TestSpatializerFly tsf = midiFilePlayer.gameObject.GetComponent<TestSpatializerFly>();
                tsf.PosSynth = new Vector3((midiFilePlayer.MPTK_DedicatedChannel * 118) - 950, tsf.PosSynth.y, 0f);
            }
            else
            {
                // Exec from the UI, applied to each MidiFilePlayer (MidiSynth)
                foreach (MidiFilePlayer mfp in MidiFilePlayer.SpatialSynths)
                {
                    TestSpatializerFly tsf = mfp.gameObject.GetComponent<TestSpatializerFly>();
                    tsf.PosSynth = new Vector3((mfp.MPTK_DedicatedChannel * 118) - 950, tsf.PosSynth.y, 0f);
                }
            }
        }

        /// <summary>
        /// Arrange each players depending of the current instrument associated to the channel
        /// </summary>
        /// <param name="fromUI"></param>
        public void ArrangeByInstrument()
        {
            IsPositionByInstrument = true;
            //Debug.Log($"ArrangeByInstrument {midiFilePlayer.MPTK_DedicatedChannel}");
            // Exec from the UI, applied to each MidiFilePlayer (MidiSynth)
            foreach (MidiFilePlayer mfp in MidiFilePlayer.SpatialSynths)
            {
                TestSpatializerFly tsf = mfp.gameObject.GetComponent<TestSpatializerFly>();
                int preset = mfp.MPTK_ChannelPresetGetIndex(mfp.MPTK_DedicatedChannel);
                tsf.PosSynth = PositionByInstrument(tsf.PosSynth, preset, mfp.MPTK_DedicatedChannel);
            }
        }

        /// <summary>
        /// Calculate the position based on the GM Instrument Families, see here: http://www.music.mcgill.ca/~ich/classes/mumt306/StandardMIDIfileformat.html
        /// Try to deploy instrument in inverse V
        /// 1-8	    Piano	
        /// 9-16	Chromatic Percussion	
        /// 17-24	Organ	
        /// 25-32	Guitar	
        /// 33-40	Bass	
        /// 41-48	Strings	
        /// 49-56	Ensemble	
        /// 57-64	Brass	
        /// 65-72	Reed
        /// 73-80	Pipe
        /// 81-88	Synth Lead
        /// 89-96	Synth Pad
        /// 97-104	Synth Effects
        /// 105-112	Ethnic
        /// 113-120	Percussive
        /// 121-128	Sound Effects            /// 
        /// </summary>
        /// <param name="preset"></param>
        /// <returns></returns>
        Vector3 PositionByInstrument(Vector3 posSynth, int preset, int channel)
        {
            float range = 950f;
            float x, z;
            if (channel != 9)
            {
                // left to right
                x = Mathf.Lerp(-range, range, (float)preset / 127f);
                // at left:ahead, center:bottom, at right:ahead
                z = preset < 64 ? z = Mathf.Lerp(0, range, (float)preset / 64f) : Mathf.Lerp(range, 0, ((float)preset - 64f) / 64f);
            }
            else
            {
                // Special case for drum: set to center at bottom
                x = 0f;
                z = range;
            }
            return new Vector3(x, posSynth.y, z);
        }

        /// <summary>
        /// Called when a Midi is started. Run for each MidiSynth, so MidiFilePlayer, so TestSpatializeFly
        /// </summary>
        /// <param name="midiname"></param>
        public void MidiStartPlay(string midiname)
        {
            if (midiFilePlayer.MPTK_DedicatedChannel < 0) // to avoid log from each channel synth
            {
                // The main TestSpatializerFly is used to the UI
                Debug.Log($"Start Playing {midiname}");
                textInfo.text = midiname + "\nCtrl + Mouse to move";
            }
            CurrentMidi = midiname;
            PosSynth.y = -150f;
            countNote = 0;
        }

        /// <summary>
        /// Event fired by MidiFilePlayer when a midi notes are available (set by Unity Editor in MidiFilePlayer Inspector)
        /// </summary>
        public void MidiReadEvents(List<MPTKEvent> events)
        {
            foreach (MPTKEvent midievent in events)
            {
                switch (midievent.Command)
                {
                    case MPTKCommand.NoteOn:
                        //Debug.LogFormat($"Receive in IdSynth:{midiFilePlayer.IdSynth} {midievent.ToString()} countNote:{countNote} Channel:{midievent.Channel} DedicatedChannel:{midiFilePlayer.DedicatedChannel}");
                        // If sphere is not visible, set to a visible position else just increase y position
                        PosSynth.y = PosSynth.y < 0f ? PosSynth.y = 200f : PosSynth.y + 90f * midievent.Duration * 1000f;
                        countNote++;
                        break;
                    case MPTKCommand.PatchChange:
                        if (IsPositionByInstrument)
                        {
                            Debug.LogFormat($"PatchChange in IdSynth:{midiFilePlayer.IdSynth} {midievent.ToString()} Channel:{midievent.Channel} DedicatedChannel:{midiFilePlayer.MPTK_DedicatedChannel}");
                            PosSynth = PositionByInstrument(PosSynth, midievent.Value, midiFilePlayer.MPTK_DedicatedChannel);
                        }
                        break;
                    case MPTKCommand.ControlChange:
                        //Debug.LogFormat($"Receive in IdSynth:{midiFilePlayer.IdSynth} {midievent.ToString()} countNote:{countNote} Channel:{midievent.Channel} DedicatedChannel:{midiFilePlayer.MPTK_DedicatedChannel}");
                        break;
                    case MPTKCommand.MetaEvent:
                        break;
                }
            }
        }

        Vector3 velocitySynth = Vector3.zero; // for the smooth movement
        void Update()
        {
            if (midiFilePlayer != null)
            {
                // Apply simplified gravity (no acceleration, linear speed)
                PosSynth.y -= Time.deltaTime * 100f;

                // Limit the position of the sphare
                PosSynth.y = Mathf.Clamp(PosSynth.y, -150f, 500f);

                // Smooth movement of the sphere+text
                transform.position = Vector3.SmoothDamp(transform.position, PosSynth, ref velocitySynth, 0.9f);

                // Because Test Mesh are always visible, even behind a plane ;-) we need to disable text if sphere is under the ground
                if (transform.position.y < -20f)
                {
                    textPlayer.gameObject.SetActive(false);
                }
                else
                {
                    //if (midiFilePlayer.MPTK_DedicatedChannel == 1) Debug.Log($"{transform.position.y}");
                    textPlayer.gameObject.SetActive(true);
                    // Rotation 10 turn (360 deg) / minute (10)
                    textPlayer.transform.Rotate(new Vector3(0f, (Time.deltaTime * 360f) / 10f, 0f));
                    textPlayer.text = midiFilePlayer.MPTK_DedicatedChannel != 9 ?
                         $"{midiFilePlayer.MPTK_ChannelPresetGetName(midiFilePlayer.MPTK_DedicatedChannel)}" :
                        "Drums"; // Default preset name is standard, not really useful.

                    if (DisplayInfo)
                        textPlayer.text += $"\nP:{midiFilePlayer.MPTK_ChannelPresetGetIndex(midiFilePlayer.MPTK_DedicatedChannel)} C:{midiFilePlayer.MPTK_DedicatedChannel} N:{countNote}";
                }
            }
        }
    }
}