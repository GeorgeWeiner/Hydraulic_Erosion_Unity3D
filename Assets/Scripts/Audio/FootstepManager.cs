using System.Collections;
using Player;
using UnityEngine;
using Random = UnityEngine.Random;

public class FootstepManager : MonoBehaviour
{
    [Header("Footsteps Configuration")]
    [SerializeField] private AudioClip[] footstepClipsGrass;
    [SerializeField] private AudioClip[] footstepClipsSnow;
    [SerializeField] private AudioClip[] footstepClipsWater;
    
    [SerializeField] private float maxGrassHeight;

    [SerializeField] private AudioClip[] armorClips;
    [SerializeField] private float footstepDelayWalk;
    [SerializeField] private float footstepDelaySprint;
    [SerializeField] private float minFootstepVelocity;

    [Header("Pitch")] 
    [SerializeField] private float minPitch;
    [SerializeField] private float maxPitch;

    private PlayerController _controller;
    private Rigidbody _rb;
    private AudioSource _audioSource;
    
    
    private float _maxWaterHeight;

    private bool _playFootsteps;
    private float _footstepDelay;
    private AudioClip[] _footsteps;

    private void Awake()
    {
        _controller = FindObjectOfType<PlayerController>();
        _rb = _controller.GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();

        StartCoroutine(FootstepPlayer());
        _maxWaterHeight = GameObject.Find("Water").transform.position.y + 1;
    }
    
    private void Update()
    {
        DecideFootstepType();
        
        if (_rb.velocity.magnitude < minFootstepVelocity || !_controller.IsGrounded())
            _playFootsteps = false;
        else
            _playFootsteps = true;

        _footstepDelay = PlayerInput.Sprint() ? footstepDelaySprint : footstepDelayWalk;
    }
    
    private void DecideFootstepType()
    {
        _footsteps = _controller.transform.position.y switch
        {
            var n when (n <= _maxWaterHeight) => footstepClipsWater,
            var n when (n <= maxGrassHeight && n >= _maxWaterHeight) => footstepClipsGrass,
            var n when (n >= maxGrassHeight) => footstepClipsSnow,
            _ => footstepClipsSnow
        };
    }

    private IEnumerator FootstepPlayer()
    {
        while (true)
        {
            if (!_playFootsteps)
                yield return new WaitUntil(() => _playFootsteps);
            _audioSource.pitch = Random.Range(minPitch, maxPitch);
            
            if (_footsteps.Length != 0)
                _audioSource.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length - 1)]);
            
            if (armorClips.Length != 0)
                _audioSource.PlayOneShot(armorClips[Random.Range(0, armorClips.Length - 1)]);
            
            yield return new WaitForSeconds(_footstepDelay);
        }
    }
}
