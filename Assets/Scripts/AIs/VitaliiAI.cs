using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseScripts;
using UnityEngine;

public class VitaliiAI : Ship
{
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

                yield return VitaliiAIMove(10);
            } else if (visibleShips.Any() && !visiblePowerUps.Any())
            {
                Ship shipToAttack = visibleShips
                    .Where(ship => Vector3.Distance(position, ship.position) <= 200 && !ship.dying)
                    .OrderBy(ship => ship.health).FirstOrDefault();

                if (shipToAttack)
                {
                    if (health < 50)
                    {
                        if (powerup == PowerUpType.Healing) yield return UseHealingPowerUp();
                        else if (shipToAttack.HasPowerup) yield return VitaliiAIMove(100);
                    }
                    else
                    {
                        //todo logic
                    }   
                }
            } else if (!visibleShips.Any() && visiblePowerUps.Any())
            {
                //todo logic
            } else if (visibleShips.Any() && visiblePowerUps.Any())
            {
                //todo logic       
            }
        }
    }

    private IEnumerator VitaliiAIMove(float distance)
    {
        if (Math.Abs(position.x) > 2000 || Math.Abs(position.z) > 2000)
        {
            Vector2 dir = new Vector2(-position.x, -position.z);
            Vector3 forward = transform.forward;
            float angle = Vector2.SignedAngle(dir, new Vector2(forward.x, forward.z));

            if (Math.Abs(angle) > 0.10f)
                yield return Rotate(Math.Abs(angle), angle < 0 ? Direction.Left : Direction.Right);
        }

        yield return MoveForward(distance);
    }
}