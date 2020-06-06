using System;
using BaseScripts;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Rigidbody))]
public class PowerUp : MonoBehaviour
{
    public PowerUpType powerUpType = PowerUpType.None;
    [Range(10, 50)] public float rotationSpeed = 25; 

    private void FixedUpdate()
    {
        transform.Rotate(0, rotationSpeed * Time.fixedDeltaTime, 0);
    }

    private void OnCollisionEnter(Collision other)
    {
        Ship checkShip = other.gameObject.GetComponent<Ship>();
        if (checkShip != null && !checkShip.HasPowerup)
        {
            
            checkShip.SetPowerUp(this);
        
            Destroy(gameObject);   
        }
    }
}
