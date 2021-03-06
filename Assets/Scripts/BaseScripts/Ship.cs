﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseScripts;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(BoxCollider))]
public class Ship : MonoBehaviour
{
    protected enum Direction
    {
        Left,
        Right
    }

    #region ShipData

    [Range(1, 100)] public float health = 100;

    [Header("Transform Data")] public Vector3 position = Vector3.zero;
    public Quaternion rotation = Quaternion.identity;

    [Header("Movement Data")] 
    [Range(10, 100)] public float speed = 60;

    [Range(10, 100)] public float rotationSpeed = 65;

    [Header("Shooting Data")] [Range(5, 15)]
    public float reloadSpeed = 5;

    [SerializeField] [Range(70, 100)] private float accuracy = 90;

    [Header("Ship Parts References")] [SerializeField]
    protected VisionSphere visionSphere;

    [SerializeField] private Cannon _cannonLeft;
    [SerializeField] private Cannon _cannonFront;
    [SerializeField] private Cannon _cannonRight;

    [Header("Audio")] [SerializeField] private AudioSource audioSRC;
    [SerializeField] private AudioClip boatMoving;
    [SerializeField] private AudioClip aFlame;
    [SerializeField] private AudioClip Monster;
    [SerializeField] private AudioClip Damaged;
    [SerializeField] private AudioClip Sink;

    [Space(20)] [HideInInspector] public bool dying = false;

    public bool HasPowerup => powerup != PowerUpType.None;

    #endregion

    private BoxCollider _shipCollider;
    private CharacterController _shipController;

    private TextMeshPro floatingShipNameText;

    protected HashSet<Ship> visibleShips = new HashSet<Ship>();

    protected HashSet<PowerUp> visiblePowerUps = new HashSet<PowerUp>();
    protected PowerUpType powerup = PowerUpType.None;

    protected bool rotating = false;
    protected bool moving = false;

    protected bool onFire = false;

    private void Awake()
    {
        UpdatePositionRotation();

        _shipCollider = GetComponent<BoxCollider>();
        _shipController = GetComponent<CharacterController>();
        
        floatingShipNameText = transform.Find("ShipFloatingName").GetComponent<TextMeshPro>();
        floatingShipNameText.text = name.Replace("(Clone)", "");
        
        audioSRC = GetComponent<AudioSource>();
        audioSRC.clip = boatMoving;
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
        //while ((CompetitionManager.current.gameStarted) && (!dying || !CompetitionManager.current.gameOver)) {
        //    your code here
        //} when implementing

        yield return null; //dont include in implementation
    }

    /// <summary>
    /// Check for cannonball collisions
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionEnter(Collision other)
    {
        CannonBall checkCannonBall = other.collider.gameObject.GetComponent<CannonBall>();
        if (checkCannonBall && !checkCannonBall.parentShip.Equals(this))
        {
            //todo hit animation???
            ApplyDamage(checkCannonBall.damage); //apply cannonball damage
            if (checkCannonBall.fire) StartCoroutine(Burn()); //if its fire cannon ball, burn the ship

            Destroy(checkCannonBall);
        }
    }

    /// <summary>
    /// Checking for kraken collision 
    /// </summary>
    /// <param name="other">Object collided with</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Kraken"))
        {
            ApplyDamage(Kraken.amount);
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
        Impact();

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
        SoundDeath();

        //destroy object when animation ends or smth
        Destroy(gameObject);
    }

    /// <summary>
    /// Rotate ship specifying direction and angle of rotation
    /// </summary>
    /// <param name="angle">Angle how much to rotate</param>
    /// <param name="direction">Direction in which to rotate the ship</param>
    /// <returns></returns>
    protected IEnumerator Rotate(float angle, Direction direction)
    {
        if (moving || rotating) yield break;

        rotating = true;

        int numFrames = (int) (angle / (rotationSpeed * Time.fixedDeltaTime));
        for (int frame = 0; frame < numFrames; frame++)
        {
            transform.Rotate(0f, rotationSpeed * Time.fixedDeltaTime * (direction == Direction.Left ? -1 : 1), 0f);
            yield return new WaitForFixedUpdate();
        }

        rotating = false;
    }

    /// <summary>
    /// Move ship forward in specific distance
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    protected IEnumerator MoveForward(float distance)
    {
        SoundMoving();
        if (rotating || moving) yield break;

        moving = true;

        float distanceWent = 0f;
        Vector3 startPos = transform.position;
        int i = 0;
        
        while (distanceWent <= distance)
        {
            // check if stuck
            if (Vector3.Distance(startPos, transform.position) <= 0.05f)
            {
                i++;
                if (i > 98) transform.position += transform.forward * 20;
                i %= 100;
            }
                
            startPos = transform.position;
            _shipController.SimpleMove(transform.forward * speed);
            distanceWent += Vector3.Distance(startPos, transform.position);
            yield return null;
        }

        moving = false;
    }

    /// <summary>
    /// Check of provided dead ship is in visible ships list, if yes, remove it
    /// </summary>
    /// <param name="ship">Ship to check</param>
    public void CheckDeadShip(Ship ship)
    {
        if (visibleShips.Contains(ship)) RemoveVisibleShip(ship);
    }

    /// <summary>
    /// Add ship to visible ships list
    /// </summary>
    /// <param name="ship">Ship to add</param>
    public void AddVisibleShip(Ship ship) => visibleShips.Add(ship);


    /// <summary>
    /// Remove ship from visible ships list
    /// </summary>
    /// <param name="ship">Ship to remove</param>
    public void RemoveVisibleShip(Ship ship) => visibleShips.Remove(ship);

    /// <summary>
    /// Add PowerUp to visible powerUps list
    /// </summary>
    /// <param name="powerUp">PowerUp to add</param>
    public void AddVisiblePowerUp(PowerUp powerUp) => visiblePowerUps.Add(powerUp);

    /// <summary>
    /// Remove PowerUp from visible powerUps list
    /// </summary>
    /// <param name="powerUp">PowerUp to Remove</param>
    public void RemoveVisiblePowerUp(PowerUp powerUp) => visiblePowerUps.Remove(powerUp);

    /// <summary>
    /// Update visible powerUps list based on existing powerUps on the map
    /// </summary>
    /// <param name="existingPowerUps">Existing powerUps</param>
    public void UpdateVisiblePowerUps(HashSet<PowerUp> existingPowerUps) => visiblePowerUps =
        new HashSet<PowerUp>(visiblePowerUps.Where(powerUp => powerUp != null && existingPowerUps.Contains(powerUp)));

    /// <summary>
    /// Set picked powerup
    /// </summary>
    /// <param name="powerup">Object to check if this method was called from the powerup object itself</param>
    public void SetPowerUp(object powerup)
    {
        if (powerup is PowerUp up) this.powerup = up.powerUpType;
    }

    /// <summary>
    /// Use Speeder powerup
    /// </summary>
    /// <returns></returns>
    protected IEnumerator UseSpeederPowerUp()
    {
        if (powerup != PowerUpType.Speeder) yield break;
        powerup = PowerUpType.None;

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
    protected IEnumerator UseKrakenPowerUp(Ship chosenShip = null)
    {
        if (powerup != PowerUpType.Kraken) yield break;
        powerup = PowerUpType.None;
        SoundMonster();

        List<GameObject> tentacles = new List<GameObject>();

        powerup = PowerUpType.None;

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
    protected IEnumerator UseFireBoatPowerUp()
    {
        SoundFire();
        if (powerup != PowerUpType.FireBoat) yield break;
        powerup = PowerUpType.None;

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
    /// Adds the given amount of health
    /// </summary>
    /// <returns></returns>
    protected IEnumerator UseHealingPowerUp()
    {
        if (powerup != PowerUpType.Healing) yield break;
        powerup = PowerUpType.None;

        health = Mathf.Clamp(health + Healing.amountToAdd, 0, 100);
    }

    /// <summary>
    /// Apply continuous damage from burning
    /// </summary>
    /// <returns></returns>
    private IEnumerator Burn()
    {
        if (onFire) yield break;

        int timeBurnt = 0;
        while (timeBurnt < FireBoat.burnTime)
        {
            yield return new WaitForSeconds(FireBoat.damageInterval);
            timeBurnt += FireBoat.damageInterval;
            ApplyDamage(FireBoat.fireDamage);
        }
    }

    /// <summary>
    /// Play moving sound
    /// </summary>
    private void SoundMoving()
    {
        audioSRC.PlayOneShot(boatMoving);
    }

    /// <summary>
    /// Play firing sound
    /// </summary>
    private void SoundFire()
    {
        audioSRC.PlayOneShot(aFlame);
    }

    /// <summary>
    /// Play kraken sound
    /// </summary>
    private void SoundMonster()
    {
        audioSRC.PlayOneShot(Monster);
    }

    /// <summary>
    /// Play sound when the damage is applied
    /// </summary>
    private void Impact()
    {
        audioSRC.PlayOneShot(Damaged);
    }

    /// <summary>
    /// Play sound when player dies
    /// </summary>
    private void SoundDeath()
    {
        audioSRC.PlayOneShot(Sink);
    }

    /// <summary>
    /// Shoot from one of the cannons depending on passed position
    /// </summary>
    /// <param name="position">Position of a cannon</param>
    protected void Shoot(VisionSphere.VisionPosition position)
    {
        if (position == VisionSphere.VisionPosition.Front) _cannonFront.Shoot(accuracy, reloadSpeed);
        else if (position == VisionSphere.VisionPosition.Left) _cannonLeft.Shoot(accuracy, reloadSpeed);
        else if (position == VisionSphere.VisionPosition.Right) _cannonRight.Shoot(accuracy, reloadSpeed);
    }

    /// <summary>
    /// Check if cannon is reloading
    /// </summary>
    /// <param name="position">Posiiton of a cannon</param>
    /// <returns></returns>
    protected bool CannonIsReloading(VisionSphere.VisionPosition position)
    {
        if (position == VisionSphere.VisionPosition.Front) return _cannonFront.reloading;
        if (position == VisionSphere.VisionPosition.Left) return _cannonLeft.reloading;
        if (position == VisionSphere.VisionPosition.Right) return _cannonRight.reloading;

        return false;
    }

    /// <summary>
    /// Get powerup ship is holding only if caller of the function is this ship's status manager
    /// </summary>
    /// <param name="caller">Caller of the function</param>
    /// <returns>Powerup ship is holding</returns>
    public PowerUpType GetPowerUp(object caller)
    {
        ShipStatsManager manager = caller as ShipStatsManager;
        if (manager != null && manager.GetShip(this) == this) return powerup;
        return PowerUpType.None;
    }

    /// <summary>
    /// Check if ship is burning but only if caller of the function is this ship's status manager
    /// </summary>
    /// <param name="caller">Caller of the function</param>
    /// <returns>Status of ship being on fire</returns>
    public bool IsBurning(object caller)
    {
        ShipStatsManager manager = caller as ShipStatsManager;
        if (manager != null && manager.GetShip(this) == this) return onFire;
        return false;
    }
}