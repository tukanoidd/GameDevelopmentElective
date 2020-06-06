using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseScripts;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class VitaliiAI : Ship
{
    private bool NeedHealing => health < 50;
    private bool CanHeal => powerup == PowerUpType.Healing;
    private bool NeedAndCanHeal => NeedHealing && CanHeal;

    private Ship ShipToAttack => visibleShips
        .Where(ship => Vector3.Distance(position, ship.position) <= 200 && !ship.dying)
        .OrderBy(ship => ship.health).FirstOrDefault();

    public override IEnumerator RunAI(object caller)
    {
        if (!(caller is CompetitionManager)) yield break;
        while ((CompetitionManager.current.gameStarted) && (!dying || !CompetitionManager.current.gameOver))
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

                yield return Explore();
            }
            else if (visibleShips.Any() && !visiblePowerUps.Any())
            {
                Ship shipToAttack = ShipToAttack;

                if (shipToAttack)
                {
                    if (NeedHealing)
                    {
                        if (CanHeal) StartCoroutine(UseHealingPowerUp());
                        else if (shipToAttack.HasPowerup)
                        {
                            if (powerup == PowerUpType.Speeder) StartCoroutine(UseSpeederPowerUp());
                            yield return Flee();
                        }
                    }
                    else yield return VitaliiAIShoot(shipToAttack);
                }
            }
            else if (!visibleShips.Any() && visiblePowerUps.Any())
            {
                if (CanHeal && health < 100 - Healing.amountToAdd)
                    StartCoroutine(UseHealingPowerUp());

                if (HasPowerup) yield return Explore();
                else yield return FindAndGoToNearestPowerUp();
            }
            else if (visibleShips.Any() && visiblePowerUps.Any())
            {
                if (NeedAndCanHeal) StartCoroutine(UseHealingPowerUp());

                Ship shipToAttack = ShipToAttack;

                if (NeedHealing)
                {
                    IEnumerable<PowerUp> healingPowerUps =
                        visiblePowerUps.Where(powerUp => powerUp.powerUpType == PowerUpType.Healing);

                    if (healingPowerUps.Any())
                    {
                        PowerUp nearestHealingPowerUp = healingPowerUps.OrderBy(powerUp =>
                            Vector3.Distance(powerUp.transform.position, position)).First();

                        yield return VitaliiAIMoveToward(nearestHealingPowerUp.transform.position);
                        yield break;
                    }

                    yield return CheckShipAndShoot(shipToAttack);
                }
                else
                {
                    if (!HasPowerup) yield return FindAndGoToNearestPowerUp();

                    yield return CheckShipAndShoot(shipToAttack);
                }
            }
        }
    }

    private IEnumerator Explore()
    {
        yield return VitaliiAIMove(50);
    }

    private IEnumerator Flee()
    {
        yield return VitaliiAIMove(100);
    }

    private IEnumerator VitaliiAIMove(float distance)
    {
        if (Math.Abs(position.x) > 350 || Math.Abs(position.z) > 350)
        {
            Vector2 dir = new Vector2(-position.x, -position.z);
            yield return RotateToward(dir);
        }

        yield return MoveForward(distance);
    }

    private IEnumerator RotateToward(Vector2 direction)
    {
        Vector3 forward = transform.forward;
        float angle = Vector2.SignedAngle(direction, new Vector2(forward.x, forward.z));

        if (Math.Abs(angle) > 0.10f)
            yield return Rotate(Math.Abs(angle), angle < 0 ? Direction.Left : Direction.Right);
    }

    private IEnumerator VitaliiAIMoveToward(Vector3 targetPosition)
    {
        Vector2 targetMapPos = new Vector2(targetPosition.x, targetPosition.z);
        Vector2 mapPos = new Vector2(position.x, position.z);

        yield return RotateToward(targetMapPos - mapPos);
        yield return visionSphere.MoveToDirection(VisionSphere.VisionPosition.Front);
        yield return MoveForward(Vector2.Distance(targetMapPos, mapPos));
    }

    private IEnumerator VitaliiAIShoot(Ship shipToAttack)
    {
        if (powerup == PowerUpType.Kraken) StartCoroutine(UseKrakenPowerUp(shipToAttack));
        if (powerup == PowerUpType.FireBoat) StartCoroutine(UseFireBoatPowerUp());

        bool shoot = true;

        bool rotate = false;
        float rotationAngle = 90;
        Direction rotationDirection = Direction.Right;

        VisionSphere.VisionPosition shootDirection = visionSphere.position;
        if (visionSphere.position == VisionSphere.VisionPosition.Back)
        {
            rotate = true;

            if (!CannonIsReloading(VisionSphere.VisionPosition.Right))
                shootDirection = VisionSphere.VisionPosition.Right;
            else if (!CannonIsReloading(VisionSphere.VisionPosition.Left))
            {
                rotationDirection = Direction.Left;
                shootDirection = VisionSphere.VisionPosition.Left;
            }
            else if (!CannonIsReloading(VisionSphere.VisionPosition.Front))
            {
                rotationAngle = 180;
                shootDirection = VisionSphere.VisionPosition.Front;
            }
            else shoot = false;
        }
        else if (CannonIsReloading(shootDirection)) shoot = false;

        if (shoot)
        {
            if (rotate) yield return Rotate(rotationAngle, rotationDirection);
            Shoot(shootDirection);
        }
        else yield return Explore();
    }

    private IEnumerator FindAndGoToNearestPowerUp()
    {
        UpdateVisiblePowerUps(visiblePowerUps);
        if (!visiblePowerUps.Any()) yield return Explore();
        else
        {
            PowerUp nearestPowerUp = visiblePowerUps
                .OrderBy(powerUp => Vector3.Distance(powerUp.transform.position, position)).First();

            yield return VitaliiAIMoveToward(nearestPowerUp.transform.position);   
        }
    }

    private IEnumerator CheckShipAndShoot(Ship shipToAttack)
    {
        if (shipToAttack)
        {
            if (shipToAttack.HasPowerup) yield return Flee();
            else yield return VitaliiAIShoot(shipToAttack);
        }   
    }
}