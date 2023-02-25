using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class WaterAudioManager : MonoBehaviour
    {
        [SerializeField] private Transform water;
        [SerializeField] private float transitionSpeed;
        
        private AudioListener _audioListener;
        private AudioSource[] audioSourcesLand;
        private AudioSource[] audioSourcesWater;

        [SerializeField] private AudioClip[] swimSounds;
        [SerializeField] private float swimSoundDelay;

        private bool playSwimSounds;
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioListener = FindObjectOfType<AudioListener>();
            GameObject[] audioSourcesGoLand = GameObject.FindGameObjectsWithTag("AudioSourceLand");
            GameObject[] audioSourcesGoWater = GameObject.FindGameObjectsWithTag("AudioSourceWater");

            audioSourcesLand = new AudioSource[audioSourcesGoLand.Length];
            audioSourcesWater = new AudioSource[audioSourcesGoWater.Length];

            _audioSource = GetComponent<AudioSource>();

            for (var i = 0; i < audioSourcesGoLand.Length; i++)
            {
                var go = audioSourcesGoLand[i];
                var source = new AudioSource();
                go.TryGetComponent(out source);
                audioSourcesLand[i] = source;
            }
            
            for (var i = 0; i < audioSourcesGoWater.Length; i++)
            {
                var go = audioSourcesGoWater[i];
                var source = new AudioSource();
                go.TryGetComponent(out source);
                audioSourcesWater[i] = source;
            }

            StartCoroutine(SwimSounds());
        }


        //Yeah I know this is horrible.
        private void Update()
        {
            if (_audioListener.transform.position.y < water.transform.position.y)
            {
                foreach (var audioSource in audioSourcesLand)
                {
                    audioSource.volume = Mathf.MoveTowards(audioSource.volume, 0, transitionSpeed * Time.deltaTime);
                }

                foreach (var audioSource in audioSourcesWater)
                {
                    if (audioSource.TryGetComponent(out AudioDayNightManager manager))
                        audioSource.volume = Mathf.MoveTowards(audioSource.volume, manager.DesiredVolume, transitionSpeed * Time.deltaTime);
                    else
                        Debug.LogError("Couldn't fetch AudioDayNightManager from " + audioSource.name + ".");
                }

                playSwimSounds = true;
            }
            else
            {
                foreach (var audioSource in audioSourcesLand)
                {
                    if (audioSource.TryGetComponent(out AudioDayNightManager manager))
                        audioSource.volume = Mathf.MoveTowards(audioSource.volume, manager.DesiredVolume, transitionSpeed * Time.deltaTime);
                    else
                        Debug.LogError("Couldn't fetch AudioDayNightManager from " + audioSource.name + ".");
                }

                foreach (var audioSource in audioSourcesWater)
                {
                    audioSource.volume = Mathf.MoveTowards(audioSource.volume, 0, transitionSpeed * Time.deltaTime);
                }
                
                playSwimSounds = false;
            }

            print(playSwimSounds);
        }
        
        private IEnumerator SwimSounds()
        {
            while (true)
            {
                if (!playSwimSounds)
                    yield return new WaitUntil(() => playSwimSounds);

                if (swimSounds.Length != 0)
                    _audioSource.PlayOneShot(swimSounds[Random.Range(0, swimSounds.Length - 1)]);

                yield return new WaitForSeconds(swimSoundDelay);
            }
        }
    }
}