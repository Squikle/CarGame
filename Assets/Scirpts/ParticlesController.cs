using UnityEngine;

[RequireComponent(typeof(CarPhysic))]
public class ParticlesController : MonoBehaviour
{
    private CarPhysic carPhysic;

    [SerializeField] [Range(0,30)] private float minSlipValue=15f;

    void Start() => carPhysic = GetComponent<CarPhysic>();

    void Update()
    {
        surfaceCheck();
        driftSmokeManager();
        dustManager();
    }

    public void driftSmokeManager()
    {
        foreach (WheelsClass wheel in carPhysic.wheels)
        {
            GroundStats groundStats;
            if (groundStats = wheel?.surface?.GetComponent<GroundStats>())
            {
                if (Mathf.Abs(carPhysic.velocity.x) > minSlipValue
                && groundStats.groundType == GroundType.Tough
                && carPhysic.grounded
                && !carPhysic.flipped)
                {
                    wheel.smokeParticles.Play();
                    continue;
                }
            }
            wheel.smokeParticles.Stop();
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
                    wheel.dustParticles.GetComponent<ParticleSystemRenderer>().material = wheel?.surface.GetComponent<MeshRenderer>().material;
                    wheel.dustParticles.Play();
                    continue;
                }
            }
            wheel.dustParticles.Stop();
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