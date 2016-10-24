using System;
using System.Collections;
using UnityEngine;
using ModuleWheels;

namespace WheelSounds
{
    public class WheelSounds : PartModule
    {
        [KSPField]
        public bool soundInVacuum = true;

        [KSPField]
        public float wheelSoundVolume = 1f;
        [KSPField]
        public float wheelSoundPitch = 1f;
        [KSPField]
        public string wheelSoundFile = "WheelSounds/Sounds/RoveMaxS2";
        public FXGroup WheelSound = null; // Initialised by KSP.

        [KSPField]
        public float skidSoundVolume = 1f;
        [KSPField]
        public float skidSoundPitch = 1f;
        [KSPField]
        public string skidSoundFile = "WheelSounds/Sounds/gravelSkid";
        public FXGroup SkidSound = null; // Initialised by KSP.

        [KSPField]
        public float damageSoundVolume = 1f;
        [KSPField]
        public float damageSoundPitch = 1f;
        [KSPField]
        private float damageSoundPitchRandomRange = 0.3f;
        [KSPField]
        public string damageSoundFile = "WheelSounds/Sounds/wheelDamage";
        public FXGroup DamageSound = null; // Initialised by KSP.

        // Used to disable sounds in case of errors to prevent log spam.
        private bool _wheelSoundsEnabled = true;
        private bool _skidSoundsEnabled = true;
        private bool _damageSoundsEnabled = true;
        private bool _hasDamageableWheel = false;
        private bool _wasDamaged = false;
        private ModuleWheelDamage _moduleWheelDamage;

        //private ModuleWheel _wheelModule = null;
        //private ModuleWheel wheelModule
        //{
        //    get { return _wheelModule ?? (_wheelModule = (ModuleWheel)part.Modules["ModuleWheel"]); }
        //}
        private IWheelModuleAdaptor _wheelModule = null;
        private IWheelModuleAdaptor WheelModule
        {
            get
            {
                if (_wheelModule == null)
                {
                    PartModule module = part.FindModuleImplementing<ModuleWheelBase>();
                    if (module != null)
                    {
                        Debug.Log("[WheelSounds] Found ModuleWheelBase, creating ModuleWheelBaseAdaptor.");

                        _wheelModule = new ModuleWheelBaseAdaptor(part);
                    }
                    else
                    {
                        module = part.Modules["FSwheel"] as PartModule;

                        if (module != null)
                        {
                            Debug.Log("[WheelSounds] Found FSwheel, creating FSwheelAdaptor.");

                            // TODO: Check if FSwheel supports its own sound, and if so, then check if this module is utilizing that capability. If so, ignore it, otherwise continue.
                            _wheelModule = new FSwheelAdaptor(module);
                        }
                        /*else
                        {
                            // Pre-1.1 support.
                            module = part.FindModuleImplementing<ModuleWheel>();
                            Debug.Log("WheelSounds: Found ModuleWheel, creating ModuleWheelAdaptor.");
                            _wheelModule = new ModuleWheelAdaptor(module);
                        }*/
                    }
                }

                if (_wheelModule == null)
                {
                    Debug.LogWarning("[WheelSounds] No valid wheel modules found in part " + part);
                }

                return _wheelModule;
            }
        }

        public override void OnStart(StartState state)
        {
            if (state == StartState.Editor || state == StartState.None) return;

            InitialiseSound(WheelSound, wheelSoundFile, wheelSoundVolume, true);
            _wheelSoundsEnabled = WheelSound != null && WheelSound.audio != null && wheelSoundVolume > 0;

            InitialiseSound(SkidSound, skidSoundFile, skidSoundVolume, true);
            _skidSoundsEnabled = SkidSound != null && SkidSound.audio != null && skidSoundVolume > 0;
            if (_skidSoundsEnabled) SkidSound.audio.pitch = skidSoundPitch;

            InitialiseSound(DamageSound, damageSoundFile, damageSoundVolume, false);
            _damageSoundsEnabled = DamageSound != null && DamageSound.audio != null && damageSoundVolume > 0;

            GameEvents.onGamePause.Add(OnPause);
            _moduleWheelDamage = part.FindModuleImplementing<ModuleWheelDamage>();
            if (_moduleWheelDamage != null)
                _hasDamageableWheel = true;
            //WheelModule.WheelDamaged += OnWheelDamage;
        }

        private void InitialiseSound(FXGroup soundGroup, string soundFile, float volume, bool loop)
        {
            if (!GameDatabase.Instance.ExistsAudioClip(soundFile))
            {
                Debug.LogError("[WheelSounds] Audio file not found: " + soundFile);
                return;
            }
            if (soundGroup == null)
            {
                Debug.LogError("[WheelSounds] FXGroup is null");
                return;
            }
            soundGroup.audio = gameObject.AddComponent<AudioSource>();
            soundGroup.audio.clip = GameDatabase.Instance.GetAudioClip(soundFile);
            soundGroup.audio.dopplerLevel = 0f;
            soundGroup.audio.rolloffMode = AudioRolloffMode.Logarithmic;
            soundGroup.audio.Stop();
            soundGroup.audio.loop = loop;
            soundGroup.audio.spatialBlend = 1;
            soundGroup.audio.volume = volume * GameSettings.SHIP_VOLUME;
            if (loop)
            {
                // Seek to a random position in the sound file so we don't have harmonic effects with other wheels.
                soundGroup.audio.time = UnityEngine.Random.Range(0, soundGroup.audio.clip.length);
            }
        }

        void OnPause()
        {
            WheelSound.audio.Stop();
            SkidSound.audio.Stop();
        }

        void OnDestroy()
        {
            if (WheelSound != null && WheelSound.audio != null)
                WheelSound.audio.Stop();
            GameEvents.onGamePause.Remove(OnPause);
            //WheelModule.WheelDamaged -= OnWheelDamage;
        }

        public override void OnUpdate()
        {
            if (WheelModule == null || !WheelModule.IsValid)
                return;

            if (!soundInVacuum && vessel.atmDensity <= 0)
            {
                if (WheelSound != null && WheelSound.audio != null)
                    WheelSound.audio.Stop();
                if (SkidSound != null && SkidSound.audio != null)
                    SkidSound.audio.Stop();
                if (DamageSound != null && DamageSound.audio != null)
                    DamageSound.audio.Stop();
                return;
            }

            if (_hasDamageableWheel)
            {
                bool isDamaged = _moduleWheelDamage.isDamaged;
                if (!_wasDamaged && isDamaged)
                    DamageSound.audio.Play();
                _wasDamaged = isDamaged;
            }

            DoWheelSounds();
            DoSkidSounds();
        }

        public void OnWheelDamage(object sender, EventArgs e)
        {
            DamageSound.audio.pitch = UnityEngine.Random.Range(1 - damageSoundPitchRandomRange, 1 + damageSoundPitchRandomRange) * damageSoundPitch;
            DamageSound.audio.Play();
        }

        public void DoWheelSounds()
        {
            if (!_wheelSoundsEnabled) return;
            if (WheelSound == null)
            {
                Debug.LogError("[WheelSounds] Wheel sound: Component was null");
                return;
            }

            double rpm = WheelModule.GetRpm();
            try
            {
                if (WheelModule.HasMotor && WheelModule.MotorEnabled && !WheelModule.Damaged && rpm > 0.5)
                {
                    WheelSound.audio.pitch = ((float)(Math.Sqrt(rpm)) / 13) * wheelSoundPitch;

                    if (rpm < 100)
                    {
                        WheelSound.audio.volume = (float)Math.Max(wheelSoundVolume * GameSettings.SHIP_VOLUME * rpm / 100f, 0.006f);
                    }
                    else
                        WheelSound.audio.volume = wheelSoundVolume * GameSettings.SHIP_VOLUME;

                    if (!WheelSound.audio.isPlaying)
                        WheelSound.audio.Play();
                }
                else
                    WheelSound.audio.Stop();
            }
            catch (Exception ex)
            {
                Debug.LogError("[WheelSounds] Wheel sound: " + ex.Message);
            }
        }

        public void DoSkidSounds()
        {
            if (!_skidSoundsEnabled) return;
            if (SkidSound == null)
            {
                Debug.LogError("[WheelSounds] Skid sound: Component was null");
                return;
            }
            try
            {
                Vector2 f = WheelModule.TireForce;
                float sideways = f.x;
                float forwards = f.y;

                if (!WheelModule.Damaged && (forwards > 5 || sideways > 2.5))
                {
                    if (f.magnitude < 20)
                    {
                        float dominantForce = (float)Math.Max(forwards, sideways * 2);
                        SkidSound.audio.volume = (float)Math.Max(skidSoundVolume * GameSettings.SHIP_VOLUME * dominantForce / 20, 0.006f);
                        if (!SkidSound.audio.isPlaying)
                            SkidSound.audio.Play();
                    }
                    else
                        SkidSound.audio.volume = wheelSoundVolume * GameSettings.SHIP_VOLUME;
                }
                else
                    SkidSound.audio.Stop();
            }
            catch (Exception ex)
            {
                Debug.LogError("[WheelSounds] Skid sound: " + ex.Message);
            }
        }
    }
}
