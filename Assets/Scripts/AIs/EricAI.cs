using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using BaseScripts;
using TMPro;
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

                for (int i = 0; i < 100; i++)
                {
                    StartCoroutine(visionSphere.MoveToDirection(VisionSphere.VisionPosition.Back));
                    StartCoroutine(EdgeDetection(1f));
                    
                    yield return Rotate(45, Direction.Left);
                    Shoot(VisionSphere.VisionPosition.Front);
                    Shoot(VisionSphere.VisionPosition.Left);
                    Shoot(VisionSphere.VisionPosition.Right);
                    yield return MoveForward(100);
                    Shoot(VisionSphere.VisionPosition.Front);
                    Shoot(VisionSphere.VisionPosition.Left);
                    Shoot(VisionSphere.VisionPosition.Right);
                   
                   
                    
                    


                }

              





            }
        }
    }

    private IEnumerator EdgeDetection(float direction)
    {
        if (Math.Abs(position.x) > 410 || Math.Abs(position.z) > 410)
        {
            Debug.Log("at the edge");
            wandering = false;
            transform.LookAt(Vector3.zero);
            yield return MoveForward(10);
            wandering = true;
        }
    }
}
