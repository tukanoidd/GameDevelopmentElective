using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class CannonBall : MonoBehaviour
{
    [Range(5, 15)] public float damage = 5;
    public bool fire = false;

    public Ship parentShip;

    private void LateUpdate()
    {
        if (transform.position.y < -40) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Sea")
        {
            //todo splash animation maybe?
            
            Destroy(gameObject);
        }
    }
}
