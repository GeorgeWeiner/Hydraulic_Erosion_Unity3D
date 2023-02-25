using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Audio
{
    public class AudioRandomizer : MonoBehaviour
    {
        [SerializeField] protected float minDelay, maxDelay;

        private AudioSource audioSource;
        private List<AudioClip> audioClips;

        protected virtual IEnumerator PlaySoundWithDelay(AudioSource audioSource, List<AudioClip> clips)
        {
            if (clips != null)
            {
                AudioClip clip = ChooseRandomAudioClip(clips);
                audioSource.clip = clip;

                if (clip != null)
                    print("Successfully initialized audio-clip!");

                yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
                audioSource.Play();
                yield return new WaitForSeconds(clip.length);

                StartCoroutine(PlaySoundWithDelay(audioSource, clips));
            }

            else
            {
                Debug.LogWarning("Audio Clips is null!");
                yield return null;
            }
        }

        protected AudioClip ChooseRandomAudioClip(List<AudioClip> audioClips)
        {
            return audioClips[Random.Range(0, audioClips.Count - 1)];
        }

    }
}
