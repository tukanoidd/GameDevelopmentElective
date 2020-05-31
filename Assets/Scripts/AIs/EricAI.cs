using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseScripts;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class EricAI : Ship
{
    public bool wandering = true;
    public bool atBack = false;

    

    public override IEnumerator RunAI(object caller)
    {

        if (!(caller is CompetitionManager)) yield break;
        while (dying == false || !CompetitionManager.current.gameOver || !CompetitionManager.current.gameStarted)
        {
            if (atBack == false)
            {
                StartCoroutine(visionSphere.MoveToDirection(VisionSphere.VisionPosition.Back));
                atBack = true;
                Debug.Log("to the back");
            }
            else
            {
                StopCoroutine(visionSphere.MoveToDirection(VisionSphere.VisionPosition.Back));
                StartCoroutine(visionSphere.MoveToDirection(VisionSphere.VisionPosition.Front));
                atBack = false;
                Debug.Log("To the front");
            }
           
            
          
         
            if (wandering)
            {

                for (int i = 0; i < 1000; i++)
                {
                    StartCoroutine(visionSphere.MoveToDirection(VisionSphere.VisionPosition.Back));
                  
                    yield return MoveForward(50);
                    Shoot(VisionSphere.VisionPosition.Front);
                    Shoot(VisionSphere.VisionPosition.Left);
                    Shoot(VisionSphere.VisionPosition.Right);
                    yield return Rotate(90, Direction.Right);
                    Shoot(VisionSphere.VisionPosition.Front);
                    Shoot(VisionSphere.VisionPosition.Left);
                    Shoot(VisionSphere.VisionPosition.Right);


                }




            }
           

           


        }
    }
}
