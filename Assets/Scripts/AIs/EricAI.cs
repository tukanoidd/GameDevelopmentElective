using System;
using System.Collections;
using System.Linq;
using BaseScripts;
using UnityEngine;

public class EricAI : Ship
{
    [SerializeField] private bool wandering;
    [SerializeField] private bool atLeft = false;
    public override IEnumerator RunAI(object caller)
    {
        if (!(caller is CompetitionManager)) yield break;
        while (CompetitionManager.current.gameStarted && (!dying || !CompetitionManager.current.gameOver))
        {
            if (wandering)
            {
                yield return MoveForward(100);
                Shoot(VisionSphere.VisionPosition.Front);
                Shoot(VisionSphere.VisionPosition.Left);
                Shoot(VisionSphere.VisionPosition.Right);
                yield return EdgeDetection(1f);
                yield return Rotate(45, Direction.Left);
                yield return EdgeDetection(1f);
                Shoot(VisionSphere.VisionPosition.Front);
                Shoot(VisionSphere.VisionPosition.Left);
                Shoot(VisionSphere.VisionPosition.Right);
                yield return EdgeDetection(1f);
            }
            if (visiblePowerUps.Any() && !visibleShips.Any() && powerup == PowerUpType.Healing && health <= 50)
            {
                StartCoroutine(UseHealingPowerUp());
            }
            if (visiblePowerUps.Any() && !visibleShips.Any())
            {
                wandering = false;
                yield return PowerUpNear();
                wandering = true;
            }
            if (powerup == PowerUpType.Speeder)
            {
                StartCoroutine(UseSpeederPowerUp());
            }
            if (visibleShips.Any())
            {
                    wandering = false;
                    Shoot(VisionSphere.VisionPosition.Front);
                    yield return ShipNear();
                    if (powerup == PowerUpType.FireBoat)
                    {
                        StartCoroutine(UseFireBoatPowerUp());
                    }

                    if (powerup == PowerUpType.Kraken)
                    {
                        StartCoroutine(UseKrakenPowerUp());
                    }
            }
            if (!visibleShips.Any() && !visiblePowerUps.Any())
            {
                wandering = true;
            }
        }
    }
    private IEnumerator EdgeDetection(float direction)
    {
        if (Math.Abs(position.x) > 350 || Math.Abs(position.z) > 350)
        {
            wandering = false;
            Vector2 targetDir = new Vector2(-position.x, -position.z);
            yield return RotTo(targetDir);
            yield return MoveForward(50);
            wandering = true;
        }
    }
    private IEnumerator specRot(Vector3 otherPos)
    {
        Vector2 otherMapPos = new Vector2(otherPos.x, otherPos.z);
        Vector2 MePos = new Vector2(position.x, position.z);
        yield return RotTo(otherMapPos - MePos);

    }
    private IEnumerator RotTo(Vector2 direction)
            {
                Vector2 forward = transform.forward;
                float degree = Vector2.SignedAngle(direction, new Vector2(forward.x, forward.y));
                if (Math.Abs(degree) > 0.10f)
                    yield return Rotate(Math.Abs(degree), degree < 0 ? Direction.Left : Direction.Right);
            }
            private IEnumerator PowerUpNear()
            {
                UpdateVisiblePowerUps(visiblePowerUps);
                if (visiblePowerUps.Any())
                {
                    PowerUp powerNear = visiblePowerUps
                        .OrderBy(powerUp => Vector3.Distance(powerUp.transform.position, position))
                        .First();
                    yield return specRot(powerNear.transform.position);
                    yield return MoveForward(100);
                }
            }
            private IEnumerator ShipNear()
            {
                if (visibleShips.Any())
                {
                    Ship aShip = visibleShips.OrderBy(ship => Vector3.Distance(ship.transform.position, position))
                        .First();
                    yield return RotTo(aShip.transform.position - position);
                    if (!CannonIsReloading(VisionSphere.VisionPosition.Front))
                    {
                        Shoot(VisionSphere.VisionPosition.Front);
                    }
                    yield return MoveForward(20);
                }
            }
    }