using System.Collections;
using System.Collections.Generic;
using BaseScripts;
using UnityEngine;

public class GradusAI : Ship
{
    [SerializeField]private bool lowHealth;
    public override IEnumerator RunAI(object caller)
    {
        if (!(caller is CompetitionManager)) yield break;
        while (!dying || !CompetitionManager.current.gameOver || !CompetitionManager.current.gameStarted)
        {
            if(health <= 50)
            {
                lowHealth = true;
            }
            if(lowHealth == true)
            {
                
            }
            yield return MoveForward(10);
        }
    }

    private IEnumerator MoveTowards(Vector3 posTarget)
    {
        yield return null;
    }
}
