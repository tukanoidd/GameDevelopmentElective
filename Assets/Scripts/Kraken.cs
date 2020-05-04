using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kraken : MonoBehaviour
{
    public int amount;
    public GameObject theKraken;
    public Vector3 center;
    public Vector3 size = new Vector3(1000, 0, 1000);
    public int TimeKrakenVisible;
    public GameObject chosenShip;
    private void Update()
    {
    }
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            StartCoroutine("SpawnKraken");
            this.gameObject.GetComponent<Collider>().enabled = false;
            this.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    IEnumerator SpawnKraken()
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 pos = center + new Vector3(Random.Range(-size.x / 2, size.x / 2), 0, Random.Range(-size.z / 2, size.z / 2));

            Instantiate(theKraken, pos, Quaternion.identity);

        }
        SpawnChooseKraken(chosenShip.transform);
        yield return new WaitForSeconds(TimeKrakenVisible);
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Kraken");
        for (int i = 0; i < objects.Length; i++)
        {
            Destroy(objects[i]);
        }
        Destroy(this.gameObject);
    }

    void SpawnChooseKraken(Transform ship)
    {
        Instantiate(theKraken, ship.position, Quaternion.identity);
    }
}
