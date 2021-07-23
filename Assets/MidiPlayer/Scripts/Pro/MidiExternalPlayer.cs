
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine.Events;
using System.Net;
using MEC;

namespace MidiPlayerTK
{
    /// <summary>
    /// [MPTK PRO] - Script associated to the prefab MidiExternalPlayer..
    /// Play a midi file from a path on the local deskop or from a web site. 
    /// There is no need to writing a script. For a simple usage, all the job can be done in the prefab inspector.
    ///! @code
    /// // Example of script. See TestMidiExternalPlayer.cs for a more detailed usage.
    /// // Need for a reference to the Prefab (to be set in the hierarchy or can be done by script)
    /// MidiExternalPlayer midiExternalPlayer;
    /// 
    /// if (midiExternalPlayer==null)  
    ///    Debug.LogError("TestMidiExternalPlayer: there is no MidiExternalPlayer Prefab set in Inspector.");
    ///    
    /// midiExternalPlayer.MPTK_MidiName = "http://www.midiworld.com/midis/other/c2/bolero.mid";
    /// midiExternalPlayer.MPTK_Play();
    ///! @endcode
    /// </summary>
    [HelpURL("https://paxstellar.fr/midi-external-player-v2/")]
    public class MidiExternalPlayer : MidiFilePlayer
    {
        /// <summary>
        /// Full path to Midi file or URL to play. Must start with file:// or http:// or https://.
        /// </summary>
        public new string MPTK_MidiName
        {
            get
            {
                return pathmidiNameToPlay;
            }
            set
            {
                pathmidiNameToPlay = value.Trim();
            }
        }
        [SerializeField]
        [HideInInspector]
        private string pathmidiNameToPlay;

        protected new void Awake()
        {
            //Debug.Log("Awake MidiExternalPlayer:" + MPTK_IsPlaying + " " + MPTK_PlayOnStart + " " + MPTK_IsPaused);
            base.AwakeMidiFilePlayer(); //V2.83
            //base.Awake(); 
        }

        protected new void Start()
        {
            //Debug.Log("Start MidiExternalPlayer:" + MPTK_IsPlaying + " " + MPTK_PlayOnStart + " " + MPTK_IsPaused);
            base.StartMidiFilePlayer(); //V2.83
            //try
            //{
            //    if (MPTK_PlayOnStart)
            //    {
            //        MPTK_Play();
            //    }
            //}
            //catch (System.Exception ex)
            //{
            //    MidiPlayerGlobal.ErrorDetail(ex);
            //}
        }

        /// <summary>
        /// Play the midi file defined in MPTK_MidiName
        /// </summary>
        public override void MPTK_Play()
        {
            try
            {
                if (MidiPlayerGlobal.MPTK_SoundFontLoaded)
                {
                    playPause = false;

                    if (!MPTK_IsPlaying)
                    {
                        MPTK_InitSynth();
                        MPTK_StartSequencerMidi();

                        if (string.IsNullOrEmpty(pathmidiNameToPlay))
                            Debug.Log("MPTK_Play: set MPTK_MidiName or Midi Url/path in inspector before playing");
                        else if (!pathmidiNameToPlay.ToLower().StartsWith("file://") &&
                                 !pathmidiNameToPlay.ToLower().StartsWith("http://") &&
                                 !pathmidiNameToPlay.ToLower().StartsWith("https://"))
                            Debug.LogWarning("MPTK_MidiName must start with file:// or http:// or https:// - found: '" + pathmidiNameToPlay + "'");
                        else
                            Timing.RunCoroutine(TheadLoadDataAndPlay());
                    }
                    else
                        Debug.LogWarning("Already playing - " + pathmidiNameToPlay);
                }
                else
                    Debug.LogWarning("Soundfont not loaded");
            }

            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Load midi file in background and play
        /// </summary>
        /// <returns></returns>
        private IEnumerator<float> TheadLoadDataAndPlay()
        {
            //for TU
            //pathmidiNameToPlay = @"C:\Users\Thierry\Desktop\BIM\Sound\Midi\Bach The Art of Fugue - No1.mid";
            //pathmidiNameToPlay = "http://www.midishrine.com/midipp/ngc/Animal_Crossing/kk_ballad.mid";
            //pathmidiNameToPlay = "http://www.midiworld.com/download/4000";
            //pathmidiNameToPlay = "http://www.midiworld.com/midis/other/bach/bwv1060b.mid";
            //http://www.midishrine.com/midipp/n64/Zelda_64_-_The_Ocarina_of_Time/kakariko.mid

            // Deprecated method        
            //            // Asynchrone loading of the midi file
            //            using (WWW www = new WWW(pathmidiNameToPlay))
            //            {
            //                //yield return www;
            //                yield return Timing.WaitUntilDone(www);
            //                if (www.bytes != null && www.bytes.Length > 4 && System.Text.Encoding.Default.GetString(www.bytes, 0, 4) == "MThd")
            //                {
            //                    // Start playing
            //                    if (MPTK_CorePlayer)
            //                        Timing.RunCoroutine(ThreadCorePlay(www.bytes).CancelWith(gameObject));
            //                    else
            //                        Timing.RunCoroutine(ThreadPlay(www.bytes).CancelWith(gameObject));
            //                }
            //                else
            //                    Debug.LogWarning("Midi not find or not a Midi file - " + pathmidiNameToPlay);
            //            }

            using (UnityEngine.Networking.UnityWebRequest req = UnityEngine.Networking.UnityWebRequest.Get(pathmidiNameToPlay))
            {
                yield return Timing.WaitUntilDone(req.SendWebRequest());
                if (!req.isNetworkError)
                {
                    byte[] data = req.downloadHandler.data;
                    if (data != null && data.Length > 4 && System.Text.Encoding.Default.GetString(data, 0, 4) == "MThd")
                    {
                        // Start playing
                        if (MPTK_CorePlayer)
                            Timing.RunCoroutine(ThreadCorePlay(data).CancelWith(gameObject));
                        else
                            Timing.RunCoroutine(ThreadPlay(data).CancelWith(gameObject));
                    }
                    else
                        Debug.LogWarning("Midi not find or not a Midi file - " + pathmidiNameToPlay);
                }
                else
                    Debug.LogWarning("Network error - " + pathmidiNameToPlay);
            }
        }

        /// <summary>
        /// Not applicable for external
        /// </summary>
        public new MidiLoad MPTK_Load()
        {
            return null;
        }

        /// <summary>
        /// Not applicable for external
        /// </summary>
        public new int MPTK_MidiIndex
        {
            get
            {
                Debug.LogWarning("MPTK_MidiIndex not available for MidiExternalPlayer");
                return -1;
            }
            set
            {
                Debug.LogWarning("MPTK_MidiIndex not available for MidiExternalPlayer");
            }
        }

        /// <summary>
        /// Not applicable for external
        /// </summary>
        public new void MPTK_Next()
        {
            try
            {
                Debug.LogWarning("MPTK_Next not available for MidiExternalPlayer");
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Not applicable for external
        /// </summary>
        public new void MPTK_Previous()
        {
            try
            {
                Debug.LogWarning("MPTK_Next not available for MidiExternalPlayer");
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}

