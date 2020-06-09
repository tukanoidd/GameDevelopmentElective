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

    private Ship Enemyship => visibleShips.Where(ship => Vector3.Distance(position, ship.position) <= 200 && !ship.dying)
        .OrderBy(ship => ship.health).FirstOrDefault();
    public override IEnumerator RunAI(object caller)
    {
        if (!(caller is CompetitionManager)) yield break;
        while (!dying || !CompetitionManager.current.gameOver || !CompetitionManager.current.gameStarted)
        {
            
            if(lowHealth == true)
            {
                Debug.Log("Need Healing");

                IEnumerable<PowerUp> healingPowerUps =
                          visiblePowerUps.Where(powerUp => powerUp.powerUpType == PowerUpType.Healing);

                if (healingPowerUps.Any())
                {
                    PowerUp nearestHealingPowerUp = healingPowerUps.OrderBy(powerUp =>
                        Vector3.Distance(powerUp.transform.position, position)).First();
                    Debug.Log("Let's do some healing");
                    yield return MoveTowards(nearestHealingPowerUp.transform.position);
                    yield break;
                }
            }
            yield return MoveForward(10);
        }
    }

    private IEnumerator RotateTowards(Vector2 direction)
    {
        float angle = Vector2.Angle(direction, transform.forward);
        yield return Rotate(angle, Direction.Left);
    }
    private IEnumerator MoveTowards(Vector3 posTarget)
    {
        Vector2 AIMapPos = new Vector2(position.x, position.z);
        Vector2 targetMapPos = new Vector2(posTarget.x, posTarget.z);

        yield return visionSphere.MoveToDirection(VisionSphere.VisionPosition.Front);
        yield return RotateTowards(targetMapPos - AIMapPos);
        yield return MoveForward(Vector2.Distance(targetMapPos, AIMapPos));
    }
}
