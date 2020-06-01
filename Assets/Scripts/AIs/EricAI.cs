﻿using System;
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



    [SerializeField] private bool wandering = true;
    [SerializeField] private bool atBack = false;


    public override IEnumerator RunAI(object caller)
    {
        if (!(caller is CompetitionManager)) yield break;
        while (dying == false || !CompetitionManager.current.gameOver || !CompetitionManager.current.gameStarted)
        {
            if (atBack == false)
            {
                //yield return visionSphere.MoveToDirection(VisionSphere.VisionPosition.Back);
                atBack = true;
            }
            else
            {
                //yield return visionSphere.MoveToDirection(VisionSphere.VisionPosition.Back);
                //yield return visionSphere.MoveToDirection(VisionSphere.VisionPosition.Front);
                atBack = false;
            }

            if (wandering)
            {
                for (int i = 0; i < 100; i++)
                {
                    //yield return visionSphere.MoveToDirection(VisionSphere.VisionPosition.Back);
                    yield return EdgeDetection(1f);

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
            wandering = false;

            Vector2 targetDir = new Vector2(-position.x, -position.y);
            yield return RotTo(targetDir);
            
            

            //transform.LookAt(Vector3.zero);

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
}


