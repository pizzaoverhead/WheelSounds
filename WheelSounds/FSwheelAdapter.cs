using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace WheelSounds
{
    /// <summary>
    /// Adapts an FSwheel part module to the well-known IWheelModuleAdapter interface using reflection.
    /// </summary>
    /// <remarks>
    /// Firespitter's FSwheel consists of several classes that are unique to the Firespitter assembly, but creating a hard reference/dependency is not
    /// desired. The remaining option is reflection, and care is taken to perform as much work as possible during initialization.
    /// </remarks>
    internal sealed class FSwheelAdapter : IWheelModuleAdapter
    {
        private const string ASSEMBLY_NAME_FIRESPITTER = "Firespitter";
        private const string TYPE_NAME_FSWHEEL = "FSwheel";
        private const string TYPE_NAME_WHEEL_LIST = "WheelList";
        private const string TYPE_NAME_WHEEL_CLASS = "WheelClass";
        private const string MEMBER_NAME_HAS_MOTOR = "hasMotor";
        private const string MEMBER_NAME_MOTOR_ENABLED = "motorEnabled";
        private const string MEMBER_NAME_WHEEL_LIST = "wheelList";
        private const string MEMBER_NAME_WHEEL_COLLIDER = "wheelCollider";
        private const string MEMBER_NAME_WHEELS = "wheels";

        // Lots of static goodness that we can do once.
        private static Type FSwheelType = Type.GetType(Assembly.CreateQualifiedName(ASSEMBLY_NAME_FIRESPITTER, TYPE_NAME_FSWHEEL));
        private static Type WheelListType = Type.GetType(Assembly.CreateQualifiedName(ASSEMBLY_NAME_FIRESPITTER, TYPE_NAME_WHEEL_LIST));
        private static Type WheelClassType = Type.GetType(Assembly.CreateQualifiedName(ASSEMBLY_NAME_FIRESPITTER, TYPE_NAME_WHEEL_CLASS));
        private static string[] NecessaryFieldNames = new string[] { MEMBER_NAME_HAS_MOTOR, MEMBER_NAME_MOTOR_ENABLED, MEMBER_NAME_WHEEL_LIST };

        private static Dictionary<string, FieldInfo> FSwheelFields = null;
        private static FieldInfo WheelClass_wheelColliderField = null;
        private static FieldInfo WheelList_wheelsField = null;

        private static bool initialized = false;

        private bool isValid;
        private PartModule firespitterWheelModule;
        private List<WheelCollider> wheelColliders;

        private static void InitializeOnce()
        {
            if(!initialized)
            {
                // Type doesn't have op_Equality/op_Inequality, but object does. (Mono/Unity issue?)
                object FSwheelTypeCheck = FSwheelType;
                object WheelListTypeCheck = WheelListType;
                object WheelClassTypeCheck = WheelClassType;

                if(FSwheelTypeCheck != null && WheelListTypeCheck != null && WheelClassTypeCheck != null)
                {
                    FSwheelFields = new Dictionary<string, FieldInfo>();

                    // FSwheel.wheelList is private, unfortunately.
                    FieldInfo[] fields = FSwheelType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    foreach(FieldInfo field in fields)
                    {
                        foreach(string necessaryFieldName in NecessaryFieldNames)
                        {
                            if(field.Name == necessaryFieldName)
                            {
                                FSwheelFields.Add(necessaryFieldName, field);
                            }
                        }
                    }

                    // If the necessary fields on FSwheel aren't available, there's no point in continuing.
                    if(FSwheelFields.Count == NecessaryFieldNames.Length)
                    {
                        WheelClass_wheelColliderField = WheelClassType.GetField(MEMBER_NAME_WHEEL_COLLIDER, BindingFlags.Public | BindingFlags.Instance);
                        object wheelColliderFieldCheck = WheelClass_wheelColliderField;    // FieldInfo doesn't have op_Equality/op_Inequality, but object does. (Mono/Unity issue?)

                        if(wheelColliderFieldCheck != null)
                        {
                            WheelList_wheelsField = WheelListType.GetField(MEMBER_NAME_WHEELS, BindingFlags.Public | BindingFlags.Instance);
                            object wheelsFieldCheck = WheelList_wheelsField;    // FieldInfo doesn't have op_Equality/op_Inequality, but object does. (Mono/Unity issue?)

                            if(wheelsFieldCheck != null)
                            {
                                initialized = true;
                            }
                        }
                    }
                }
            }
        }

        public FSwheelAdapter(PartModule partModule)
        {
            if(partModule == null)
                throw new ArgumentNullException("partModule");

            InitializeOnce();

            // Setup class members the same even if initialization failed, for sane environment.
            this.isValid = false;
            this.firespitterWheelModule = partModule;
            this.wheelColliders = new List<WheelCollider>();

            // Now check to see if the static gear is initialized and setup the dynamic gear specific to this instance.
            if(initialized)
            {
                // WheelList (Firespitter class)
                object wheelList = FSwheelFields[MEMBER_NAME_WHEEL_LIST].GetValue(partModule);

                // List<WheelClass> (Firespitter class)
                object wheels = WheelList_wheelsField.GetValue(wheelList);
                IEnumerable enumerable = wheels as IEnumerable;

                // item is WheelClass (Firespitter class)
                foreach(object item in enumerable)
                {
                    WheelCollider wheelCollider = WheelClass_wheelColliderField.GetValue(item) as WheelCollider;

                    if(wheelCollider != null)
                    {
                        this.wheelColliders.Add(wheelCollider);

                        // Once at least one WheelCollider is available, the adapter should be valid for use by WheelSounds.
                        this.isValid = true;
                    }
                }
            }
        }

        private bool GetBooleanFieldValue(FieldInfo fieldInfo, bool defaultValue)
        {
            bool? value = fieldInfo.GetValue(this.firespitterWheelModule) as bool?;

            return value.HasValue ? value.Value : defaultValue;
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
                return this.GetBooleanFieldValue(FSwheelFields[MEMBER_NAME_HAS_MOTOR], false);
            }
        }

        public bool MotorEnabled
        {
            get
            {
                return this.GetBooleanFieldValue(FSwheelFields[MEMBER_NAME_MOTOR_ENABLED], false);
            }
        }

        public bool Damaged
        {
            get
            {
                // Firespitter doesn't implement wheel damage, so always return false.
                return false;
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
