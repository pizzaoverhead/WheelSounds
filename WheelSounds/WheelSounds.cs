using System;
using System.Collections;

using UnityEngine;

namespace WheelSounds
{
    public class WheelSounds : PartModule
    {
        [KSPField]
        public float wheelSoundVolume = 1f;
        [KSPField]
        public string wheelSoundFile = "WheelSounds/Sounds/RoveMaxS2";
        [KSPField]
        public bool soundInVacuum = true;
        public FXGroup WheelSound = null; // Initialised by KSP.

        private bool soundsEnabled = true;

        //private ModuleWheel _wheelModule = null;
        //private ModuleWheel wheelModule
        //{
        //    get { return _wheelModule ?? (_wheelModule = (ModuleWheel)part.Modules["ModuleWheel"]); }
        //}
        private IWheelModuleAdapter _wheelModule = null;
        private IWheelModuleAdapter WheelModule
        {
            get
            {
                if (_wheelModule == null)
                {
                    PartModule module = part.FindModuleImplementing<ModuleWheel>();

                    if (module != null)
                    {
                        Debug.Log("WheelSounds: Found ModuleWheel, creating ModuleWheelAdapter.");

                        _wheelModule = new ModuleWheelAdapter(module);
                    }
                    else
                    {
                        module = part.Modules["FSwheel"] as PartModule;

                        if (module != null)
                        {
                            Debug.Log("WheelSounds: Found FSwheel, creating FSwheelAdapter.");

                            // TODO: Check if FSwheel supports its own sound, and if so, then check if this module is utilizing that capability. If so, ignore it, otherwise continue.
                            _wheelModule = new FSwheelAdapter(module);
                        }
                    }
                }

                if (_wheelModule == null)
                {
                    Debug.LogWarning("WheelSounds: No valid wheel modules found in part " + part);
                }

                return _wheelModule;
            }
        }

        public override void OnStart(StartState state)
        {
            if (state == StartState.Editor || state == StartState.None) return;

            if (!GameDatabase.Instance.ExistsAudioClip(wheelSoundFile))
            {
                Debug.LogError("WheelSounds: Audio file not found: " + wheelSoundFile);
                return;
            }

            if (WheelSound == null)
            {
                Debug.LogError("WheelSounds: FXGroup is null");
                return;
            }
            WheelSound.audio = gameObject.AddComponent<AudioSource>();
            WheelSound.audio.clip = GameDatabase.Instance.GetAudioClip(wheelSoundFile);
            WheelSound.audio.dopplerLevel = 0f;
            WheelSound.audio.rolloffMode = AudioRolloffMode.Logarithmic;
            WheelSound.audio.Stop();
            WheelSound.audio.loop = true;
            WheelSound.audio.volume = wheelSoundVolume * GameSettings.SHIP_VOLUME;

            // Seek to a random position in the sound file so we don't have harmonic effects with other wheels.
            WheelSound.audio.time = UnityEngine.Random.Range(0, WheelSound.audio.clip.length);

            GameEvents.onGamePause.Add(OnPause);
        }

        void OnPause()
        {
            WheelSound.audio.Stop();
        }

        void OnDestroy()
        {
            if (WheelSound != null && WheelSound.audio != null)
                WheelSound.audio.Stop();
            GameEvents.onGamePause.Remove(OnPause);
        }

        public override void OnUpdate()
        {
            if (soundsEnabled)
            {
                if (WheelSound == null)
                {
                    Debug.LogError("WheelSounds on update: Component was null");
                    return;
                }

                if (WheelModule == null || !WheelModule.IsValid)
                {
                    soundsEnabled = false;
                    return;
                }

                if (!soundInVacuum && vessel.atmDensity <= 0)
                {
                    WheelSound.audio.Stop();
                    return;
                }

                double totalRpm = 0d;
                int wheelCount = 0;

                foreach (WheelCollider wheelCollider in WheelModule.Wheels)
                {
                    totalRpm += Math.Abs(wheelCollider.rpm);
                    wheelCount++;
                }

                double averageRpm = totalRpm / wheelCount;
                try
                {
                    if (WheelModule.HasMotor && WheelModule.MotorEnabled && !WheelModule.Damaged && averageRpm > 0.5)
                    {
                        WheelSound.audio.pitch = (float)(Math.Sqrt(averageRpm)) / 13;

                        if (averageRpm < 100)
                        {
                            WheelSound.audio.volume = (float)Math.Max(wheelSoundVolume * GameSettings.SHIP_VOLUME * averageRpm / 100f, 0.006f);
                        }

                        if (!WheelSound.audio.isPlaying)
                            WheelSound.audio.Play();
                    }
                    else
                        WheelSound.audio.Stop();
                }
                catch (Exception ex)
                {
                    Debug.LogError("WheelSounds: " + ex.Message);
                }
            }
        }
    }
}
