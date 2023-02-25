using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource), typeof(AudioSource))]
public class AudioDayNightManager : MonoBehaviour
{
    [SerializeField] private List<AudioClip> audioClipsDay;
    [SerializeField] private List<AudioClip> audioClipsNight;

    [SerializeField] private AudioSource audioSource;
    
    [SerializeField] protected float minDelay, maxDelay;
    [SerializeField] [Range(0, 1)] private float desiredVolume;
    [SerializeField] private float lerpSpeed;

    private DayNightCycleManager _dayNight;
    private List<AudioClip> _currentList;

    public float DesiredVolume => desiredVolume;

    private void Awake()
    {
        _dayNight = FindObjectOfType<DayNightCycleManager>();
        _currentList = _currentList = _dayNight.Progression < 0.5 ? audioClipsDay : audioClipsNight;
        StartCoroutine(PlaySoundWithDelay());
    }

    private IEnumerator PlaySoundWithDelay()
    {
        if (_currentList != null)
        {
            AudioClip clip = ChooseRandomAudioClip(_currentList);
            audioSource.clip = clip;
            audioSource.volume = 0;

            print(clip != null ? "Successfully initialized audio-clip!" : "Audio Clip is null");

            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
            audioSource.Play();
            //Lerp up in lerpSpeed in seconds.
            while (Math.Abs(audioSource.volume - desiredVolume) > 0.01f)
            {
                audioSource.volume = Mathf.MoveTowards(audioSource.volume, desiredVolume, lerpSpeed * Time.deltaTime);
                yield return null;
            }
            yield return new WaitForSeconds(clip.length - 10);
            //Lerp down in lerpSpeed in seconds.
            while (audioSource.volume != 0)
            {
                audioSource.volume = Mathf.MoveTowards(audioSource.volume, 0, lerpSpeed * Time.deltaTime);
                yield return null;
            }
            StartCoroutine(PlaySoundWithDelay());
        }

        else
        {
            Debug.LogWarning("Audio Clips is null!");
            yield return null;
        }
    }

    private void Update()
    {
        _currentList = _dayNight.Progression < 0.5 ? audioClipsDay : audioClipsNight;
    }

    private AudioClip ChooseRandomAudioClip(List<AudioClip> audioClips)
    {
        return audioClips[Random.Range(0, audioClips.Count)];
    }
}
