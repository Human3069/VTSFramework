using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    [DisallowMultipleComponent]
    public class UIDObject : MonoBehaviour
    {
        [HideInInspector]
        public BaseObject _BaseObj;

        [Header("=== UIDObject ===")]
        public string UidValue;

        public enum UidType
        {
            NonRegistered,

            _2D,
            _3D,
            _Utill
        }

        [ReadOnly]
        public UidType _UidType = UidType.NonRegistered;

        public virtual void Registered()
        {
            Debug.Assert(string.IsNullOrEmpty(UidValue) == false, "UID ���� ����ֽ��ϴ� [" + this.transform.parent.name + "/" + this.name + "]");

            _BaseObj = this.GetComponent<BaseObject>();
            if (_BaseObj != null)
            {
                _BaseObj.OnRegistered(this);
            }
        }
    }
}