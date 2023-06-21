using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelEffects : MonoBehaviour
{
    [SerializeField] private Transform SkidTrailPrefab;
    [SerializeField] private static Transform skidTrailsDetachedParent;

    private ParticleSystem skidParticles;
    private WheelCollider wheelCollider;
    private AudioSource audioSource;
    private Transform skidTrail;

    public bool Skidding { get; private set; }
    public bool PlayingAudio { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        skidParticles = transform.root.GetComponentInChildren<ParticleSystem>();
        if (skidParticles == null)
        {
            Debug.LogWarning(" no particle system found on car to generate smoke particles", gameObject);
        }
        else
        {
            skidParticles.Stop();
        }

        wheelCollider = GetComponent<WheelCollider>();
        audioSource = GetComponent<AudioSource>();
        PlayingAudio = false;

        if (skidTrailsDetachedParent == null)
        {
            skidTrailsDetachedParent = new GameObject("Skid Trails - Detached").transform;
        }
    }

    public void EmitTyreSmoke()
    {
        skidParticles.transform.position = transform.position - transform.up * wheelCollider.radius;
        skidParticles.Emit(1);
        if (!Skidding)
        {
            StartCoroutine(StartSkidTrail());
        }
    }

    public void PlayAudio()
    {
        audioSource.Play();
        PlayingAudio = true;
    }

    public void StopAudio()
    {
        audioSource.Stop();
        PlayingAudio = false;
    }

    public IEnumerator StartSkidTrail()
    {
        Skidding = true;
        skidTrail = Instantiate(SkidTrailPrefab);
        while (skidTrail == null)
        {
            yield return null;
        }
        skidTrail.parent = transform;
        skidTrail.localPosition = -Vector3.up * wheelCollider.radius;
    }

    public void EndSkidTrail()
    {
        if (!Skidding)
        {
            return;
        }
        Skidding = false;
        skidTrail.parent = skidTrailsDetachedParent;
        Destroy(skidTrail.gameObject, 10);
    }
}
