using System;
using System.Collections.Generic;

using UnityEngine;

namespace WheelSounds
{
    internal interface IWheelModuleAdaptor
    {
        bool IsValid { get; }
        bool HasMotor { get; }
        bool MotorEnabled { get; }
        bool Damaged { get; }
        double GetRpm();
        Vector2 TireForce { get; }
    }
}
