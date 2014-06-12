using System;
using UnityEngine;

public class WheelSounds : PartModule
{
    [KSPField]
    public float wheelSoundVolume = 1f;
    [KSPField]
    public string wheelSoundFile = "WheelSounds/Sounds/RoveMaxS2";
    public FXGroup WheelSound = null; // Initialised by KSP.

    private ModuleWheel _wheelModule = null;
    private ModuleWheel wheelModule
    {
        get
        {
            if (this._wheelModule == null)
                this._wheelModule = (ModuleWheel)this.part.Modules["ModuleWheel"];
            return this._wheelModule;
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
            Debug.LogError("WheelSounds: Component was null");
            return;
        }
        else
        {
            WheelSound.audio = gameObject.AddComponent<AudioSource>();
            WheelSound.audio.clip = GameDatabase.Instance.GetAudioClip(wheelSoundFile);
            WheelSound.audio.dopplerLevel = 0f;
            WheelSound.audio.rolloffMode = AudioRolloffMode.Logarithmic;
            WheelSound.audio.Stop();
            WheelSound.audio.loop = true;
            WheelSound.audio.volume = wheelSoundVolume * GameSettings.SHIP_VOLUME;

            // Seek to a random position in the sound file so we don't have harmonic effects with other wheels.
            WheelSound.audio.time = UnityEngine.Random.Range(0, WheelSound.audio.clip.length);
        }

        GameEvents.onGamePause.Add(new EventVoid.OnEvent(this.OnPause));
    }

    void OnPause()
    {
        WheelSound.audio.Stop();
    }

    void OnDestroy()
    {
        WheelSound.audio.Stop();
        GameEvents.onGamePause.Remove(new EventVoid.OnEvent(OnPause));
    }

    public override void OnUpdate()
    {
        if (WheelSound != null)
        {
            double totalRpm = 0f;
            int wheelCount = 0;

            foreach (Wheel wheel in wheelModule.wheels)
            {
                if (wheel.whCollider != null)
                {
                    totalRpm += Math.Abs(wheel.whCollider.rpm);
                    wheelCount++;
                }
            }

            if (wheelCount > 0)
            {
                double averageRpm = totalRpm / wheelCount;
                try
                {
                    if (wheelModule.hasMotor && wheelModule.motorEnabled && !wheelModule.isDamaged && averageRpm > 0.5)
                    {
                        WheelSound.audio.pitch = (float)(Math.Sqrt(averageRpm)) / 13;

                        if (averageRpm < 100)
                        {
                            WheelSound.audio.volume = (float)Math.Max(wheelSoundVolume * GameSettings.SHIP_VOLUME * averageRpm / 100f, 0.006f); ;
                        }

                        if (!WheelSound.audio.isPlaying)
                            WheelSound.audio.Play();
                    }
                    else
                    {
                        WheelSound.audio.Stop();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("WheelSounds: " + ex.Message);
                }
            }
            else
            {
                // Do nothing. This happens once every several updates.
            }
        }
        else
            Debug.LogError("WheelSounds on update: Component was null");
    }
}
