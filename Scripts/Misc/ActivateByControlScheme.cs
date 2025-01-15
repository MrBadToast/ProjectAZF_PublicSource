using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ActivateByControlScheme : SerializedMonoBehaviour
{
    public class SchemeSettings
    {
        public GameObject[] obejctToEnable;
        public GameObject[] obejctToDisable;
    }

    public bool InitializeToDisabled = false;
    public Dictionary<string, SchemeSettings> schemes;

    private void OnEnable()
    {
        if(InitializeToDisabled)
        {
            foreach (var scheme in schemes.Values)
            {
                if (scheme.obejctToEnable != null)
                {
                    foreach (var enable in scheme.obejctToEnable)
                        enable.SetActive(false);
                }

                if (scheme.obejctToDisable != null)
                {
                    foreach (var disable in scheme.obejctToDisable)
                        disable.SetActive(false);
                }
            }
        }

        string currentScheme = PlayerInput.all[0].currentControlScheme;
        SchemeSettings currentSchemeSetting;

        if (schemes.TryGetValue(currentScheme, out currentSchemeSetting))
        {
            if (currentSchemeSetting.obejctToEnable != null)
            {
                foreach (var enable in schemes[currentScheme].obejctToEnable)
                    enable.SetActive(true);
            }

            if (currentSchemeSetting.obejctToDisable != null)
            {
                foreach (var disable in schemes[currentScheme].obejctToDisable)
                    disable.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("컨트롤스킴을 찾을 수 없었습니다: " + currentScheme);
        }
    }
}
