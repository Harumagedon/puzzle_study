using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController
{
    private float time = 0.0f;
    private float invTimeMax = 1.0f;
    private const float DELTA_TIME_MAX = 1.0f;

    public void Set(float maxTime)
    {
        Debug.Assert(0.0f < maxTime);

        time = maxTime;
        invTimeMax = 1.0f / maxTime;
    }

    public bool Update(float deltaTime)
    {
        if (DELTA_TIME_MAX < deltaTime) deltaTime = DELTA_TIME_MAX;

        time -= deltaTime;

        if (time <= 0.0f)
        {
            time = 0.0f;
            return false;
        }

        return true;
    }

    public float GetNormalized()
    {
        return time * invTimeMax;
    }
}