using System.Collections;
using System.Collections.Generic;
using BaseScripts;
using UnityEngine;

public class GradusAI : Ship
{
    public override IEnumerator RunAI(object caller)
    {
        if (!(caller is CompetitionManager)) yield break;
        while (!dying || !CompetitionManager.current.gameOver || !CompetitionManager.current.gameStarted)
        {
            yield return MoveForward(200);
        }
    }
}
