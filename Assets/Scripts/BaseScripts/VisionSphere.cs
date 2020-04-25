using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class VisionSphere : MonoBehaviour
{
    public enum VisionPosition
    {
        Right,
        Left,
        Front,
        Back
    }

    [NonSerialized] public VisionPosition position = VisionPosition.Front;

    [SerializeField] private Ship parentShip;
    [SerializeField] private Transform parentLookout;
    [SerializeField] [Range(10, 50)] private float rotationSpeed = 10;

    private bool _rotating = false;

    public IEnumerator MoveToDirection(VisionPosition newPosition)
    {
        if (_rotating) yield break;
        if (newPosition == position) yield break;

        _rotating = true;
        
        float angle = 0;

        switch (position)
        {
            case VisionPosition.Front:
                if (newPosition == VisionPosition.Right) angle = 90;
                else if (newPosition == VisionPosition.Back) angle = 180;
                else if (newPosition == VisionPosition.Left) angle = -90;

                break;
            case VisionPosition.Right:
                if (newPosition == VisionPosition.Back) angle = 90;
                else if (newPosition == VisionPosition.Left) angle = 180;
                else if (newPosition == VisionPosition.Front) angle = -90;
                
                break;
            case VisionPosition.Back:
                if (newPosition == VisionPosition.Left) angle = 90;
                else if (newPosition == VisionPosition.Front) angle = 180;
                else if (newPosition == VisionPosition.Right) angle = -90;
                
                break;
            case VisionPosition.Left:
                if (newPosition == VisionPosition.Front) angle = 90;
                else if (newPosition == VisionPosition.Right) angle = 180;
                else if (newPosition == VisionPosition.Back) angle = -90;
                break;
        }
        
        int numFrames = (int)(angle / (rotationSpeed * Time.fixedDeltaTime));
        for (int frame = 0; frame < numFrames; frame++) {
            parentLookout.Rotate(0f, rotationSpeed * Time.fixedDeltaTime, 0f);

            yield return new WaitForFixedUpdate();            
        }

        _rotating = false;
    }

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