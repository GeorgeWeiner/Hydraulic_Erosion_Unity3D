using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DayNightCycleManager : MonoBehaviour
{
    [SerializeField] private Vector3 speed;
    [SerializeField] private Transform sun;
    [SerializeField] private Volume volume;

    private VolumeProfile _volumeProfile;
    private PhysicallyBasedSky _physicallyBasedSky;
    private Vector3Parameter _spaceEulerRotation;

    public float Progression => sun.transform.eulerAngles.x / 360;

    private void Awake()
    {
        _volumeProfile = volume.profile;
        if(!_volumeProfile) throw new NullReferenceException(nameof(VolumeProfile));
        if(!_volumeProfile.TryGet(out _physicallyBasedSky)) throw new NullReferenceException(nameof(PhysicallyBasedSky));
    }

    private void Update()
    {
        RotateAroundAxis();
    }

    private void RotateAroundAxis()
    {
        sun.transform.Rotate(speed, Space.World);
        
        _physicallyBasedSky.spaceRotation.value = sun.eulerAngles;
    }
}
