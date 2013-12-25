using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;

public class WheelSounds : PartModule
{
    [KSPField]
    public float wheelSoundVolume = 1f;
    [KSPField]
    public string wheelSoundFile = "WheelSounds/Sounds/RoveMaxS2";
    public FXGroup WheelSound = null;

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
            WheelSound.audio.Stop();
            WheelSound.audio.loop = true;
            WheelSound.audio.volume = wheelSoundVolume;
        }

        GameEvents.onGamePause.Add(new EventVoid.OnEvent(this.OnPause));
        GameEvents.onGameUnpause.Add(new EventVoid.OnEvent(this.OnUnPause));

        base.OnStart(state);
    }

    void OnPause()
    {
        WheelSound.audio.Stop();
    }

    void OnUnPause()
    {
        WheelSound.audio.volume = wheelSoundVolume;
    }

    void OnDestroy()
    {
        GameEvents.onGamePause.Remove(new EventVoid.OnEvent(OnPause));
        GameEvents.onGameUnpause.Remove(new EventVoid.OnEvent(OnUnPause));
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (WheelSound != null)
        {
            float totalRpm = 0f;
            int wheelCount = 0;

            ModuleWheel wheelModule = (ModuleWheel)this.part.Modules["ModuleWheel"];
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
                float averageRpm = totalRpm / wheelCount;

                if (wheelModule.hasMotor && wheelModule.motorEnabled && !wheelModule.isDamaged && averageRpm > 0)
                {
                    WheelSound.audio.pitch = (float)(Math.Sqrt(averageRpm)) / 13;

                    if (averageRpm < 100)
                    {
                        WheelSound.audio.volume = wheelSoundVolume * averageRpm / 100;
                    }

                    if (!WheelSound.audio.isPlaying)
                        WheelSound.audio.Play();
                }
                else
                    WheelSound.audio.Stop();
            }
            else
                WheelSound.audio.Stop();
        }
        else
            Debug.LogError("WheelSounds on update: Component was null");
    }
}