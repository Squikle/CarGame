using UnityEngine;

[RequireComponent(typeof(CarPhysic))]
public class ParticlesController : MonoBehaviour
{
    private CarPhysic carPhysic;

    [Range(0, 30)] public float minimalSlipValue=15f;
    void Start() => carPhysic = GetComponent<CarPhysic>();

    void Update()
    {
        foreach (WheelsClass wheel in carPhysic.wheels)
        {
            if (wheel.surface != null)
            {
                GroundStats groundStats = wheel.surface.GetComponent<GroundStats>();

                if (groundStats != null)
                {
                    //smoke
                    if (Mathf.Abs(carPhysic.velocity.x) > minimalSlipValue
                        && Mathf.Abs(carPhysic.verticalAxis) > 0.3f
                        && groundStats.groundType == GroundType.Tough
                        && carPhysic.grounded
                        && !carPhysic.flipped)
                    {
                        wheel.smokeParticles.Play();
                    }
                    else
                        wheel.smokeParticles.Stop();

                    //dust
                    if (carPhysic.grounded
                        && !carPhysic.flipped
                        && groundStats.groundType == GroundType.Dusty)
                    {
                        wheel.dustParticles.GetComponent<ParticleSystemRenderer>().material = wheel.surface.GetComponent<MeshRenderer>().material;
                        wheel.dustParticles.Play();
                    }
                    else
                        wheel.dustParticles.Stop();
                }
            }
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
