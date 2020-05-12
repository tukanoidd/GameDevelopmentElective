using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireCannon : MonoBehaviour
{
    public int fireDamage = 1;
    private GameObject playerShip;
    private Ship shipScript;
    private GameObject frontCannon;
    private Cannon frontShot;
    private GameObject leftCannon;
    private Cannon leftShot;
    private GameObject rightCannon;
    private Cannon rightShot;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Boat")
        {
            playerShip = other.gameObject;
            shipScript = playerShip.GetComponent<Ship>();
           
            frontCannon = playerShip.transform.Find("FrontSpawnPoint").gameObject;
            frontShot = frontCannon.GetComponent<Cannon>();
            leftCannon = playerShip.transform.Find("LeftSpawnPoint").gameObject;
            leftShot = leftCannon.GetComponent<Cannon>();
            rightCannon = playerShip.transform.Find("RightSpawnPoint").gameObject;
            rightShot = rightCannon.GetComponent<Cannon>();
           
            StartCoroutine("LoadFireballs", 10);
            this.gameObject.GetComponent<Collider>().enabled = false;
            this.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private IEnumerator LoadFireballs(int fireTime)
    {
            Debug.Log("Started Fire Powerup");
            frontShot.cannonBallPrefab = Resources.Load("Prefabs/FireBall") as GameObject;
            leftShot.cannonBallPrefab = Resources.Load("Prefabs/FireBall") as GameObject;
            rightShot.cannonBallPrefab = Resources.Load("Prefabs/FireBall") as GameObject;
            Debug.Log("loaded cannons");

            while (fireTime > 0)
            {
                Debug.Log(fireTime--);
                shipScript.health = shipScript.health - fireDamage;            
                yield return new WaitForSeconds(1);
            }
            
            frontShot.cannonBallPrefab = Resources.Load("Prefabs/CannonBall") as GameObject;
            leftShot.cannonBallPrefab = Resources.Load("Prefabs/CannonBall") as GameObject;
            rightShot.cannonBallPrefab = Resources.Load("Prefabs/CannonBall") as GameObject;
            Debug.Log("Finished Fire Powerup");
            Destroy(gameObject);
    }
}
