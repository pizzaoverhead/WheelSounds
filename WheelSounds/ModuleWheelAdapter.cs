using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace WheelSounds
{
    internal sealed class ModuleWheelAdapter : IWheelModuleAdapter
    {
        private ModuleWheel moduleWheel;
        private List<WheelCollider> wheelColliders;
        private bool isValid;

        public ModuleWheelAdapter(PartModule partModule)
        {
            if(partModule == null)
                throw new ArgumentNullException("partModule");

            this.moduleWheel = partModule as ModuleWheel;
            this.wheelColliders = new List<WheelCollider>();

            foreach(Wheel wheel in this.moduleWheel.wheels)
            {
                if(wheel.whCollider != null)
                {
                    this.wheelColliders.Add(wheel.whCollider);

                    this.isValid = true;
                }
            }
        }

        #region IWheelModuleAdapter Members

        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
        }

        public bool HasMotor
        {
            get
            {
                return this.moduleWheel.hasMotor;
            }
        }

        public bool MotorEnabled
        {
            get
            {
                return this.moduleWheel.motorEnabled;
            }
        }

        public bool Damaged
        {
            get
            {
                return this.moduleWheel.isDamaged;
            }
        }

        public List<WheelCollider> Wheels
        {
            get
            {
                return this.wheelColliders;
            }
        }

        #endregion
    }
}
