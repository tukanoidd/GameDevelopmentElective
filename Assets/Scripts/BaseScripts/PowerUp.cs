using System;
using BaseScripts;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(MeshFilter))]
public class PowerUp : MonoBehaviour
{
    public PowerUpType powerUpType = PowerUpType.None;
    [Range(10, 50)] public float rotationSpeed = 25; 

    private void FixedUpdate()
    {
        transform.Rotate(0, rotationSpeed * Time.fixedDeltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        Ship checkShip = other.GetComponent<Ship>();
        if (checkShip != null) checkShip.SetPowerUp(this);
        
        Destroy(this);
    }
}
