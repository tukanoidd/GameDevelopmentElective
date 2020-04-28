using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kraken : MonoBehaviour
{
    public GameObject theKraken;
    public Vector3 center;
    public Vector3 size;

    private void Update()
    {
        SpawnKraken();
    }
    private void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "Player")
        {
            SpawnKraken();
            Destroy(this.gameObject);
        }
    }

    public void SpawnKraken()
    {
        Vector3 pos = center + new Vector3(Random.Range(-size.x / 2, size.x / 2), 0, Random.Range(-size.z / 2, size.z / 2));

        Instantiate(theKraken, pos, Quaternion.identity);
    }
}
