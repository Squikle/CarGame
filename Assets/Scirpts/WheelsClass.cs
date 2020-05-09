using System;
using UnityEngine;

[System.Serializable]
public class WheelsClass
{
    public Transform wheel;
    public Transform wheelModel;
    public Transform raySource;

    [NonSerialized] public GameObject surface;

    public ParticleSystem dustParticles;
    public ParticleSystem smokeParticles;
}
