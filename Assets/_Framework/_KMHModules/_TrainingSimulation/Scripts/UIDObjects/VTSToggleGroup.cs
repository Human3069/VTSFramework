using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    // ��� ����� 2D�� 3D�� �������̽�(IToggle)�� ���������Ƿ� 2D�� 3D�� ������� ��� �����մϴ�.
    public class VTSToggleGroup : MonoBehaviour
    {
        protected List<IToggle> toggleList = new List<IToggle>();

        [Header("=== VTSToggleGroup ===")]
        [SerializeField]
        protected bool isWithoutNotify;

        public virtual void Register(IToggle targetToggle)
        {
            toggleList.Add(targetToggle);
        }

        public virtual void SetOffWithout(IToggle targetToggle)
        {
            foreach (IToggle toggle in toggleList)
            {
                if (toggle != targetToggle)
                {
                    if (isWithoutNotify == true)
                    {
                        toggle.SetWithoutNotify(false);
                    }
                    else
                    {
                        toggle.IsOn = false;
                    }
                }
            }
        }
    }
}