using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseScripts;
using UnityEngine;

public class GradusAI : Ship
{
    private bool lowHealth => health <= 50;
    private bool hasHealing => powerup == PowerUpType.Healing;

    private bool canHeal => lowHealth && hasHealing;
    private float SeaSize = 350;
    private Ship Enemyship => visibleShips.Where(ship => Vector3.Distance(position, ship.position) <= 200 && !ship.dying)
        .OrderBy(ship => ship.health).FirstOrDefault();
    public override IEnumerator RunAI(object caller)
    {
        if (!(caller is CompetitionManager)) yield break;
        while (!dying || !CompetitionManager.current.gameOver || !CompetitionManager.current.gameStarted)
        {
            if (!visibleShips.Any() && !visiblePowerUps.Any())
            {
                VisionSphere.VisionPosition oldPos = visionSphere.position;
                VisionSphere.VisionPosition newPos = oldPos == VisionSphere.VisionPosition.Back
                    ? VisionSphere.VisionPosition.Left
                    : (oldPos == VisionSphere.VisionPosition.Left
                        ? VisionSphere.VisionPosition.Front
                        : (oldPos == VisionSphere.VisionPosition.Front
                            ? VisionSphere.VisionPosition.Right
                            : VisionSphere.VisionPosition.Back));

                yield return visionSphere.MoveToDirection(newPos);
                Shoot(VisionSphere.VisionPosition.Front);
                Shoot(VisionSphere.VisionPosition.Left);
                Shoot(VisionSphere.VisionPosition.Right);
                yield return GradusMoveForward(200);
            }
            if (lowHealth)
            {
                if (hasHealing)
                {
                    StartCoroutine(UseHealingPowerUp());
                }
                else
                {
                    IEnumerable<PowerUp> healingPowerUps =
                                            visiblePowerUps.Where(powerUp => powerUp.powerUpType == PowerUpType.Healing);
                    if (healingPowerUps.Any())
                    {
                        PowerUp nearestHealingPowerUp = healingPowerUps.OrderBy(powerUp =>
                            Vector3.Distance(powerUp.transform.position, position)).First();
                        yield return MoveTowards(nearestHealingPowerUp.transform.position);
                    }
                }
            }

            if (visibleShips.Any() && !visiblePowerUps.Any())
            {
                yield return ShootGradus();
            }

            if (powerup == PowerUpType.Kraken)
            {
                StartCoroutine(UseKrakenPowerUp());
            }

            if (visibleShips.Any() && health <= 50 && powerup == PowerUpType.Speeder)
            {
                StartCoroutine(UseSpeederPowerUp());
                yield return Hide();
            }

            if(visiblePowerUps.Any() && !visibleShips.Any())
            {
                yield return CloseToPowerUps();
            }
            if(visiblePowerUps.Any() && visibleShips.Any())
            {
                yield return CloseToPowerUps();
                yield return ShootGradus();
            }
        }
    }
    private IEnumerator GradusMoveForward(float distance)
    {
        yield return MoveForward(distance);
        if (Mathf.Abs(position.x) > SeaSize || Mathf.Abs(position.z) > SeaSize)
        {
            Vector2 rotationDirection = new Vector2(-position.x, -position.z);
            yield return RotateTowards(rotationDirection);
        }

    }
    private IEnumerator CloseToPowerUps()
    {
        UpdateVisiblePowerUps(visiblePowerUps);
        if (visiblePowerUps.Any())
        {
            StartCoroutine(UseHealingPowerUp());
            StartCoroutine(UseKrakenPowerUp());
            StartCoroutine(UseSpeederPowerUp());
            StartCoroutine(UseFireBoatPowerUp());
            PowerUp nearbyPowerUp = visiblePowerUps
                .OrderBy(powerUp => Vector3.Distance(powerUp.transform.position, position))
                .First();
            yield return MoveTowards(nearbyPowerUp.transform.position);
        }
    }
    private IEnumerator RotateTowards(Vector2 direction)
    {
        Vector3 forward = transform.forward;
        float angle = Vector2.SignedAngle(direction, new Vector2(forward.x, forward.z));
        if (Math.Abs(angle) > 0.10f)
            yield return Rotate(Math.Abs(angle), angle < 0 ? Direction.Left : Direction.Right);
    }
    private IEnumerator MoveTowards(Vector3 posTarget)
    {
        Vector2 AIMapPos = new Vector2(position.x, position.z);
        Vector2 targetMapPos = new Vector2(posTarget.x, posTarget.z);

        yield return visionSphere.MoveToDirection(VisionSphere.VisionPosition.Front);
        yield return RotateTowards(targetMapPos - AIMapPos);
        yield return GradusMoveForward(Vector2.Distance(targetMapPos, AIMapPos));
    }

    private IEnumerator Hide()
    {
        yield return Rotate(20, Direction.Right);
        yield return GradusMoveForward(100);
    }
    private IEnumerator ShootGradus()
    {
        if (visibleShips.Any())
        {
            Ship closestShip = visibleShips
                .OrderBy(ship => Vector3.Distance(ship.transform.position, position))
                .First();
            RotateTowards(closestShip.transform.position);    
            
            if (powerup == PowerUpType.FireBoat == health >= 50)
            {
                StartCoroutine(UseFireBoatPowerUp());
            }
            if (VisionSphere.VisionPosition.Left == visionSphere.position)
            {
                Shoot(VisionSphere.VisionPosition.Left);
            }
            if (VisionSphere.VisionPosition.Right == visionSphere.position)
            {
                Shoot(VisionSphere.VisionPosition.Right);
            }
            if (VisionSphere.VisionPosition.Front == visionSphere.position)
            {
                Shoot(VisionSphere.VisionPosition.Front);
            }
        }
        
        yield return GradusMoveForward(10);
    }
}