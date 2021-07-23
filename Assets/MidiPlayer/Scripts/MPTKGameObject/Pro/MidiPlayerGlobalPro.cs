using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;
using System;
using System.Collections.ObjectModel;
using MEC;

namespace MidiPlayerTK
{
    /// <summary>
    /// Singleton class to manage all global features of MPTK.
    /// </summary>
    public partial class MidiPlayerGlobal : MonoBehaviour
    {
        /// <summary>
        /// [MPTK PRO] - Full path to SoundFont file (.sf2) or URL to load. 
        /// Defined in the MidiPlayerGlobal editor inspector. 
        /// Must start with file:// or http:// or https://.
        /// </summary>
        public string MPTK_LiveSoundFont;

        /// <summary>
        /// [MPTK PRO] - Changing the current Soundfont on fly. If some Midis are playing they are restarted.
        /// </summary>
        /// <param name="name">SoundFont name</param>
        /// <param name="restartPlayer">if a midi is playing, restart the current playing midi</param>
        public static void MPTK_SelectSoundFont(string name, bool restartPlayer = true)
        {
            if (Application.isPlaying)
                Timing.RunCoroutine(SelectSoundFontThread(name, restartPlayer));
            else
                SelectSoundFont(name);
        }

        /// <summary>
        /// Set default soundfont
        /// </summary>
        /// <param name="name"></param>
        /// <param name="restartPlayer"></param>
        /// <returns></returns>
        private static IEnumerator<float> SelectSoundFontThread(string name, bool restartPlayer = true)
        {
            if (!string.IsNullOrEmpty(name))
            {
                int index = CurrentMidiSet.SoundFonts.FindIndex(s => s.Name == name);
                if (index >= 0)
                {
                    MidiPlayerGlobal.CurrentMidiSet.SetActiveSoundFont(index);
                    MidiPlayerGlobal.CurrentMidiSet.Save();
                }
                else
                {
                    Debug.LogWarning("SoundFont not found: " + name);
                    yield return 0;
                }
            }
            // Load selected soundfont
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(LoadSoundFontThread(restartPlayer)));
        }

        /// <summary>
        /// [MPTK PRO] - Select and load a SF when editor
        /// </summary>
        /// <param name="name"></param>
        private static void SelectSoundFont(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                int index = CurrentMidiSet.SoundFonts.FindIndex(s => s.Name == name);
                if (index >= 0)
                {
                    MidiPlayerGlobal.CurrentMidiSet.SetActiveSoundFont(index);
                    MidiPlayerGlobal.CurrentMidiSet.Save();
                    // Load selected soundfont
                    LoadSoundFont();
                }
                else
                {
                    Debug.LogWarning("SoundFont not found " + name);
                }
            }
        }

        /// <summary>
        ///  [MPTK PRO] - Load a SoundFont on the fly when application is running. SoundFont is loaded from a local file or from the web.
        ///  If some Midis are playing they are restarted.
        /// </summary>
        /// <param name="pathSF">Full path to Midi file or URL to play. must start with file:// or http:// or https://.</param>
        /// <param name="defaultBank">default bank to use for instrument, default is the first</param>
        /// <param name="drumBank">bank to use for drum kit, default is the last</param>
        /// <param name="restartPlayer">Restart MidiFilePlayer</param>
        static public void MPTK_LoadLiveSF(string pathSF, int defaultBank = -1, int drumBank = -1, bool restartPlayer = true)
        {
            if (string.IsNullOrEmpty(pathSF))
                Debug.Log("LoadLiveSF: SoundFont path not defined");
            else if (!pathSF.ToLower().StartsWith("file://") &&
                     !pathSF.ToLower().StartsWith("http://") &&
                     !pathSF.ToLower().StartsWith("https://"))
                Debug.LogWarning("LoadLiveSF: path to SoundFont must start with file:// or http:// or https:// - found: '" + pathSF + "'");
            else
            {
                MidiSynth[] synths = FindObjectsOfType<MidiSynth>();
                Timing.RunCoroutine(ImSoundFont.LoadLiveSF(instance.MPTK_LiveSoundFont, defaultBank, drumBank, synths, restartPlayer));
            }
        }
    }
}
