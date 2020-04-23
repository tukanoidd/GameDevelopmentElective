using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class VisionSphere : MonoBehaviour
{
    [SerializeField] private Ship parentShip;
    
    private void OnTriggerEnter(Collider other)
    {
        Ship checkShip = other.gameObject.GetComponent<Ship>();
        if (checkShip && !checkShip.dying) parentShip.AddVisibleShip(checkShip);
    }

    private void OnTriggerExit(Collider other)
    {
        Ship checkShip = other.gameObject.GetComponent<Ship>();
        if (checkShip) parentShip.RemoveVisibleShip(checkShip);
    }
}
