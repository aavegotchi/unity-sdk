using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Beautify.Universal;

namespace TPSBR
{
    public class AnimationPostProcessingHandler : MonoBehaviour
    {
        [SerializeField] Volume volume;
        [Header("Temperature")]
        private WhiteBalance whiteBalance;
        [SerializeField] bool IsSaturationStarted = false;
        [SerializeField] float saturationInitialValue;
        [SerializeField] float saturationEndValue;
        [SerializeField] float timeOfSaturation = 30f;

        

        private void Start()
        {
           

        }

        private void Update()
        {
            if (volume.profile.TryGet(out whiteBalance) == false)
            {
                Debug.LogError("Volume Profile doesn't contain Color Adjustments!");
            }
            else
            {
                if (IsSaturationStarted)
                {
                    SetTemperature(saturationInitialValue, saturationEndValue);
                }

            }
        }

        public void SetTemperature(float initValue, float endValue)
        {
            StartCoroutine(AnimateTemperature(initValue, endValue));
        }

         public IEnumerator AnimateTemperature(float start, float end)
         {
             //float startSaturation = colorAdjustments.saturation.value;
             float elapsedTime = 0f;

             while (elapsedTime < timeOfSaturation)
             {
                 float currentSaturation = Mathf.Lerp(start, end, elapsedTime / timeOfSaturation);
                 whiteBalance.temperature.Override(currentSaturation);
                 elapsedTime += Time.deltaTime;
                 yield return null;
             }

             whiteBalance.temperature.Override(end);
             IsSaturationStarted = false;
        }

    }
}
