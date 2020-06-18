using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenAliens : AlienEntity
{
    [SerializeField]
    private float startWalkingAlienTime, pauseTimeBeforeChangeToRed;

    protected override void Start()
    {
        alienType = "Green";

        alienDirection = Direction.NONE;
        StartCoroutine(StartMoveLeft(startWalkingAlienTime));

        base.Start();
    }
    
    protected override void Update()
    {
        HandleGreenAliensPatrolingAI();
        AlienGreenToRed();

        base.Update();
    }

    private IEnumerator StartMoveLeft(float time)
    {
        yield return new WaitForSeconds(time);

        alienDirection = Direction.LEFT;
    }

    private void HandleGreenAliensPatrolingAI()
    {
        if ((canGoDown || canGoUp) && chooseOnce && CheckOnGround())
        {
            SelectRandomDirection();
        }

        if (!canGoUp && alienDirection == Direction.UP && CheckOnGround())
        {
            SelectRandomDirection();
        }

        if (!canGoDown && alienDirection == Direction.DOWN && CheckOnGround())
        {
            SelectRandomDirection();
        }
    }

    //Change the alien from Green to Red when Green alien trapped in floor and not killed from player.
    private void AlienGreenToRed()
    {
        if (!isInFloor)
        {
            return;
        }

        //Pause until green alien jumps out
        if (Pause(pauseTimeBeforeChangeToRed))
        {
            myGameController.GreenAlienPosInit(gameObject.transform.position);
            myGameController.greenAliens--;
            myGameController.redAliens++;
            SelfDestroy(false);
        }
    }

    // Choose a random direction when alien collide with a ladder
    private void SelectRandomDirection()
    {
        switch (alienDirection)
        {
            case Direction.RIGHT:
                ChangeAIDirectionYAxis(Direction.RIGHT);
                break;
            case Direction.LEFT:
                ChangeAIDirectionYAxis(Direction.LEFT);
                break;
            case Direction.UP:
                ChangeAIDirectionXAxis(Direction.UP , !canGoUp);
                break;
            case Direction.DOWN:
                ChangeAIDirectionXAxis(Direction.DOWN , !canGoDown);
                break;
        }

        chooseOnce = false;
    }

    private void ChangeAIDirectionYAxis(Direction direction)
    {
        // Choose a random number for alien to move randomly when touch the ladder
        if (canGoUp && canGoDown)
        {
            switch (UnityEngine.Random.Range(1, 4))
            {
                case 1:
                    alienDirection = direction;
                    break;
                case 2:
                    StopX();
                    alienDirection = Direction.UP;
                    break;
                case 3:
                    StopX();
                    alienDirection = Direction.DOWN;
                    break;
            }
        }
        else if (!canGoUp)
        {    
            switch (UnityEngine.Random.Range(1, 3))
            {
                case 1:
                    alienDirection = direction;
                    break;
                case 2:
                    StopX();
                    alienDirection = Direction.DOWN;
                    break;
            }
        }
        else if (!canGoDown)
        {
            switch (UnityEngine.Random.Range(1, 3))
            {
                case 1:
                    alienDirection = direction;
                    break;
                case 2:
                    StopX();
                    alienDirection = Direction.UP;
                    break;
            }
        }
    }

    private void ChangeAIDirectionXAxis(Direction direction, bool canGoUpOrDown)
    {
        // Choose a random number for alien to move randomly when touch the ladder
        if (canGoUpOrDown)
        {   
            switch (UnityEngine.Random.Range(1, 3))
            {
                case 1:
                    StopY();
                    alienDirection = Direction.LEFT;
                    break;
                case 2:
                    StopY();
                    alienDirection = Direction.RIGHT;
                    break;
            }
        }
        else
        {
            switch (UnityEngine.Random.Range(1, 4))
            {
                case 1:
                    StopY();
                    alienDirection = Direction.LEFT;
                    break;
                case 2:
                    StopY();
                    alienDirection = Direction.RIGHT;
                    break;
                case 3:
                    alienDirection = direction;
                    break;
               
            }
        }
    }
}