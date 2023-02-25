using UnityEngine;
using UnityEngine.VFX;

public class AngleSetter : MonoBehaviour
{
    [SerializeField] private Transform directionalLight;
    [SerializeField] private VisualEffect godRays;
    private void Update()
    {
        UpdateValue();
    }

    private void OnValidate()
    {
        UpdateValue();
    }

    private void UpdateValue()
    {
        var targetRotation = Quaternion.Euler(directionalLight.eulerAngles - new Vector3(90, 0, 0));
        transform.localRotation = targetRotation;
    }
}
