using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;
using MEC;
using System.Runtime.InteropServices;
using System.Threading;

namespace MidiPlayerTK
{
    /// <summary>
    /// [MPTK PRO] - class extention
    /// </summary>
    public partial class MidiSynth : MonoBehaviour
    {
        // ----------------------------------------------------------------------
        // Apply effect defined in SoundFont : apply individually on each voices
        // ----------------------------------------------------------------------

        /// <summary>
        /// [MPTK PRO] - Apply frequency low-pass filter as defined in the SoundFont. 
        /// This effect is processed with the fluidsynth algo independently on each voices but with a small decrease of performace (40%).
        /// </summary>
        [HideInInspector]
        public bool MPTK_ApplySFFilter;

        /// <summary>
        /// [MPTK PRO] - Frequency cutoff is defined in the SoundFont for each notes. This parameter increase or decrease the default SoundFont value.
        /// </summary>
        [Range(-2000f, 3000f)]
        [HideInInspector]
        public float MPTK_SFFilterFreqOffset = 0f;

        [Range(-96f, 96f)]
        [HideInInspector]
        public float filterQModOffset;
        /// <summary>
        /// [MPTK PRO] - Quality Factor is defined in the SoundFont for each notes. This parameter increase or decrease the default SoundFont value.
        /// </summary>
        public float MPTK_SFFilterQModOffset
        {
            get { return filterQModOffset; }
            set
            {
                if (filterQModOffset != value)
                {
                    filterQModOffset = value;
                    if (ActiveVoices != null)
                        foreach (fluid_voice voice in ActiveVoices)
                            if (voice.resonant_filter != null)
                                voice.resonant_filter.fluid_iir_filter_set_q(voice.q_dB, filterQModOffset);
                }
            }
        }

        [HideInInspector]
        /// <summary>
        /// [MPTK PRO] - Apply reverberation effect as defined in the SoundFont. 
        /// This effect is processed with the fluidsynth algo independently on each voices but with a small decrease of performace (40%).
        /// </summary>
        public bool MPTK_ApplySFReverb;

        [HideInInspector]
        /// <summary>
        /// [MPTK PRO] - Reverberation level is defined in the SoundFont for each notes. This parameter increase or decrease the default SoundFont value.
        /// </summary>
        [Range(-1f, 1f)]
        public float MPTK_SFReverbAmplify;

        [HideInInspector]
        /// <summary>
        /// [MPTK PRO] - Apply chorus effect as defined in the SoundFont.
        /// This effect is processed with the fluidsynth algo independently on each voices but with a small decrease of performace (10%).
        /// </summary>
        public bool MPTK_ApplySFChorus;

        [HideInInspector]
        /// <summary>
        /// [MPTK PRO] - Chorus level is defined in the SoundFont for each notes. This parameter increase or decrease the default SoundFont value.
        /// </summary>
        [Range(-1f, 1f)]
        public float MPTK_SFChorusAmplify;

        // ------------------------
        // Apply effect from Unity
        // ------------------------

        // -------
        // Reverb
        // -------

        /// <summary>
        /// [MPTK PRO] - Set Reverb Unity default value as defined with Unity.
        /// </summary>
        public void MPTK_ReverbSetDefault()
        {
            MPTK_ReverbDryLevel = Mathf.InverseLerp(-10000f, 0f, 0f);
            MPTK_ReverbRoom = Mathf.InverseLerp(-10000f, 0f, -1000f);
            MPTK_ReverbRoomHF = Mathf.InverseLerp(-10000f, 0f, -100f);
            MPTK_ReverbRoomLF = Mathf.InverseLerp(-10000f, 0f, 0f);
            MPTK_ReverbDecayTime = 1.49f;
            MPTK_ReverbDecayHFRatio = 0.83f;
            MPTK_ReverbReflectionLevel = Mathf.InverseLerp(-10000f, 1000f, -2602f);
            MPTK_ReverbReflectionDelay = Mathf.InverseLerp(-10000f, 1000f, -10000f);
            MPTK_ReverbLevel = Mathf.InverseLerp(-10000f, 2000f, 200f);
            MPTK_ReverbDelay = 0.011f;
            MPTK_ReverbHFReference = 5000f;
            MPTK_ReverbLFReference = 250f;
            MPTK_ReverbDiffusion = Mathf.InverseLerp(0f, 100f, 100f);
            MPTK_ReverbDensity = Mathf.InverseLerp(0f, 100f, 100f);
        }

        /// <summary>
        /// [MPTK PRO] - Apply Reverb Unity effect to the AudioSource. The effect is applied to all voices.
        /// </summary>
        public bool MPTK_ApplyUnityReverb
        {
            get { return applyReverb; }
            set { if (ReverbFilter != null) ReverbFilter.enabled = value; applyReverb = value; }
        }
        [HideInInspector]
        public bool applyReverb;

        [HideInInspector]
        public float reverbRoom, reverbRoomHF, reverbRoomLF, reverbReflectionLevel, reverbReflectionDelay, reverbDryLevel;

        [HideInInspector]
        public float reverbDecayTime, reverbDecayHFRatio, reverbLevel, reverbDelay, reverbHfReference, reverbLfReference, reverbDiffusion, reverbDensity;

        /// <summary>
        /// [MPTK PRO] - Mix level of dry signal in output. Ranges from 0 to 1. 
        /// </summary>
        public float MPTK_ReverbDryLevel
        {
            get { return reverbDryLevel; }
            set { reverbDryLevel = value; if (ReverbFilter != null) ReverbFilter.dryLevel = Mathf.Lerp(-10000f, 0f, reverbDryLevel); }
        }

        /// <summary>
        /// [MPTK PRO] - Room effect level at low frequencies. Ranges from 0 to 1.
        /// </summary>
        public float MPTK_ReverbRoom
        {
            get { return reverbRoom; }
            set { reverbRoom = value; if (ReverbFilter != null) ReverbFilter.room = Mathf.Lerp(-10000f, 0f, reverbRoom); }
        }

        /// <summary>
        /// [MPTK PRO] - Room effect high-frequency level. Ranges from 0 to 1.
        /// </summary>
        public float MPTK_ReverbRoomHF
        {
            get { return reverbRoomHF; }
            set { reverbRoomHF = value; if (ReverbFilter != null) ReverbFilter.roomHF = Mathf.Lerp(-10000f, 0f, reverbRoomHF); }
        }

        /// <summary>
        /// [MPTK PRO] - Room effect low-frequency level. Ranges from 0 to 1.
        /// </summary>
        public float MPTK_ReverbRoomLF
        {
            get { return reverbRoomLF; }
            set { reverbRoomLF = value; if (ReverbFilter != null) ReverbFilter.roomLF = Mathf.Lerp(-10000f, 0f, reverbRoomLF); }
        }

        /// <summary>
        /// [MPTK PRO] - Reverberation decay time at low-frequencies in seconds. Ranges from 0.1 to 20. Default is 1.
        /// </summary>
        public float MPTK_ReverbDecayTime
        {
            get { return reverbDecayTime; }
            set { reverbDecayTime = value; if (ReverbFilter != null) ReverbFilter.decayTime = reverbDecayTime; }
        }


        /// <summary>
        /// [MPTK PRO] - Decay HF Ratio : High-frequency to low-frequency decay time ratio. Ranges from 0.1 to 2.0.
        /// </summary>
        public float MPTK_ReverbDecayHFRatio
        {
            get { return reverbDecayHFRatio; }
            set { reverbDecayHFRatio = value; if (ReverbFilter != null) ReverbFilter.decayHFRatio = reverbDecayHFRatio; }
        }

        /// <summary>
        /// [MPTK PRO] - Early reflections level relative to room effect. Ranges from 0 to 1.
        /// </summary>
        public float MPTK_ReverbReflectionLevel
        {
            get { return reverbReflectionLevel; }
            set { reverbReflectionLevel = value; if (ReverbFilter != null) ReverbFilter.reflectionsLevel = Mathf.Lerp(-10000f, 1000f, reverbReflectionLevel); }
        }

        /// <summary>
        /// [MPTK PRO] - Late reverberation level relative to room effect. Ranges from -10000.0 to 2000.0. Default is 0.0.
        /// </summary>
        public float MPTK_ReverbReflectionDelay
        {
            get { return reverbReflectionDelay; }
            set { reverbReflectionDelay = value; if (ReverbFilter != null) ReverbFilter.reflectionsDelay = Mathf.Lerp(-10000f, 1000f, reverbReflectionDelay); }
        }

        /// <summary>
        /// [MPTK PRO] - Late reverberation level relative to room effect. Ranges from 0 to 1. 
        /// </summary>
        public float MPTK_ReverbLevel
        {
            get { return reverbLevel; }
            set { reverbLevel = value; if (ReverbFilter != null) ReverbFilter.reverbLevel = Mathf.Lerp(-10000f, 2000f, reverbLevel); }
        }

        /// <summary>
        /// [MPTK PRO] - Late reverberation delay time relative to first reflection in seconds. Ranges from 0 to 0.1. Default is 0.04
        /// </summary>
        public float MPTK_ReverbDelay
        {
            get { return reverbDelay; }
            set { reverbDelay = value; if (ReverbFilter != null) ReverbFilter.reverbDelay = reverbDelay; }
        }

        /// <summary>
        /// [MPTK PRO] - Reference high frequency in Hz. Ranges from 1000 to 20000. Default is 5000
        /// </summary>
        public float MPTK_ReverbHFReference
        {
            get { return reverbHfReference; }
            set { reverbHfReference = value; if (ReverbFilter != null) ReverbFilter.hfReference = reverbHfReference; }
        }

        /// <summary>
        /// [MPTK PRO] - Reference low-frequency in Hz. Ranges from 20 to 1000. Default is 250
        /// </summary>
        public float MPTK_ReverbLFReference
        {
            get { return reverbLfReference; }
            set { reverbLfReference = value; if (ReverbFilter != null) ReverbFilter.lfReference = reverbLfReference; }
        }

        /// <summary>
        /// [MPTK PRO] - Reverberation diffusion (echo density) in percent. Ranges from 0 to 1. Default is 1.
        /// </summary>
        public float MPTK_ReverbDiffusion
        {
            get { return reverbDiffusion; }
            set { reverbDiffusion = value; if (ReverbFilter != null) ReverbFilter.diffusion = Mathf.Lerp(0f, 100f, reverbDiffusion); }
        }

        /// <summary>
        /// [MPTK PRO] - Reverberation density (modal density) in percent. Ranges from 0 to 1.
        /// </summary>
        public float MPTK_ReverbDensity
        {
            get { return reverbDensity; }
            set { reverbDensity = value; if (ReverbFilter != null) ReverbFilter.density = Mathf.Lerp(0f, 100f, reverbDensity); }
        }

        // -------
        // Chorus
        // -------

        /// <summary>
        /// [MPTK PRO] - Set Chorus Unity default value as defined with Unity.
        /// </summary>
        public void MPTK_ChorusSetDefault()
        {
            MPTK_ChorusDryMix = 0.5f;
            MPTK_ChorusWetMix1 = 0.5f;
            MPTK_ChorusWetMix2 = 0.5f;
            MPTK_ChorusWetMix3 = 0.5f;
            MPTK_ChorusDelay = 40f;
            MPTK_ChorusRate = 0.8f;
            MPTK_ChorusDepth = 0.03f;
        }

        [HideInInspector]
        public bool applyChorus;

        [HideInInspector]
        public float chorusDryMix, chorusWetMix1, chorusWetMix2, chorusWetMix3, chorusDelay, chorusRate, chorusDepth;
        /// <summary>
        /// [MPTK PRO] - Apply Chorus Unity effect to the AudioSource. The effect is applied to all voices.
        /// </summary>
        public bool MPTK_ApplyUnityChorus
        {
            get { return applyChorus; }
            set { if (ChorusFilter != null) ChorusFilter.enabled = value; applyChorus = value; }
        }

        /// <summary>
        /// [MPTK PRO] - Volume of original signal to pass to output. 0 to 1. Default = 0.5.
        /// </summary>
        public float MPTK_ChorusDryMix
        {
            get { return chorusDryMix; }
            set { chorusDryMix = value; if (ChorusFilter != null) ChorusFilter.dryMix = chorusDryMix; }
        }

        /// <summary>
        /// [MPTK PRO] - Volume of 1st chorus tap. 0 to 1. Default = 0.5.
        /// </summary>
        public float MPTK_ChorusWetMix1
        {
            get { return chorusWetMix1; }
            set { chorusWetMix1 = value; if (ChorusFilter != null) ChorusFilter.wetMix1 = chorusWetMix1; }
        }

        /// <summary>
        /// [MPTK PRO] - Volume of 2nd chorus tap. This tap is 90 degrees out of phase of the first tap. 0 to 1. Default = 0.5.
        /// </summary>
        public float MPTK_ChorusWetMix2
        {
            get { return chorusWetMix2; }
            set { chorusWetMix2 = value; if (ChorusFilter != null) ChorusFilter.wetMix2 = chorusWetMix2; }
        }

        /// <summary>
        /// [MPTK PRO] - Volume of 3rd chorus tap. This tap is 90 degrees out of phase of the second tap. 0 to 1. Default = 0.5.
        /// </summary>
        public float MPTK_ChorusWetMix3
        {
            get { return chorusWetMix3; }
            set { chorusWetMix3 = value; if (ChorusFilter != null) ChorusFilter.wetMix3 = chorusWetMix3; }
        }

        /// <summary>
        /// [MPTK PRO] - Chorus delay in ms. 0.1 to 100. Default = 40 ms.
        /// </summary>
        public float MPTK_ChorusDelay
        {
            get { return chorusDelay; }
            set { chorusDelay = value; if (ChorusFilter != null) ChorusFilter.delay = chorusDelay; }
        }

        /// <summary>
        /// [MPTK PRO] - Chorus modulation rate in hz. 0 to 20. Default = 0.8 hz.
        /// </summary>
        public float MPTK_ChorusRate
        {
            get { return chorusRate; }
            set { chorusRate = value; if (ChorusFilter != null) ChorusFilter.rate = chorusRate; }
        }

        /// <summary>
        /// [MPTK PRO] - Chorus modulation depth. 0 to 1. Default = 0.03.
        /// </summary>
        public float MPTK_ChorusDepth
        {
            get { return chorusDepth; }
            set { chorusDepth = value; if (ChorusFilter != null) ChorusFilter.depth = chorusDepth; }
        }

        fluid_revmodel reverb;
        private float[] fx_reverb;
        fluid_chorus chorus;
        private float[] fx_chorus;

        /* Those are the default settings for the reverb */
        const float FLUID_REVERB_DEFAULT_ROOMSIZE = 0.2f;
        const float FLUID_REVERB_DEFAULT_DAMP = 0.0f;
        const float FLUID_REVERB_DEFAULT_WIDTH = 0.5f;
        const float FLUID_REVERB_DEFAULT_LEVEL = 0.9f;

        const int FLUID_CHORUS_DEFAULT_N = 3;           /**< Default chorus voice count */
        const float FLUID_CHORUS_DEFAULT_LEVEL = 2f;      /**< Default chorus level */
        const float FLUID_CHORUS_DEFAULT_SPEED = 0.3f;    /**< Default chorus speed */
        const float FLUID_CHORUS_DEFAULT_DEPTH = 8f;      /**< Default chorus depth */
        const fluid_chorus.fluid_chorus_mod FLUID_CHORUS_DEFAULT_TYPE = fluid_chorus.fluid_chorus_mod.FLUID_CHORUS_MOD_SINE;  /**< Default chorus waveform type */

        private void InitEffect()
        {
            if (CoreAudioSource != null)
            {
                ReverbFilter = CoreAudioSource.GetComponent<AudioReverbFilter>();
                ReverbFilter.enabled = MPTK_ApplyUnityReverb;

                ChorusFilter = CoreAudioSource.GetComponent<AudioChorusFilter>();
                ChorusFilter.enabled = MPTK_ApplyUnityChorus;

                ///* Effects audio buffers */
                /* allocate the reverb module */
                fx_reverb = new float[FLUID_BUFSIZE];
                reverb = new fluid_revmodel(OutputRate, FLUID_BUFSIZE);
                reverb.fluid_revmodel_set(/*(int)fluid_revmodel.fluid_revmodel_set_t.FLUID_REVMODEL_SET_ALL*/0xFF,
                    FLUID_REVERB_DEFAULT_ROOMSIZE, FLUID_REVERB_DEFAULT_DAMP, FLUID_REVERB_DEFAULT_WIDTH, FLUID_REVERB_DEFAULT_LEVEL);

                fx_chorus = new float[FLUID_BUFSIZE];
                /* allocate the chorus module */
                chorus = new fluid_chorus(OutputRate, FLUID_BUFSIZE);
                chorus.fluid_chorus_set((int)fluid_chorus.fluid_chorus_set_t.FLUID_CHORUS_SET_ALL,
                    FLUID_CHORUS_DEFAULT_N, FLUID_CHORUS_DEFAULT_LEVEL, FLUID_CHORUS_DEFAULT_SPEED, FLUID_CHORUS_DEFAULT_DEPTH, FLUID_CHORUS_DEFAULT_TYPE);
            }
        }

        private void PrepareBufferEffect(out float[] reverb_buf, out float[] chorus_buf)
        {
            // Set up the reverb / chorus buffers only, when the effect is enabled on synth level.
            // Nonexisting buffers are detected in theDSP loop. 
            // Not sending the reverb / chorus signal saves some time in that case.
            if (MPTK_ApplySFReverb)
            {
                Array.Clear(fx_reverb, 0, FLUID_BUFSIZE);
                reverb_buf = fx_reverb;
            }
            else
                reverb_buf = null;

            if (MPTK_ApplySFChorus)
            {
                Array.Clear(fx_chorus, 0, FLUID_BUFSIZE);
                chorus_buf = fx_chorus;
            }
            else
                chorus_buf = null;
        }

        private void ProcessEffect(float[] reverb_buf, float[] chorus_buf)
        {
            /* send to reverb */
            if (MPTK_ApplySFReverb && reverb_buf != null)
            {
                reverb.fluid_revmodel_processmix(reverb_buf, left_buf, right_buf);
            }

            /* send to chorus */
            if (MPTK_ApplySFChorus && chorus_buf != null)
            {
                chorus.fluid_chorus_processmix(chorus_buf, left_buf, right_buf);
            }
        }

        // MultiPlayer is here JORDI!
        private void BuildChannelSynth()
        {
            // Only the main midi reader instanciate all the others synths
            if (IsMidiChannelSpace && MPTK_DedicatedChannel < 0)
            {
                SpatialSynths = new MidiFilePlayer[16];
                for (int channel = 0; channel < SpatialSynths.Length; channel++)
                {
                    // Bad parameters could exec infinite loop, bodyguard below
                    if (lastIdSynth > 100) break;
                    //Debug.Log($"Before Instantiate synth  IdSynth:{IdSynth} channel:{channel}");
                    MidiFilePlayer mfp = Instantiate<MidiFilePlayer>((MidiFilePlayer)this);
                    //Debug.Log($"After Instantiate synth mfp.IdSynth:{mfp.IdSynth} mfp.IsMidiReader:{mfp.IsMidiReader}");
                    //mfp.IsMidiReader = false;
                    mfp.dedicatedChannel = channel;
                    //mfp.name = $"Synth {IdSynth} {mfp.IdSynth} Channel:{channel}";
                    mfp.name = $"Synth C{channel}";
                    mfp.MPTK_PlayOnStart = false;
                    mfp.MPTK_InitSynth();
                    mfp.MPTK_Spatialize = true;
                    SpatialSynths[channel] = mfp;
                }
                // Avoid set parent in the previous loop because infinite loop are created. Why? I don't known!!!
                foreach (MidiFilePlayer mfp in SpatialSynths) mfp.transform.SetParent(this.transform);
            }
        }

    }
}
