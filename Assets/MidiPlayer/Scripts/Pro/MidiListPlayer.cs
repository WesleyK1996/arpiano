
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
    /// <summary>
    /// [MPTK PRO] -  Script for the prefab MidiListPlayer. 
    /// Play a list of pre-selected midi file from the dedicated inspector.
    /// List of Midi files must exists in MidiDB. See Midi Player Setup (Unity menu MPTK).
    /// </summary>
    [HelpURL("https://paxstellar.fr/midi-list-player-v2/")]
    public class MidiListPlayer : MonoBehaviour
    {
        /// <summary>
        /// Define a midi to be added in the list
        /// </summary>
        [Serializable]
        public class MPTK_MidiPlayItem
        {
            /// <summary>
            /// Midi Name. Use the exact name defined in Unity resources folder MidiDB without any path or extension.
            /// Tips: Add Midi files to your project with the Unity menu MPTK or add it directly in the ressource folder and open Midi File Setup to automatically integrate Midi in MPTK.
            /// </summary>
            public string MidiName;

            /// <summary>
            /// Select or unselect this Midi in the Inspector to apply actions (reorder, delete, ...) NO MORE USED
            /// </summary>
            public bool UIAction;

            /// <summary>
            /// Select or unselect this Midi to be played in the list ...)
            /// </summary>
            public bool Selected;

            /// <summary>
            /// Position of the Midi in the list. Use method MPTK_ReIndexMidi() recalculate the index.
            /// </summary>
            public int Index;

            /// <summary>
            /// Time (ms) position where to start playing the midi file
            /// </summary>
            public float StartFrom;

            /// <summary>
            /// Time (ms) position where to end playing the midi file
            /// </summary>
            public float EndFrom;

            //! @cond NODOC
            /// <summary>
            /// value set by MPTK, don't change anything
            /// </summary>
            public long LastTick;
            /// <summary>
            /// value set by MPTK, don't change anything
            /// </summary>
            public float RealDurationMs;
            /// <summary>
            /// value set by MPTK, don't change anything
            /// </summary>
            public double TickLengthMs;
            //! @endcond

            override public string ToString()
            {
                return string.Format("{0} Index:{1} {2} StartFrom:{3} EndFrom:{4} LastTick:{5} RealDurationMs:{6:F3} TickLengthMs:{7:F3} ", MidiName, Index, Selected, StartFrom, EndFrom, LastTick, RealDurationMs, TickLengthMs);
            }
        }

        public enum enStatusPlayer
        {
            Starting,
            Playing,
            Ending,
            Stopped
        }

        //! @cond NODOC
        /// <summary>
        /// Internal class
        /// </summary>
        [Serializable]
        public class MidiListPlayerStatus
        {
            public MidiFilePlayer MPTK_MidiFilePlayer;
            public enStatusPlayer StatusPlayer;
            public float EndAt;
            public float Volume;
            public float PctVolume;
            public MidiListPlayerStatus()
            {
                //PctVolume = 100f;
                StatusPlayer = enStatusPlayer.Stopped;
            }
            public void UpdateVolume()
            {
                //if (StatusPlayer != enStatusPlayer.Stopped)
                if (MPTK_MidiFilePlayer != null)
                    MPTK_MidiFilePlayer.MPTK_Volume = Volume * PctVolume;
            }
        }

        [HideInInspector]
        public bool showDefault;

        //! @endcond

        /// <summary>
        /// Volume of midi playing. 
        /// Must be >=0 and <= 1
        /// </summary>
        public float MPTK_Volume
        {
            get { return volume; }
            set
            {
                if (volume >= 0f && volume <= 1f)
                {
                    SetVolume(value);
                }
                else
                    Debug.LogWarning("MidiListPlayer - Set Volume value not valid : " + value);
            }
        }

        private void SetVolume(float value)
        {
            volume = value;
            MPTK_MidiFilePlayer_1.Volume = volume;
            MPTK_MidiFilePlayer_1.UpdateVolume();
            MPTK_MidiFilePlayer_2.Volume = volume;
            MPTK_MidiFilePlayer_2.UpdateVolume();
        }

        [SerializeField]
        [HideInInspector]
        private float volume = 0.5f;

        /// <summary>
        /// Play list
        /// </summary>
        public List<MPTK_MidiPlayItem> MPTK_PlayList;

        /// <summary>
        /// Play a specific Midi in the list.
        /// </summary>
        public int MPTK_PlayIndex
        {
            get { return playIndex; }
            set
            {
                if (MPTK_MidiFilePlayer_1 != null && MPTK_MidiFilePlayer_2 != null)
                {
                    if (MPTK_PlayList == null || MPTK_PlayList.Count == 0)
                        Debug.LogWarning("No Play List defined");
                    else if (value < 0 || value >= MPTK_PlayList.Count)
                        Debug.LogWarning("Index to play " + value + " not correct");
                    else
                    {
                        playIndex = value;
                        //Debug.Log("PlayIndex: Index to play " + playIndex + " " + MPTK_PlayList[playIndex].MidiName);
                        MidiListPlayerStatus mps1 = GetFirstAvailable;
                        //Debug.Log("PlayIndex: Play on " + mps1.MPTK_MidiFilePlayer.name);

                        if (mps1 != null && mps1.MPTK_MidiFilePlayer != null)
                        {
                            mps1.PctVolume = 0f;
                            mps1.UpdateVolume();
                            mps1.MPTK_MidiFilePlayer.MPTK_MidiName = MPTK_PlayList[playIndex].MidiName;
                            if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                            {
                                mps1.MPTK_MidiFilePlayer.MPTK_UnPause();
                                mps1.MPTK_MidiFilePlayer.MPTK_Position = 0d;

                                if (!mps1.MPTK_MidiFilePlayer.MPTK_IsPlaying)
                                {
                                    // Load description of available soundfont
                                    if (MidiPlayerGlobal.ImSFCurrent != null && MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                                    {
                                        mps1.MPTK_MidiFilePlayer.MPTK_InitSynth();
                                        mps1.MPTK_MidiFilePlayer.MPTK_StartSequencerMidi();
                                        //if (VerboseSynth)Debug.Log(MPTK_MidiName);
                                        if (string.IsNullOrEmpty(mps1.MPTK_MidiFilePlayer.MPTK_MidiName))
                                            mps1.MPTK_MidiFilePlayer.MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[0];
                                        int selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == mps1.MPTK_MidiFilePlayer.MPTK_MidiName);
                                        if (selectedMidi < 0)
                                        {
                                            Debug.LogWarning("MidiFilePlayer - MidiFile " + mps1.MPTK_MidiFilePlayer.MPTK_MidiName + " not found. Try with the first in list.");
                                            selectedMidi = 0;
                                            mps1.MPTK_MidiFilePlayer.MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[0];
                                        }

                                        float endAt = MPTK_PlayList[MPTK_PlayIndex].EndFrom;

                                        if (endAt <= 0)
                                            endAt = MPTK_PlayList[MPTK_PlayIndex].RealDurationMs;

                                        // If end before start, set end after with 2 * overlay (time to up and time to down)
                                        if (endAt < MPTK_PlayList[MPTK_PlayIndex].StartFrom + 2f * MPTK_OverlayTimeMS)
                                            endAt = MPTK_PlayList[MPTK_PlayIndex].StartFrom + 2f * MPTK_OverlayTimeMS;
                                        mps1.EndAt = endAt;
                                        //Debug.Log("start play " + mps1.MPTK_MidiFilePlayer.MPTK_MidiName + " from " + MPTK_PlayList[playIndex].StartFrom + " to " + endAt);

                                        if (mps1.MPTK_MidiFilePlayer.MPTK_CorePlayer)
                                            Timing.RunCoroutine(mps1.MPTK_MidiFilePlayer.ThreadCorePlay(null,
                                                MPTK_PlayList[playIndex].StartFrom, endAt).CancelWith(gameObject), Segment.Update);
                                        else
                                            Timing.RunCoroutine(mps1.MPTK_MidiFilePlayer.ThreadPlay(null,
                                                MPTK_PlayList[playIndex].StartFrom, endAt).CancelWith(gameObject), Segment.Update);
                                    }
                                    else
                                        Debug.LogWarning(MidiPlayerGlobal.ErrorNoMidiFile);
                                }
                            }

                            mps1.StatusPlayer = enStatusPlayer.Starting;

                            MidiListPlayerStatus mps2 = mps1 == MPTK_MidiFilePlayer_1 ? MPTK_MidiFilePlayer_2 : MPTK_MidiFilePlayer_1;
                            if (mps2.StatusPlayer != enStatusPlayer.Stopped)
                            {
                                // Set to ending phase
                                mps2.EndAt = (float)mps2.MPTK_MidiFilePlayer.MPTK_Position + MPTK_OverlayTimeMS;
                                mps2.StatusPlayer = enStatusPlayer.Ending;
                            }
                        }
                    }
                }
            }
        }

        private MidiListPlayerStatus GetFirstAvailable
        {
            get
            {
                if (MPTK_MidiFilePlayer_1.StatusPlayer == enStatusPlayer.Stopped)
                    return MPTK_MidiFilePlayer_1;
                if (MPTK_MidiFilePlayer_2.StatusPlayer == enStatusPlayer.Stopped)
                    return MPTK_MidiFilePlayer_2;
                if (MPTK_MidiFilePlayer_1.StatusPlayer == enStatusPlayer.Ending)
                    return MPTK_MidiFilePlayer_1;
                if (MPTK_MidiFilePlayer_2.StatusPlayer == enStatusPlayer.Ending)
                    return MPTK_MidiFilePlayer_2;
                return null;
            }
        }

        public MidiListPlayerStatus MPTK_GetPlaying
        {
            get
            {
                if (MPTK_MidiFilePlayer_1 != null && MPTK_MidiFilePlayer_1.StatusPlayer == enStatusPlayer.Playing)
                    return MPTK_MidiFilePlayer_1;
                if (MPTK_MidiFilePlayer_2 != null && MPTK_MidiFilePlayer_2.StatusPlayer == enStatusPlayer.Playing)
                    return MPTK_MidiFilePlayer_2;
                return null;
            }
        }

        public MidiListPlayerStatus MPTK_GetStarting
        {
            get
            {
                if (MPTK_MidiFilePlayer_1.StatusPlayer == enStatusPlayer.Starting)
                    return MPTK_MidiFilePlayer_1;
                if (MPTK_MidiFilePlayer_2.StatusPlayer == enStatusPlayer.Starting)
                    return MPTK_MidiFilePlayer_2;
                return null;
            }
        }

        public MidiListPlayerStatus MPTK_GetEnding
        {
            get
            {
                if (MPTK_MidiFilePlayer_1.StatusPlayer == enStatusPlayer.Ending)
                    return MPTK_MidiFilePlayer_1;
                if (MPTK_MidiFilePlayer_2.StatusPlayer == enStatusPlayer.Ending)
                    return MPTK_MidiFilePlayer_2;
                return null;
            }
        }

        public MidiListPlayerStatus MPTK_GetPausing
        {
            get
            {
                if (MPTK_MidiFilePlayer_1.MPTK_MidiFilePlayer.MPTK_IsPaused)
                    return MPTK_MidiFilePlayer_1;
                if (MPTK_MidiFilePlayer_2.MPTK_MidiFilePlayer.MPTK_IsPaused)
                    return MPTK_MidiFilePlayer_2;
                return null;
            }
        }
        /// <summary>
        /// Should the Midi start playing when application start ?
        /// </summary>
        public bool MPTK_PlayOnStart { get { return playOnStart; } set { playOnStart = value; } }

        /// <summary>
        /// Should automatically restart when Midi reach the end ?
        /// </summary>
        public bool MPTK_Loop { get { return loop; } set { loop = value; } }

        /// <summary>
        /// Set or Get midi position time from 0 to lenght time of midi playing (in millisecond). No effect if the Midi is not playing.
        ///! @code
        /// // Be carefull when modifying position on fly from GUI. 
        /// // Each change generates 0.2s of pause, avoid little and frequent position change. 
        /// // Below change is applied only above 2 decimals.
        /// double currentPosition = Math.Round(midiFilePlayer.MPTK_Position / 1000d, 2);
        /// double newPosition = Math.Round(GUILayout.HorizontalSlider((float)currentPosition, 0f, (float)midiFilePlayer.MPTK_RealDuration.TotalSeconds, GUILayout.Width(buttonWidth)), 2);
        /// if (newPosition != currentPosition)
        /// {
        ///    Debug.Log("New position " + currentPosition + " --> " + newPosition );
        ///    midiFilePlayer.MPTK_Position = newPosition * 1000d;
        ///  }
        ///! @endcode
        /// </summary>
        public double MPTK_Position
        {
            get
            {
                if (MPTK_GetPlaying != null)
                    return MPTK_GetPlaying.MPTK_MidiFilePlayer.MPTK_Position;
                else
                    return 0d;
            }
            set
            {
                if (MPTK_GetPlaying != null)
                    MPTK_GetPlaying.MPTK_MidiFilePlayer.MPTK_Position = value;
            }
        }

        /// <summary>
        /// Last tick position in Midi: Value of the tick for the last midi event in sequence expressed in number of "ticks". MPTK_TickLast / MPTK_DeltaTicksPerQuarterNote equal the duration time of a quarter-note regardless the defined tempo.
        /// </summary>
        public long MPTK_TickLast
        {
            get
            {
                if (MPTK_GetPlaying != null)
                    return MPTK_GetPlaying.MPTK_MidiFilePlayer.MPTK_TickLast;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Current tick position in Midi: Time of the current midi event expressed in number of "ticks". MPTK_TickCurrent / MPTK_DeltaTicksPerQuarterNote equal the duration time of a quarter-note regardless the defined tempo.
        /// </summary>
        public long MPTK_TickCurrent
        {
            get
            {
                if (MPTK_GetPlaying != null)
                    return MPTK_GetPlaying.MPTK_MidiFilePlayer.MPTK_TickCurrent;
                else
                    return 0;
            }
            set
            {
                if (MPTK_GetPlaying != null)
                    MPTK_GetPlaying.MPTK_MidiFilePlayer.MPTK_TickCurrent = value;
            }
        }

        /// <summary>
        /// Duration of the midi. This duration can change during the playing when Change Tempo Event are processed.
        /// </summary>
        public TimeSpan MPTK_Duration
        {
            get
            {
                if (MPTK_GetPlaying != null)
                    return MPTK_GetPlaying.MPTK_MidiFilePlayer.MPTK_Duration;
                else
                    return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Is Midi file playing is paused ?
        /// </summary>
        public bool MPTK_IsPaused
        {
            get
            {
                if (MPTK_GetPlaying != null)
                    return MPTK_GetPlaying.MPTK_MidiFilePlayer.MPTK_IsPaused;
                return false;
            }
        }

        /// <summary>
        /// Is Midi file is playing ?
        /// </summary>
        public bool MPTK_IsPlaying
        {
            get
            {
                return (MPTK_GetPlaying != null || MPTK_GetStarting!=null || MPTK_GetEnding != null);
                //return (MPTK_MidiFilePlayer_1.MPTK_MidiFilePlayer.MPTK_IsPlaying || MPTK_MidiFilePlayer_2.MPTK_MidiFilePlayer.MPTK_IsPlaying);
            }
        }

        /// <summary>
        /// Define unity event to trigger at start
        /// </summary>
        [HideInInspector]
        public EventStartMidiClass OnEventStartPlayMidi;

        /// <summary>
        /// Define unity event to trigger at end
        /// </summary>
        [HideInInspector]
        public EventEndMidiClass OnEventEndPlayMidi;

        /// <summary>
        /// First MidiFilePlayer to play the Midi
        /// </summary>
        /// 
        public MidiListPlayerStatus MPTK_MidiFilePlayer_1;

        /// <summary>
        /// Second MidiFilePlayer to play the Midi
        /// </summary>
        public MidiListPlayerStatus MPTK_MidiFilePlayer_2;

        /// <summary>
        /// Duration of overlay between playing two midi 
        /// </summary>
        public float MPTK_OverlayTimeMS;

        [SerializeField]
        [HideInInspector]
        private bool playOnStart = false, loop = false;

        [SerializeField]
        [HideInInspector]
        private int playIndex;

        void Awake()
        {
            //Debug.Log("Awake midiIsPlaying:" + MPTK_IsPlaying);
            MidiFilePlayer[] mfps = GetComponentsInChildren<MidiFilePlayer>();
            if (mfps == null || mfps.Length != 2)
                Debug.LogWarning("Two MidiFilePlayer components are needed for MidiListPlayer.");
            else
            {
                MPTK_MidiFilePlayer_1 = new MidiListPlayerStatus() { MPTK_MidiFilePlayer = mfps[0] };
                MPTK_MidiFilePlayer_2 = new MidiListPlayerStatus() { MPTK_MidiFilePlayer = mfps[1] };
                MPTK_MidiFilePlayer_1.MPTK_MidiFilePlayer.OnEventStartPlayMidi.AddListener(EventStartPlayMidi);
                MPTK_MidiFilePlayer_2.MPTK_MidiFilePlayer.OnEventStartPlayMidi.AddListener(EventStartPlayMidi);
                MPTK_MidiFilePlayer_1.MPTK_MidiFilePlayer.OnEventEndPlayMidi.AddListener(EventEndPlayMidi);
                MPTK_MidiFilePlayer_2.MPTK_MidiFilePlayer.OnEventEndPlayMidi.AddListener(EventEndPlayMidi);
            }
        }

        public void EventStartPlayMidi(string midiname)
        {
            //Debug.LogFormat("EventEndPlayMidi {0} reason:{1}", midiname);
            OnEventStartPlayMidi.Invoke(midiname);
        }

        public void EventEndPlayMidi(string midiname, EventEndMidiEnum reason)
        {
            //Debug.LogFormat("EventEndPlayMidi {0} reason:{1}", midiname, reason);
            OnEventEndPlayMidi.Invoke(midiname, reason);
        }

        void Start()
        {
            //Debug.Log("Start MPTK_PlayOnStart:" + MPTK_PlayOnStart);
            try
            {
                SetVolume(volume);
                if (MPTK_PlayOnStart)
                {
                    // Find first 
                    foreach (MPTK_MidiPlayItem item in MPTK_PlayList)
                    {
                        //Debug.Log(item.ToString());
                        if (item.Selected)
                        {
                            MPTK_PlayIndex = item.Index;
                            break;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        public void Update()
        {
            MidiListPlayerStatus mpsPlaying = MPTK_GetPlaying;
            if (mpsPlaying != null)
            {
                if (mpsPlaying.MPTK_MidiFilePlayer.MPTK_Position > mpsPlaying.EndAt - MPTK_OverlayTimeMS)
                {
                    //Debug.Log("Time to swap to the next midi");
                    // Time to swap to the next midi
                    if (MPTK_PlayIndex < MPTK_PlayList.Count - 1 || MPTK_Loop)
                        MPTK_Next();
                }
            }

            MidiListPlayerStatus mpsStarting = MPTK_GetStarting;
            if (mpsStarting != null && mpsStarting.MPTK_MidiFilePlayer.MPTK_Position != 0)
            {
                float overlayTime = (float)mpsStarting.MPTK_MidiFilePlayer.MPTK_Position - MPTK_PlayList[MPTK_PlayIndex].StartFrom;
                //Debug.Log("Starting " + mpsStarting.MPTK_MidiFilePlayer.MPTK_MidiName + " overlayTime:" + overlayTime.ToString("F3") + " " + mpsStarting.MPTK_MidiFilePlayer.MPTK_Position.ToString("F3"));
                if (MPTK_OverlayTimeMS > 0f && overlayTime < MPTK_OverlayTimeMS)
                {
                    mpsStarting.PctVolume = overlayTime / MPTK_OverlayTimeMS;
                    mpsStarting.PctVolume = Mathf.Clamp(mpsStarting.PctVolume, 0f, 1f);
                }
                else
                {
                    mpsStarting.PctVolume = 1f;
                    mpsStarting.StatusPlayer = enStatusPlayer.Playing;
                }
            }

            MidiListPlayerStatus mpsEnding = MPTK_GetEnding;
            if (mpsEnding != null)
            {
                float overlayTime = mpsEnding.EndAt - (float)mpsEnding.MPTK_MidiFilePlayer.MPTK_Position;
                //Debug.Log("Ending " + mpsEnding.MPTK_MidiFilePlayer.MPTK_MidiName + " overlayTime:" + overlayTime.ToString("F3") + " MPTK_Position:" + mpsEnding.MPTK_MidiFilePlayer.MPTK_Position.ToString("F3") + " MPTK_IsPlaying:" + mpsEnding.MPTK_MidiFilePlayer.MPTK_IsPlaying);
                if (overlayTime > 0f && MPTK_OverlayTimeMS > 0f && mpsEnding.MPTK_MidiFilePlayer.MPTK_IsPlaying)
                {
                    mpsEnding.PctVolume = overlayTime / MPTK_OverlayTimeMS;
                    mpsEnding.PctVolume = Mathf.Clamp(mpsEnding.PctVolume, 0f, 1f);
                }
                else
                {
                    mpsEnding.PctVolume = 0f;
                    mpsEnding.StatusPlayer = enStatusPlayer.Stopped;
                    mpsEnding.MPTK_MidiFilePlayer.MPTK_Stop();
                }
            }

            MPTK_MidiFilePlayer_1.UpdateVolume();
            MPTK_MidiFilePlayer_2.UpdateVolume();
        }

        /// <summary>
        /// Create an empty list
        /// </summary>
        public void MPTK_NewList()
        {
            MPTK_PlayList = new List<MPTK_MidiPlayItem>();
        }

        /// <summary>
        /// Add a Midi name to the list. Use the exact name defined in Unity resources (folder MidiDB) without any path or extension.
        /// Tips: Add Midi files to your project with the Unity menu MPTK or add it directly in the ressource folder and open Midi File Setup to automatically integrate Midi in MPTK.
        ///! @code
        /// midiListPlayer.MPTK_AddMidi("Albinoni - Adagio");
        /// midiListPlayer.MPTK_AddMidi("Conan The Barbarian", 10000, 20000);
        ///! @endcode
        /// </summary>
        /// <param name="name">midi filename as defined in resources</param>
        /// <param name="start">starting time of playing (ms). Default: start of the midi</param>
        /// <param name="end">endding time of playing (ms). Default: end of midi</param>
        public void MPTK_AddMidi(string name, float start = 0, float end = 0)
        {
            try
            {
                MidiLoad midifile = new MidiLoad();
                if (midifile.MPTK_Load(name))
                {
                    MPTK_PlayList.Add(new MPTK_MidiPlayItem()
                    {
                        MidiName = name,
                        Selected = true,
                        Index = MPTK_PlayList.Count,
                        LastTick = midifile.MPTK_TickLast,
                        RealDurationMs = (float)midifile.MPTK_DurationMS,
                        TickLengthMs = midifile.MPTK_PulseLenght,
                        StartFrom = start,
                        EndFrom = end <= 0f ? (float)midifile.MPTK_DurationMS : end,
                    });
                    //Debug.Log(MPTK_PlayList[MPTK_PlayList.Count - 1].ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("MPTK_AddMidi: " + ex.Message);
            }
        }

        /// <summary>
        /// Remove a Midi name from the list. Use the exact name defined in Unity resources folder MidiDB without any path or extension.
        ///! @code
        /// midiListPlayer.MPTK_RemoveMidi("Albinoni - Adagio");
        ///! @endcode
        /// </summary>
        public void MPTK_RemoveMidi(string name)
        {
            int index = MPTK_PlayList.FindIndex(s => s.MidiName == name);
            if (index >= 0)
                MPTK_PlayList.RemoveAt(index);
            MPTK_ReIndexMidi();
        }

        /// <summary>
        /// Remove a Midi at position from the list..
        ///! @code
        /// midiListPlayer.MPTK_RemoveMidiAt(1);
        ///! @endcode
        /// </summary>
        public void MPTK_RemoveMidiAt(int index)
        {
            if (index >= 0 && index < MPTK_PlayList.Count)
                MPTK_PlayList.RemoveAt(index);
            MPTK_ReIndexMidi();
        }

        /// <summary>
        /// Get description of a play item at position.
        ///! @code
        /// midiListPlayer.MPTK_GetAt(1);
        ///! @endcode
        /// </summary>
        public MPTK_MidiPlayItem MPTK_GetAt(int index)
        {
            if (index >= 0 && index < MPTK_PlayList.Count)
               return MPTK_PlayList[index];
            return null;
        }

        /// <summary>
        /// Recalculate the index of the midi from the list.
        /// </summary>
        public void MPTK_ReIndexMidi()
        {
            int index = 0;
            foreach (MPTK_MidiPlayItem item in MPTK_PlayList)
                item.Index = index++;
        }

        /// <summary>
        /// Play the midi in list at MPTK_PlayIndex position
        /// </summary>
        public void MPTK_Play()
        {
            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    // Load description of available soundfont
                    if (MidiPlayerGlobal.ImSFCurrent != null && MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                        // Force to start the current midi index
                        MPTK_PlayIndex = MPTK_PlayIndex;
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
        /// Stop playing
        /// </summary>
        public void MPTK_Stop()
        {
            try
            {
                if (MPTK_MidiFilePlayer_1 != null)
                {
                    MPTK_MidiFilePlayer_1.MPTK_MidiFilePlayer.MPTK_Stop();
                    MPTK_MidiFilePlayer_1.StatusPlayer = enStatusPlayer.Stopped;
                }
                if (MPTK_MidiFilePlayer_2 != null)
                {
                    MPTK_MidiFilePlayer_2.MPTK_MidiFilePlayer.MPTK_Stop();
                    MPTK_MidiFilePlayer_2.StatusPlayer = enStatusPlayer.Stopped;
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Restart playing the current midi file
        /// </summary>
        public void MPTK_RePlay()
        {
            try
            {
                // Force to play the same index
                MPTK_PlayIndex = MPTK_PlayIndex;

                //if (MPTK_MidiFilePlayer_1 != null && MPTK_MidiFilePlayer_1.StatusPlayer == enStatusPlayer.Stopped)
                //{
                //    MPTK_MidiFilePlayer_1.MPTK_MidiFilePlayer.MPTK_RePlay();
                //    return;
                //}
                //if (MPTK_MidiFilePlayer_2 != null && MPTK_MidiFilePlayer_1.StatusPlayer == enStatusPlayer.Stopped)
                //    MPTK_MidiFilePlayer_2.MPTK_MidiFilePlayer.MPTK_RePlay();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }


        /// <summary>
        /// Pause the current playing
        /// </summary>
        public void MPTK_Pause()
        {
            try
            {
                if (MPTK_MidiFilePlayer_1 != null && MPTK_MidiFilePlayer_2 != null)
                {
                    if (MPTK_MidiFilePlayer_1.StatusPlayer == enStatusPlayer.Playing)
                    {
                        MPTK_MidiFilePlayer_1.MPTK_MidiFilePlayer.MPTK_Pause();
                        return;
                    }
                    if (MPTK_MidiFilePlayer_2.StatusPlayer == enStatusPlayer.Playing)
                        MPTK_MidiFilePlayer_2.MPTK_MidiFilePlayer.MPTK_Pause();
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Pause the current playing
        /// </summary>
        public void MPTK_UnPause()
        {
            try
            {
                if (MPTK_MidiFilePlayer_1 != null && MPTK_MidiFilePlayer_2 != null)
                {
                    if (MPTK_MidiFilePlayer_1.StatusPlayer == enStatusPlayer.Playing && MPTK_MidiFilePlayer_1.MPTK_MidiFilePlayer.MPTK_IsPaused)
                    {
                        MPTK_MidiFilePlayer_1.MPTK_MidiFilePlayer.MPTK_UnPause();
                        return;
                    }
                    if (MPTK_MidiFilePlayer_2.StatusPlayer == enStatusPlayer.Playing && MPTK_MidiFilePlayer_2.MPTK_MidiFilePlayer.MPTK_IsPaused)
                        MPTK_MidiFilePlayer_2.MPTK_MidiFilePlayer.MPTK_UnPause();
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play next Midi in list
        /// </summary>
        public void MPTK_Next()
        {
            try
            {
                if (MPTK_MidiFilePlayer_1 != null && MPTK_MidiFilePlayer_2 != null)
                {
                    if (!MPTK_IsPaused)
                    {
                        int count = 0;
                        int newIndex = MPTK_PlayIndex;
                        bool find = false;
                        while (!find && count < MPTK_PlayList.Count)
                        {

                            if (newIndex < MPTK_PlayList.Count - 1)
                                newIndex++;
                            else
                                newIndex = 0;
                            if (MPTK_PlayList[newIndex].Selected)
                                find = true;
                            count++;
                        }
                        if (find)
                        {
                            MPTK_PlayIndex = newIndex;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play previous Midi in list
        /// </summary>
        public void MPTK_Previous()
        {
            try
            {
                if (MPTK_MidiFilePlayer_1 != null && MPTK_MidiFilePlayer_2 != null)
                {
                    if (!MPTK_IsPaused)
                    {
                        int count = 0;
                        int newIndex = MPTK_PlayIndex;
                        bool find = false;
                        while (!find && count < MPTK_PlayList.Count)
                        {

                            if (newIndex > 0)
                                newIndex--;
                            else
                                newIndex = MPTK_PlayList.Count - 1;
                            if (MPTK_PlayList[newIndex].Selected)
                                find = true;
                            count++;
                        }
                        if (find)
                        {
                            MPTK_PlayIndex = newIndex;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}

