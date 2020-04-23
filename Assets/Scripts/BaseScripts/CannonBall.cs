using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class CannonBall : MonoBehaviour
{
    [Range(5, 15)] public float damage = 5;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name == "Sea")
        {
            //todo splash animation maybe?
            
            Destroy(gameObject);
        }
    }
}
