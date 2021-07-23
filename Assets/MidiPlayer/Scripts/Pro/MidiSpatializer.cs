
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
    /// [MPTK PRO] - Script associated to the prefab MidiSpatializer. V2.83.
    /// </summary>
  //  [HelpURL("https://paxstellar.fr/midi-external-player-v2/")]
    public class MidiSpatializer : MidiFilePlayer
    {
        protected new void Awake()
        {
            //Debug.Log("Awake MidiSpatializer:" + MPTK_IsPlaying + " " + MPTK_PlayOnStart + " " + MPTK_IsPaused);
            IsMidiChannelSpace = true;
            MPTK_Spatialize = true;
            if (!MPTK_CorePlayer)
            {
                Debug.LogWarning($"MidiSpatializer works only in Core player mode. Change properties in inspector");
                return;
            }

            if (MPTK_MaxDistance<=0f)
                Debug.LogWarning($"Max Distance is set to 0, any sound will be played.");

            base.AwakeMidiFilePlayer();
        }

        public new void Start()
        {
            //Debug.Log("Start MidiSpatializer:" + MPTK_IsPlaying + " " + MPTK_PlayOnStart + " " + MPTK_IsPaused);
            if (!MPTK_CorePlayer)
                return;
            base.StartMidiFilePlayer();
        }
    }
}

