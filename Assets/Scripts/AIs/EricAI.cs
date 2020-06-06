using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class EricAI : Ship
{
    [SerializeField] private bool wandering = true;
    [SerializeField] private bool atBack = false;

    private Ship shipNear => visibleShips.Where(ship => Vector3.Distance(position, ship.position) <= 200 && !ship.dying)
        .OrderBy(ship => ship.health).FirstOrDefault();
    
    public override IEnumerator RunAI(object caller)
    {
        if (!(caller is CompetitionManager)) yield break;
        while ((CompetitionManager.current.gameStarted) && (!dying || !CompetitionManager.current.gameOver))
        {
            // if (atBack == false)
            //{
            // yield return visionSphere.MoveToDirection(VisionSphere.VisionPosition.Back);
            //atBack = true;
            // }
            //else
            //{
            // yield return visionSphere.MoveToDirection(VisionSphere.VisionPosition.Back);
            //yield return visionSphere.MoveToDirection(VisionSphere.VisionPosition.Front);
            // atBack = false;
            //}

            if (wandering)
            {
                for (int i = 0; i < 100; i++)
                {
                    yield return MoveForward(100);
                    Shoot(VisionSphere.VisionPosition.Front);
                    Shoot(VisionSphere.VisionPosition.Left);
                    Shoot(VisionSphere.VisionPosition.Right);
                    // yield return visionSphere.MoveToDirection(VisionSphere.VisionPosition.Back);
                    yield return EdgeDetection(1f);

                    yield return Rotate(45, Direction.Left);
                    Shoot(VisionSphere.VisionPosition.Front);
                    Shoot(VisionSphere.VisionPosition.Left);
                    Shoot(VisionSphere.VisionPosition.Right);
                }

                if (visiblePowerUps.Any())
                {
                    Debug.Log("found shit!");
                    wandering = false;
                    yield return PowerUpNear();
                    wandering = true;
                }

                if (visibleShips.Any() && !visiblePowerUps.Any())
                {
                    wandering = false;
                    yield return ShipNear();
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
        if (Math.Abs(position.x) > 410 || Math.Abs(position.z) > 410)
        {
            wandering = false;

            Vector2 targetDir = new Vector2(-position.x, -position.y);
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
        PowerUp powerNear = visiblePowerUps.OrderBy(powerUp => Vector3.Distance(powerUp.transform.position, position))
            .First();
        Vector2 nearestPower = new Vector2(powerNear.transform.position.x, powerNear.transform.position.y);
        Debug.Log("power-up in sight");
        yield return RotTo(nearestPower);
        yield return MoveForward(50);
    }

    private IEnumerator ShipNear()
    {
        Vector2 nearShip = new Vector2(shipNear.transform.position.x, shipNear.transform.position.y);
        Debug.Log("enemy spotted");
        yield return RotTo(nearShip);
        yield return MoveForward(50);
        Shoot(VisionSphere.VisionPosition.Front);
    }
}