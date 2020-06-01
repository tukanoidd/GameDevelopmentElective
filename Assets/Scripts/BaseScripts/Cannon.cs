using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cannon : MonoBehaviour
{
    public bool reloading = false;
    
    [SerializeField] public GameObject cannonBallPrefab;
    [SerializeField] private float shootPower = 300000;
    [SerializeField] private Transform cannonPlaceholder;

    [SerializeField] private Ship parentShip;

    public void Shoot(float accuracy, float reloadSpeed)
    {
        if (cannonBallPrefab && cannonPlaceholder && !reloading)
        {
            Vector3 shootDirection = cannonPlaceholder.forward;
            float deviation = (100 - accuracy) / 100;

            shootDirection.x += Random.Range(-deviation, deviation);
            shootDirection.y += Random.Range(-deviation, deviation);
            shootDirection.z += Random.Range(-deviation, deviation);
            
            GameObject cannonBall = Instantiate(cannonBallPrefab, cannonPlaceholder.position, cannonPlaceholder.rotation);
            cannonBall.GetComponent<CannonBall>().parentShip = parentShip;
            Rigidbody rb = cannonBall.GetComponent<Rigidbody>();

            rb.AddForce(shootDirection * shootPower, ForceMode.Impulse);
            
            StartCoroutine(Reload(reloadSpeed));
        }
    }

    private IEnumerator Reload(float reloadSpeed)
    {
        if (reloading) yield break;
        
        reloading = true;
        
        float normalizedTime = 0;
        while(normalizedTime <= 1f)
        {
            normalizedTime += Time.deltaTime / reloadSpeed;
            yield return null;
        }

        reloading = false;
    }
}
