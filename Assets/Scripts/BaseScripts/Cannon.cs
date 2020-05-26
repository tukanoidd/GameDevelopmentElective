using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cannon : MonoBehaviour
{
    public bool reloading = false;
    
    [SerializeField] public GameObject cannonBallPrefab;
    [SerializeField] [Range(1, 10)] private float shootPower;
    [SerializeField] private Transform cannonPlaceholder;

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
            cannonBall.GetComponent<Rigidbody>().AddForce(shootDirection * shootPower, ForceMode.Impulse);

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
