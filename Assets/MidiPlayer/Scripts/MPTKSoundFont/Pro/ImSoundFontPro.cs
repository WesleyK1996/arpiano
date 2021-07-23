using MEC;
using MidiPlayerTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace MidiPlayerTK
{
    /// <summary>
    /// SoundFont adapted to Unity
    /// </summary>
    public partial class ImSoundFont
    {
        /// <summary>
        /// Save an ImSoundFont 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        public void Save(string path, string name, bool onlyXML)
        {
            try
            {
                if (!onlyXML)
                {
                    // Save SF binary data 
                    new SFSave(path + "/" + name + MidiPlayerGlobal.ExtensionSoundFileFileData, HiSf);
                }

                // Build bank selected
                StrBankSelected = "";
                for (int b = 0; b < BankSelected.Length; b++)
                    if (BankSelected[b])
                        StrBankSelected += b + ",";

                var serializer = new XmlSerializer(typeof(ImSoundFont));
                using (var stream = new FileStream(path + "/" + name + MidiPlayerGlobal.ExtensionSoundFileDot, FileMode.Create))
                {
                    serializer.Serialize(stream, this);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        static public IEnumerator<float> LoadLiveSF(string pathSF, int defaultBank = -1, int drumBank = -1, MidiSynth[] synths = null, bool restartPlayer = true)
        {
            //Debug.Log("LoadLiveSF " + pathSF);
            MidiPlayerGlobal.MPTK_SoundFontLoaded = false;
            List<MidiFilePlayer> playerToRestart = new List<MidiFilePlayer>();

            if (synths != null)
            {
                foreach (MidiSynth synth in synths)
                {
                    if (synth is MidiFilePlayer)
                    {
                        MidiFilePlayer player = (MidiFilePlayer)synth;
                        if (player.MPTK_IsPlaying)
                        {
                            playerToRestart.Add(player);
                            player.MPTK_Stop(); // stop and clear all sound
                        }
                    }
                    //synth.MPTK_ClearAllSound();
                    yield return Timing.WaitUntilDone(Timing.RunCoroutine(synth.ThreadWaitAllStop()), false);
                    synth.MPTK_StopSynth();
                }
            }
            DicAudioClip.Init();
            DicAudioWave.Init();

            //Debug.Log("Start Loading SF " + pathSF);

            using (UnityEngine.Networking.UnityWebRequest req = UnityEngine.Networking.UnityWebRequest.Get(pathSF))
            {
                yield return Timing.WaitUntilDone(req.SendWebRequest());

                if (!req.isNetworkError)
                {
                    try
                    {
                        byte[] data = req.downloadHandler.data;
                        if (data != null && data.Length > 4 && System.Text.Encoding.Default.GetString(data, 0, 4) == "RIFF")
                        {
                            //Debug.Log("Load with header " + System.Text.Encoding.Default.GetString(data, 0, 8));

                            System.Diagnostics.Stopwatch watchLoadSF = new System.Diagnostics.Stopwatch(); // High resolution time
                            watchLoadSF.Start();

                            ImSoundFont imsf = new ImSoundFont();
                            SFLoad load = new SFLoad(data, SFFile.SfSource.SF2);
                            imsf.HiSf = load.SfData;
                            //Debug.Log("   SampleData.Length:" + load.SfData.SampleData.Length);
                            //Debug.Log("   preset.Length:" + load.SfData.preset.Length);
                            //Debug.Log("   Samples.Length:" + load.SfData.Samples.Length);

                            imsf.LiveSF = true;
                            imsf.SoundFontName = Path.GetFileNameWithoutExtension(pathSF);

                            imsf.Banks = new ImBank[ImSoundFont.MAXBANKPRESET];
                            foreach (HiPreset p in imsf.HiSf.preset)
                            {
                                imsf.BankSelected[p.Bank] = true;
                                if (imsf.Banks[p.Bank] == null)
                                {
                                    // New bank, create it
                                    imsf.Banks[p.Bank] = new ImBank()
                                    {
                                        BankNumber = p.Bank,
                                        defpresets = new HiPreset[ImSoundFont.MAXBANKPRESET]
                                    };
                                }

                                // Sort preset by number of patch
                                imsf.Banks[p.Bank].defpresets[p.Num] = p;
                            }

                            //int lastBank = 0; // generally drum kit

                            foreach (ImBank bank in imsf.Banks)
                            {
                                if (bank != null)
                                {
                                    bank.PatchCount = 0;
                                    //lastBank = bank.BankNumber;
                                    foreach (HiPreset preset in bank.defpresets)
                                        if (preset != null)
                                        {
                                            // Bank count
                                            bank.PatchCount++;
                                        }
                                    // sf.PatchCount += bank.PatchCount;
                                }
                            }

                            imsf.DefaultBankNumber = defaultBank < 0 ? imsf.FirstBank() : defaultBank;
                            imsf.DrumKitBankNumber = drumBank < 0 ? imsf.LastBank() : drumBank;
                            //Debug.Log("DefaultBankNumber:" + imsf.DefaultBankNumber);
                            //Debug.Log("DrumKitBankNumber:" + imsf.DrumKitBankNumber);
                            if (MidiPlayerGlobal.ImSFCurrent != null)
                            {
                                //Debug.Log(">>> Collect " + DateTime.Now + " " + GC.GetTotalMemory(false));
                                MidiPlayerGlobal.ImSFCurrent.SampleData = null;
                                GC.Collect();
                                //Debug.Log("<<< Collect " + DateTime.Now + " " + GC.GetTotalMemory(false));
                            }
                            MidiPlayerGlobal.ImSFCurrent = imsf;
                            MidiPlayerGlobal.BuildBankList();
                            MidiPlayerGlobal.BuildPresetList(true);
                            MidiPlayerGlobal.BuildPresetList(false);
                            MidiPlayerGlobal.MPTK_SoundFontLoaded = true;
                            MidiPlayerGlobal.timeToLoadSoundFont = watchLoadSF.Elapsed;

                            System.Diagnostics.Stopwatch watchLoadWave = new System.Diagnostics.Stopwatch(); // High resolution time
                            watchLoadWave.Start();
                            imsf.SampleData = new float[imsf.HiSf.SampleData.Length / 2];
                            int size = imsf.HiSf.SampleData.Length / 2 - 1;
                            for (int i = 0, j = 0; i <= size; i++, j += 2)
                                imsf.SampleData[i] = ((short)((imsf.HiSf.SampleData[j + 1] << 8) | imsf.HiSf.SampleData[j])) / 32768.0F;
                            MidiPlayerGlobal.timeToLoadWave = watchLoadWave.Elapsed;

                            if (MidiPlayerGlobal.OnEventPresetLoaded != null) MidiPlayerGlobal.OnEventPresetLoaded.Invoke();

                            if (synths != null)
                            {
                                foreach (MidiSynth synth in synths)
                                {
                                    synth.MPTK_InitSynth();
                                    if (synth is MidiFilePlayer)
                                        synth.MPTK_StartSequencerMidi();
                                }
                                if (restartPlayer)
                                    foreach (MidiFilePlayer player in playerToRestart)
                                        player.MPTK_RePlay();
                            }
                        }
                        else
                            Debug.LogWarning("SoundFont not find or not a SoundFont - " + pathSF);

                    }
                    catch (System.Exception ex)
                    {
                        MidiPlayerGlobal.ErrorDetail(ex);
                    }
                }
                else
                    Debug.LogWarning("Network error - " + pathSF);
            }
        }
    }
}
