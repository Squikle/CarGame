using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CarPhysic))]
public class ParticlesController : MonoBehaviour
{
    private CarPhysic carPhysic;

    [SerializeField] private ParticleSystem smokeParticles=null;

    [SerializeField] private ParticleSystem dustParticles=null;

    [SerializeField] [Range(0,30)] private float minSlipValue=15f;

    void Start() => carPhysic = GetComponent<CarPhysic>();

    void Update()
    {
        surfaceCheck();
        driftSmokeManager();
        dustManager();
        clearParticles();
    }

    public void playParticle(ParticleSystem particle, WheelsClass wheel)
    {
        foreach (GameObject emitter in wheel.emitters)
        {
            if (emitter.gameObject.tag == particle.gameObject.tag)
            {
                emitter.GetComponent<ParticleSystem>().Play();
                return;
            }
        }

        wheel.emitters.Insert(0, Instantiate(particle.gameObject, wheel.wheelModel.position, Quaternion.identity, wheel.wheelModel));
    }

    public void playParticle(ParticleSystem particle, WheelsClass wheel, Material particleMaterial)
    {
        foreach (GameObject emitter in wheel.emitters)
            if (emitter.gameObject.tag == particle.gameObject.tag)
            {
                if (wheel.newSurface) StartCoroutine(stopCoroutine(emitter.GetComponent<ParticleSystem>()));
                else
                {
                    emitter.GetComponent<ParticleSystemRenderer>().material = particleMaterial;
                    emitter.GetComponent<ParticleSystem>().Play();
                    return;
                }
            }

        GameObject newEmitter = Instantiate(particle.gameObject, wheel.wheelModel.position, Quaternion.identity, wheel.wheelModel);
        newEmitter.GetComponent<ParticleSystemRenderer>().material = particleMaterial;
        wheel.emitters.Insert(0, newEmitter);
    }

    void clearParticles()
    {
        foreach (WheelsClass wheel in carPhysic.wheels)
        {
            for (int i = 0; i < wheel.emitters.Count; i++)
            {
                ParticleSystem particles = wheel.emitters[i].GetComponent<ParticleSystem>();
                if (!particles.IsAlive())
                {
                    Destroy(wheel.emitters[i]);
                    wheel.emitters.RemoveAt(i);
                }
            }
        }
    }

    void stopParticle(ParticleSystem particle, WheelsClass wheel)
    {
        for (int i = 0; i < wheel.emitters.Count; i++)
            if (wheel.emitters[i].tag == particle.gameObject.tag)
            {
                ParticleSystem emitter = wheel.emitters[i].GetComponent<ParticleSystem>();
                StartCoroutine(stopCoroutine(emitter));
            }
    }

    IEnumerator stopCoroutine(ParticleSystem emitter)
    {
        yield return new WaitForSeconds(0.05f);
        if (emitter)
            emitter.Stop();
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
                    playParticle(dustParticles, wheel, wheel.surface.GetComponent<MeshRenderer>().material);
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
                if (hit.collider.gameObject != wheel.surface)
                    wheel.newSurface = true;
                else
                    wheel.newSurface = false;

                wheel.surface = hit.collider.gameObject;
            }
        }

        if (Physics.Raycast(transform.position, -transform.up, out hit, 10f, carPhysic.layerMask))
            carPhysic.surface = hit.collider.gameObject;
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