//#define DEBUGPERF
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine.Events;
using MEC;
using System.Runtime.InteropServices;

namespace MidiPlayerTK
{
    /// <summary>
    /// [MPTK PRO] - Script associated to the prefab MidiInReader. 
    /// Read Midi events from a Midi keyboard connected your device (Windows 10 or MacOS). See example of use in TestMidiInputScripting.cs
    /// There is no need to writing a script. For a simple usage, all the job can be done in the prefab inspector.
    ///! @code
    /// // Example of script. See TestMidiInputScripting.cs for a more detailed usage.
    /// // Need for a reference to the Prefab (can also be set from the hierarchy)
    /// MidiInReader midiIn = FindObjectOfType<MidiInReader>();
    /// 
    /// if (midiIn == null) 
    ///     Debug.Log("Can't find a MidiInReader Prefab in the Hierarchy. No events will be read");
    ///     
    /// // There is two methods to trigger event: in inpector from the Unity editor or by script
    /// midiIn.OnEventInputMidi.AddListener((MPTKEvent evt) => 
    /// {
    ///     // your processing here
    ///     Debug.Log(evt.ToString());
    /// });
    ///! @endcode
    /// </summary>
    [HelpURL("https://paxstellar.fr/prefab-midiinreader/")]
    [RequireComponent(typeof(AudioSource))]
    public class MidiInReader : MidiSynth
    {
        /// <summary>
        /// Read Midi input
        /// </summary>
        public bool MPTK_ReadMidiInput;

        /// <summary>
        /// Log midi events
        /// </summary>
        public bool MPTK_LogEvents;

        public int MPTK_CountEndpoints
        {
            get
            {
                //Debug.Log("MPTK_CountEndpoints:" + CountEndpoints().ToString());
                return CountEndpoints();
            }
        }

        public string MPTK_GetEndpointDescription(int index)
        {
            uint id = GetEndpointIdAtIndex(index);
            return string.Format("id:{0} name:{1}", id, Marshal.PtrToStringAnsi(MidiJackGetEndpointName(id)));
        }

        /// <summary>
        /// Define unity event to trigger when note available from the Midi file.
        ///! @code
        /// MidiInReader midiFilePlayer = FindObjectOfType<MidiInReader>(); 
        ///         ...
        /// if (!midiFilePlayer.OnEventInputMidi.HasEvent())
        /// {
        ///    // No listener defined, set now by script. NotesToPlay will be called for each new notes read from Midi file
        ///    midiFilePlayer.OnEventInputMidi.AddListener(NotesToPlay);
        /// }
        ///         ...
        /// public void NotesToPlay(MPTKEvent notes)
        /// {
        ///    Debug.Log(notes.Value);
        ///    foreach (MPTKEvent midievent in notes)
        ///    {
        ///         ...
        ///    }
        /// }
        ///! @endcode
        /// </summary>
        [HideInInspector]
        public EventMidiClass OnEventInputMidi;

        [DllImport("MidiJackPlugin", EntryPoint = "MidiJackDequeueIncomingData")]
        static extern ulong DequeueIncomingData();

        [DllImport("MidiJackPlugin", EntryPoint = "MidiJackCountEndpoints")]
        static extern int CountEndpoints();

        [DllImport("MidiJackPlugin", EntryPoint = "MidiJackGetEndpointIDAtIndex")]
        static extern uint GetEndpointIdAtIndex(int index);

        [DllImport("MidiJackPlugin")]
        static extern System.IntPtr MidiJackGetEndpointName(uint id);

        new void Awake()
        {
            base.Awake();
        }

        new void Start()
        {
            try
            {
                MidiInReader[] list = FindObjectsOfType<MidiInReader>();
                if (list.Length > 1)
                {
                    Debug.LogWarning("No more than one MidiInReader must be present in your hierarchy, we found " + list.Length + " MidiInReader.");
                }
                MPTK_InitSynth();
                base.Start();
                // Always enabled for midi stream
                MPTK_EnablePresetDrum = true;
                ThreadDestroyAllVoice();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        void Update()
        {
            int count = 0;
            try
            {
                // Process the message queue and avoid locking Unity
                while (MPTK_ReadMidiInput && count < 100)
                {
                    count++;

                    // Pop from the queue.
                    ulong data = DequeueIncomingData();
                    if (data == 0) break;

                    // Parse the message.
                    MPTKEvent midievent = new MPTKEvent(data);

                    // Active Sensing. This message is intended to be sent repeatedly to tell the receiver that a connection is alive
                    if (midievent.Command == MPTKCommand.AutoSensing) continue;

                    // Call event with these midi events
                    try
                    {
                        if (OnEventInputMidi != null)
                            OnEventInputMidi.Invoke(midievent);
                    }
                    catch (System.Exception ex)
                    {
                        MidiPlayerGlobal.ErrorDetail(ex);
                    }

                    if (MPTK_DirectSendToPlayer)
                        PlayEvent(midievent);

                    if (MPTK_LogEvents)
                        Debug.Log(midievent.ToString());
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}

