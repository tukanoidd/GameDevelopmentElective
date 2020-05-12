using UnityEngine;

public class FireBoat
{
    public static int fireDamage = 1;
    public static int burnTime = 10;
    public static int useTime = 5;
    public static int damageInterval = 1;

    public static GameObject fireCanonBallPrefab = Resources.Load<GameObject>("Prefabs/FireBall");
}