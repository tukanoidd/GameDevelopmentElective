using System;
using System.Collections;
using System.Linq;
using BaseScripts;
using UnityEngine;

public class EricAI : Ship
{
    [SerializeField] private bool wandering;
    [SerializeField] private bool atLeft = false;

    private Ship shipNear => visibleShips.Where(ship => Vector3.Distance(position, ship.position) <= 200 && !ship.dying)
        .OrderBy(ship => ship.health).FirstOrDefault();

    public override IEnumerator RunAI(object caller)
    {
        if (!(caller is CompetitionManager)) yield break;
        while ((CompetitionManager.current.gameStarted) && (!dying || !CompetitionManager.current.gameOver))
        {
            if (wandering)
            {
                for (int i = 0; i < 100; i++)
                {
                    yield return EdgeDetection(1f);
                    yield return MoveForward(300);
                    Shoot(VisionSphere.VisionPosition.Front);
                    Shoot(VisionSphere.VisionPosition.Left);
                    Shoot(VisionSphere.VisionPosition.Right);
                    yield return EdgeDetection(1f);
                    // yield return Rotate(45, Direction.Left);
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

                if (visiblePowerUps.Any())
                {
                    Debug.Log("Powerups!");
                }


                if (visiblePowerUps.Any() && !visibleShips.Any())
                {
                    Debug.Log("found shit!");
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
                    Debug.Log("Ship ahoy");
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
    }

    private IEnumerator EdgeDetection(float direction)
    {
        if (Math.Abs(position.x) > 400 || Math.Abs(position.z) > 400)
        {
            wandering = false;

            Vector2 targetDir = new Vector2(-position.x, -position.z);
            yield return RotTo(targetDir);


            yield return MoveForward(10);
            wandering = true;
        }
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
            Vector2 nearestPower = new Vector2(powerNear.transform.position.x, powerNear.transform.position.y);
            Debug.Log("power-up in sight");
            yield return RotTo(nearestPower);
            yield return MoveForward(200);
        }
    }

    private IEnumerator ShipNear()
    {
        Vector2 nearShip = new Vector2(shipNear.transform.position.x, shipNear.transform.position.y);
        Debug.Log("enemy spotted");
        yield return RotTo(nearShip);
        yield return MoveForward(200);
        Shoot(VisionSphere.VisionPosition.Front);
    }
}