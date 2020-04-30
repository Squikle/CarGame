using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shooting : MonoBehaviour
{
    public GameObject objectPrefab;
    public Camera cam;
    public float force=700;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            //spawned.GetComponent<Rigidbody>().AddForce(cam.transform.forward.x * 1300, cam.transform.forward.y + 350, cam.transform.forward.z * 1300);

            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            GameObject spawned = Instantiate(objectPrefab, ray.origin, Quaternion.identity);
            spawned.GetComponent<Rigidbody>().AddForce(ray.direction*force);

            /*if (Physics.Raycast(ray, out hit))
            {
                GameObject spawned = Instantiate(objectPrefab, cam.ScreenToWorldPoint(Input.mousePosition), Quaternion.identity);
                if (hit.collider.gameObject == null)
                    return;
                spawned.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                spawned.GetComponent<Rigidbody>().AddForce((hit.point - cam.ScreenToWorldPoint(Input.mousePosition)) * 200);
                spawned.GetComponent<Rigidbody>().mass = 50;
                GameObject[] carDetails = GameObject.FindGameObjectsWithTag("Car");
                foreach (GameObject cardetail in carDetails)
                {
                    Physics.IgnoreCollision(spawned.GetComponent<Collider>(), cardetail.GetComponent<Collider>(), true);
                }
                Debug.DrawLine(ray.origin, hit.point);
            }
*/
        }
    }
}
