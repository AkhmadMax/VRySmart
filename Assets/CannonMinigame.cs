using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonMinigame : MonoBehaviour
{
    public Material projectileMaterial;
    public void Shoot(float force)
    {
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.transform.position = transform.position;
        projectile.transform.localScale = Vector3.one * 0.05f;
        projectile.layer = LayerMask.NameToLayer("AR");
        projectile.GetComponent<Renderer>().material = projectileMaterial;
        Rigidbody rigidbody = projectile.AddComponent<Rigidbody>();
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rigidbody.AddForce(-transform.up * force * 0.25f);
    }
}
