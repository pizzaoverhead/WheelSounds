using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModuleWheels;
using UnityEngine;

namespace WheelSounds
{
    internal sealed class ModuleWheelBaseAdaptor : IWheelModuleAdaptor
    {
        /*
         * 1.1 stock wheel modules:
         * ModuleWheelBase
         * ModuleWheelBogey
         * ModuleWheelBrakes
         * ModuleWheelDamage
         * ModuleWheelDeployment
         * ModuleWheelLock
         * ModuleWheelMotor
         * ModuleWheelMotorSteering
         * ModuleWheelSteering
         * ModuleWheelSubmodule
         * ModuleWheelSuspension
         */

        private ModuleWheelBase _moduleWheelBase;
        private ModuleWheelMotor _moduleWheelMotor;
        private ModuleWheelMotorSteering _moduleWheelMotorSteering;
        private ModuleWheelDamage _moduleWheelDamage;

        public ModuleWheelBaseAdaptor(Part part)
        {
            _moduleWheelBase = part.FindModuleImplementing<ModuleWheelBase>();
            _moduleWheelMotor = part.FindModuleImplementing<ModuleWheelMotor>();
            _moduleWheelMotorSteering = part.FindModuleImplementing<ModuleWheelMotorSteering>();
            _moduleWheelDamage = part.FindModuleImplementing<ModuleWheelDamage>();
        }


        /*public void FixedUpdate()
        {
            if (_hasDamageableWheel)
            {
                bool isDamaged = _moduleWheelDamage.isDamaged;
                if (!_wasDamaged && isDamaged)
                    FireOnWheelDamage();
                _wasDamaged = isDamaged;
            }
        }*/

        #region IWheelModuleAdaptor Members

        /*public event EventHandler WheelDamaged;
        public void FireOnWheelDamage()
        {
            if (WheelDamaged != null)
                WheelDamaged(this, null);
        }*/

        public bool IsValid
        {
            get { return _moduleWheelBase.Wheel != null; }
        }

        public bool HasMotor
        {
            get { return _moduleWheelMotor != null || _moduleWheelMotorSteering != null; }
        }

        public bool MotorEnabled
        {
            get {
                if (_moduleWheelMotor == null)
                {
                    if (_moduleWheelMotorSteering == null) return false;
                    return _moduleWheelMotorSteering.motorEnabled;
                }
                return _moduleWheelMotor.motorEnabled;
            }
        }

        public bool Damaged
        {
            get
            {
                if (_moduleWheelDamage == null) return false;
                return _moduleWheelDamage.isDamaged;
            }
        }

        public double GetRpm()
        {
            if (_moduleWheelBase.Wheel == null)
            {
                return 0;
            }
            return _moduleWheelBase.Wheel.wheelCollider.angularVelocity * 10; // Close enough.
        }

        public Vector2 TireForce
        {
            get
            {
                if (_moduleWheelBase.Wheel == null)
                {
                    return Vector2.zero;
                }
                return _moduleWheelBase.Wheel.wheelState[0].tireForce;
            }
        }

        #endregion
    }
}
