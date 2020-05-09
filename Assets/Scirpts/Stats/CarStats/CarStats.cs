using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "stats_NewCar", menuName = "CarStats")]
public class CarStats : ScriptableObject
{
    [Header("Visualisation")]
    public ParticleSystem jumpEffect;
    public float wheelRadius = 1f;

    [Header("Stats")]
    public float maxSpeed = 60f;
    public float acceleration = 14000f;
    public float backAcceleration = 8000f;
    public float steering = 10000f;

    [Header("Raycast")]
    public float suspensionStrength = 1200f;
    public float suspensionLenght = 0.5f;
    public float damping = 50f;
}