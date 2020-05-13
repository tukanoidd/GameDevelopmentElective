using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseScripts;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(BoxCollider))]
public class Ship : MonoBehaviour
{
    enum Direction
    {
        Left,
        Right
    }

    #region ShipData

    [Range(1, 100)] public float health = 100;

    [Header("Transform Data")] public Vector3 position = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;

    [Header("Movement Data")] [Range(10, 50)]
    public float speed = 10;

    [Range(10, 25)] public float rotationSpeed = 10;

    [Header("Shooting Data")] [Range(5, 15)]
    public float reloadSpeed = 5;

    [SerializeField] [Range(70, 100)] private float accuracy = 90;

    [Header("Ship Parts References")] [SerializeField]
    private VisionSphere visionSphere;

    [SerializeField] private Cannon _cannonLeft;
    [SerializeField] private Cannon _cannonFront;
    [SerializeField] private Cannon _cannonRight;

    [HideInInspector] public bool dying = false;

    #endregion

    private HashSet<Ship> _visibleShips = new HashSet<Ship>();
    private BoxCollider _shipCollider;
    private CharacterController _shipController;

    private PowerUpMode _powerup = PowerUpMode.None;

    private bool _rotating = false;
    private bool _moving = false;

    private bool _onFire = false;

    private void Awake()
    {
        UpdatePositionRotation();

        _shipCollider = GetComponent<BoxCollider>();
        _shipController = GetComponent<CharacterController>();
    }

    private void FixedUpdate()
    {
        UpdatePositionRotation();
    }

    /// <summary>
    /// Coroutine that will run AI logic
    /// </summary>
    /// <param name="caller">object that called the function</param>
    /// <returns></returns>
    public virtual IEnumerator RunAI(object caller)
    {
        //START WITH
        //if (!(caller is CompetitionManager)) yield break;
        //while (!dying || !CompetitionManager.current.gameOver || !CompetitionManager.current.gameStarted) {
        //    your code here
        //} when implementing

        yield return null; //dont include in implementation
    }

    /// <summary>
    /// Checking for cannonball collision 
    /// </summary>
    /// <param name="other">Object collided with</param>
    private void OnCollisionEnter(Collision other)
    {
        CannonBall checkCannonBall = other.gameObject.GetComponent<CannonBall>();
        if (checkCannonBall)
        {
            //todo hit animation???
            ApplyDamage(checkCannonBall.damage); //apply cannonball damage
            if (checkCannonBall.fire) StartCoroutine(Burn()); //if its fire cannon ball, burn the ship

            Destroy(checkCannonBall);
        }
    }

    /// <summary>
    /// Update position and rotation of the ship 
    /// </summary>
    private void UpdatePositionRotation()
    {
        position = transform.position;
        rotation = transform.rotation;
    }

    /// <summary>
    /// Called when need to apply damage to the ship
    /// </summary>
    /// <param name="damage">How much damage to apply</param>
    private void ApplyDamage(float damage)
    {
        health = Mathf.Clamp(health - damage, 0, 100);

        if (Mathf.Abs(health) < 0.05f) Death();
    }

    /// <summary>
    /// Called when ship dies
    /// </summary>
    private void Death()
    {
        dying = true; //can be used for later just so AI doesnt shot smth that is already dead

        //todo explosion animation
        CompetitionManager.current.RemoveShip(this);

        //destroy object when animation ends or smth
        Destroy(gameObject);
    }

    /// <summary>
    /// Rotate ship specifying direction and angle of rotation
    /// </summary>
    /// <param name="angle">Angle how much to rotate</param>
    /// <param name="direction">Direction in which to rotate the ship</param>
    /// <returns></returns>
    private IEnumerator Rotate(float angle, Direction direction)
    {
        if (_moving) yield break;

        _rotating = true;

        int numFrames = (int) (angle / (rotationSpeed * Time.fixedDeltaTime));
        for (int frame = 0; frame < numFrames; frame++)
        {
            Debug.Log("Rotating");
            transform.Rotate(0f, rotationSpeed * Time.fixedDeltaTime * (direction == Direction.Left ? -1 : 1), 0f);

            yield return new WaitForFixedUpdate();
        }

        _rotating = false;
    }

    /// <summary>
    /// Move ship forward in specific distance
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    private IEnumerator MoveForward(float distance)
    {
        if (_rotating) yield break;

        _moving = true;

        float distanceWent = 0f;
        Vector3 startPos;

        while (distanceWent <= distance)
        {
            startPos = transform.position;
            _shipController.SimpleMove(transform.forward * speed);
            distanceWent += Vector3.Distance(startPos, transform.position);
            yield return null;
        }

        _moving = false;
    }

    /// <summary>
    /// Check of provided dead ship is in visible ships list, if yes, remove it
    /// </summary>
    /// <param name="ship">Ship to check</param>
    public void CheckDeadShip(Ship ship)
    {
        if (_visibleShips.Contains(ship)) RemoveVisibleShip(ship);
    }

    /// <summary>
    /// Add ship to visible ships list
    /// </summary>
    /// <param name="ship">Ship to add</param>
    public void AddVisibleShip(Ship ship)
    {
        _visibleShips.Add(ship);
    }

    /// <summary>
    /// Remove ship from visible ships list
    /// </summary>
    /// <param name="ship">Ship to remove</param>
    public void RemoveVisibleShip(Ship ship)
    {
        _visibleShips.Remove(ship);
    }

    /// <summary>
    /// Set picked powerup
    /// </summary>
    /// <param name="powerup">Object to check if this methos was called from the powerup object itself</param>
    public void SetPowerUp(object powerup)
    {
        if (powerup is PowerUp) _powerup = ((PowerUp) powerup).powerUpMode;
    }

    /// <summary>
    /// Use Speeder powerup
    /// </summary>
    /// <returns></returns>
    private IEnumerator UseSpeederPowerUp()
    {
        if (_powerup != PowerUpMode.Speeder) yield break;
        _powerup = PowerUpMode.None;

        float oldSpeed = speed;
        float oldRotationSpeed = rotationSpeed;
        float oldAccuracy = accuracy;

        speed = Speeder.speed;
        rotationSpeed = Speeder.rotationSpeed;
        accuracy = Speeder.accuracy;

        yield return new WaitForSeconds(Speeder.useTime);

        speed = oldSpeed;
        rotationSpeed = oldRotationSpeed;
        accuracy = oldAccuracy;
    }

    /// <summary>
    /// Use Kraken Powerup
    /// </summary>
    /// <param name="chosenShip">ship to attack with kraken tentacle</param>
    /// <returns></returns>
    private IEnumerator UseKrakenPowerUp(Ship chosenShip = null)
    {
        if (_powerup != PowerUpMode.Kraken) yield break;
        _powerup = PowerUpMode.None;

        List<GameObject> tentacles = new List<GameObject>();

        _powerup = PowerUpMode.None;

        Vector3 center = CompetitionManager.current.mapCenter;
        Vector3 size = CompetitionManager.current.mapSize;

        for (int i = 0; i < Kraken.amount; i++)
        {
            Vector3 pos = center + new Vector3(
                Random.Range(-size.x / 2, size.x / 2),
                0,
                Random.Range(-size.z / 2, size.z / 2)
            );

            tentacles.Add(Instantiate(Kraken.krakenPrefab, pos, Quaternion.identity));
        }

        if (chosenShip != null)
            tentacles.Add(Instantiate(Kraken.krakenPrefab, chosenShip.transform.position, Quaternion.identity));

        yield return new WaitForSeconds(Kraken.timeKrakenVisible);

        foreach (GameObject tentacle in tentacles) Destroy(tentacle);
    }

    /// <summary>
    /// Use Fireboat powerup
    /// </summary>
    /// <returns></returns>
    private IEnumerator UseFireBoatPowerUp()
    {
        if (_powerup != PowerUpMode.FireBoat) yield break;
        _powerup = PowerUpMode.None;

        GameObject oldPrefab = _cannonFront.cannonBallPrefab;

        _cannonFront.cannonBallPrefab = FireBoat.fireCanonBallPrefab;
        _cannonLeft.cannonBallPrefab = FireBoat.fireCanonBallPrefab;
        _cannonRight.cannonBallPrefab = FireBoat.fireCanonBallPrefab;

        yield return new WaitForSeconds(FireBoat.useTime);

        _cannonFront.cannonBallPrefab = oldPrefab;
        _cannonLeft.cannonBallPrefab = oldPrefab;
        _cannonRight.cannonBallPrefab = oldPrefab;
    }

    /// <summary>
    /// Apply continuous damage from burning
    /// </summary>
    /// <returns></returns>
    private IEnumerator Burn()
    {
        if (_onFire) yield break;

        int timeBurnt = 0;
        while (timeBurnt < FireBoat.burnTime)
        {
            yield return new WaitForSeconds(FireBoat.damageInterval);
            timeBurnt += FireBoat.damageInterval;
            ApplyDamage(FireBoat.fireDamage);
        }
    }
}