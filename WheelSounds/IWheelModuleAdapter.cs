using System;
using System.Collections.Generic;

using UnityEngine;

namespace WheelSounds
{
    internal interface IWheelModuleAdapter
    {
        bool IsValid { get; }
        bool HasMotor { get; }
        bool MotorEnabled { get; }
        bool Damaged { get; }
        List<WheelCollider> Wheels { get; }
    }
}
