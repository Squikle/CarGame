using Boo.Lang;
using System.Collections;
using System.Security.Cryptography;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(CarPhysic))]
public class ParticlesController : MonoBehaviour
{
    private CarPhysic carPhysic;

    [SerializeField] private ParticleSystem smokeParticles;
    private float defaulSmokeParticlesRate;

    [SerializeField] private ParticleSystem dustParticles;

    [SerializeField] [Range(0,30)] private float minSlipValue=15f;

    void Start()
    {
        defaulSmokeParticlesRate = smokeParticles.emission.rateOverDistance.constant;
        carPhysic = GetComponent<CarPhysic>();
    }

    void Update()
    {
        surfaceCheck();
        driftSmokeManager();
        dustManager();
    }

    public void playParticle(ParticleSystem particle, WheelsClass wheel)
    {
        foreach (GameObject emitter in wheel.emitters)
            if (emitter.gameObject.tag == particle.gameObject.tag)
            {
                Debug.Log("Resume " + particle.name);
                emitter.GetComponent<ParticleSystem>().Play();
                var emission = emitter.GetComponent<ParticleSystem>().emission;
                emission.rateOverDistance = defaulSmokeParticlesRate;
                return;
            }

        wheel.emitters.Insert(0, Instantiate(particle.gameObject, wheel.wheelModel.position, Quaternion.identity, wheel.wheelModel));
    }

    public void playParticle(ParticleSystem particle, WheelsClass wheel, Material particleMaterial)
    {
        foreach (GameObject emitter in wheel.emitters)
            if (emitter.gameObject.tag == particle.gameObject.tag)
            {
                var emission = emitter.GetComponent<ParticleSystem>().emission;
                emission.rateOverDistance = defaulSmokeParticlesRate;
                emitter.GetComponent<ParticleSystem>().Play();
                return;
            }

        GameObject newEmitter = Instantiate(particle.gameObject, wheel.wheelModel.position, Quaternion.identity, wheel.wheelModel);
        newEmitter.GetComponent<ParticleSystemRenderer>().material = particleMaterial;
        wheel.emitters.Insert(0, newEmitter);
    }

    public IEnumerator fadeParticles(ParticleSystem particle, WheelsClass wheel, int emmiterIndex)
    {
        ParticleSystem emitter = wheel.emitters[emmiterIndex].GetComponent<ParticleSystem>();
        for (float j = defaulSmokeParticlesRate; j >= 1; j -= 0.1f) // куратина не останавливается и мешает возвращению рейта в норму в методе playParticle
        {
            var emission = emitter.emission;
            emission.rateOverDistance = j;
            Debug.Log("Pausing");
            yield return null;
        }
        emitter.Stop();
        if (!emitter.IsAlive())
        {
            Destroy(wheel.emitters[emmiterIndex]);
            wheel.emitters.RemoveAt(emmiterIndex);
        }
    }

    void stopParticle(ParticleSystem particle, WheelsClass wheel)
    {
        for (int i=0; i<wheel.emitters.Count; i++)
            if (wheel.emitters[i].tag == particle.gameObject.tag)
                StartCoroutine(fadeParticles(particle, wheel, i));
    }

    public void driftSmokeManager()
    {
        foreach (WheelsClass wheel in carPhysic.wheels)
        {
            if (wheel.wheel.tag == "BackWheels")
            {
                GroundStats groundStats;
                if (groundStats = wheel?.surface?.GetComponent<GroundStats>())
                {
                    if (Mathf.Abs(carPhysic.velocity.x) > minSlipValue
                    && groundStats.groundType == GroundType.Tough
                    && carPhysic.grounded
                    && !carPhysic.flipped)
                    {
                        playParticle(smokeParticles, wheel);
                        continue;
                    }
                }
                stopParticle(smokeParticles, wheel);
            }
        }
    }

    public void dustManager()
    {
        foreach (WheelsClass wheel in carPhysic.wheels)
        {
            GroundStats groundStats;
            if (groundStats = wheel?.surface?.GetComponent<GroundStats>())
            {
                if (carPhysic.grounded
                && !carPhysic.flipped
                && groundStats.groundType == GroundType.Dusty)
                {
                    Material groundMaterial = wheel?.surface.GetComponent<MeshRenderer>().material;
                    playParticle(dustParticles, wheel, groundMaterial);
                    continue;
                }
            }
            stopParticle(dustParticles, wheel);
        }
    }

    public void surfaceCheck()
    {
        RaycastHit hit;

        foreach (WheelsClass wheel in carPhysic.wheels)
        {
            if (Physics.Raycast(wheel.raySource.transform.position, -transform.up, out hit, carPhysic.carStats.suspensionLenght, carPhysic.layerMask))
            {
                wheel.surface = hit.collider.gameObject;
            }
        }

        if (Physics.Raycast(transform.position, -transform.up, out hit, 10f, carPhysic.layerMask))
        {
            carPhysic.surface = hit.collider.gameObject;
        }
    }

    public void land()
    {
        GroundStats groundStats = carPhysic.surface.GetComponent<GroundStats>();
        if (groundStats == null)
            return;

        if (groundStats.groundType == GroundType.Dusty)
        {
            carPhysic.carStats.jumpEffect.GetComponent<ParticleSystemRenderer>().material = carPhysic.surface.GetComponent<MeshRenderer>().material;
            GameObject groundingEffect = Instantiate(carPhysic.carStats.jumpEffect.gameObject, transform.position, Quaternion.Euler(90, 0, 0));
            Destroy(groundingEffect, 1f);
        }
    }
}