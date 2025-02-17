using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class AudioClipData
{
    [SerializeField]
    public AudioClip _AudioClip;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    public float MinVolume = 1.0f;

    [SerializeField]
    [Range(0.0f, 1.0f)]
    public float MaxVolume = 1.0f;

    [SerializeField]
    public float ChanceFactor = 1.0f;
}

public class AdvancedAudioSource : MonoBehaviour
{
    [SerializeField] public AudioSource _AudioSource = null;
    [SerializeField] public AudioMixerGroup _AudioMixerGroup = null;

    [SerializeField] public bool AutoPlayOnAwake = true;
    [SerializeField] public float AutoPlayDelay = 0.0f;

    private Sequence PlayDelay;

    [SerializeField] public List<AudioClipData> _AudioClips = new List<AudioClipData>();

    //--------------------------------------------------------------------------------------------------
    public void Awake()
    {
        if (_AudioSource == null)
        {
            _AudioSource = gameObject.AddComponent<AudioSource>();
            _AudioSource.playOnAwake = false;
        }

        if (_AudioMixerGroup != null)
        {
            _AudioSource.outputAudioMixerGroup = _AudioMixerGroup;
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void OnEnable()
    {
        if (AutoPlayOnAwake)
        {
            if (AutoPlayDelay == 0.0f)
            {
                Play();
            }
            else
            {
                PlayDelay = DOTween.Sequence();
                PlayDelay.InsertCallback(AutoPlayDelay, () =>
                    {
                        Play();
                        PlayDelay.Kill();
                        PlayDelay = null;
                    });
            }
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void OnDisable()
    {
        if (PlayDelay != null)
        {
            PlayDelay.Kill();
            PlayDelay = null;
        }
    }

    //--------------------------------------------------------------------------------------------------
    public void Play()
    {
        if (_AudioClips.Count > 0)
        {
            float totalChance = 0.0f;

            foreach (var audioData in _AudioClips)
            {
                totalChance += audioData.ChanceFactor;
            }

            float randomFloat = UnityEngine.Random.Range(0.0f, totalChance);

            AudioClipData selectedClip = null;

            // Iterate through the audio data again to find the selected clip
            foreach (var audioData in _AudioClips)
            {
                if (randomFloat <= audioData.ChanceFactor)
                {
                    // You've found the audio data to use, so set it as the clip
                    selectedClip = audioData;
                    break; // Exit the loop once you've found the clip
                }
                else
                {
                    // Subtract the chance factor of the current audio data
                    randomFloat -= audioData.ChanceFactor;
                }
            }

            if (selectedClip != null)
            {
                _AudioSource.clip = selectedClip._AudioClip;

                _AudioSource.volume = UnityEngine.Random.Range(selectedClip.MinVolume, selectedClip.MaxVolume);
            }
        }

        if (_AudioSource.clip != null)
        {
            _AudioSource.Play();
        }
    }
}
