using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController
{
    private int time = 0;
    private float invTimeMax = 1.0f;

    public void Set(int maxTime)
    {
        Debug.Assert(0 < maxTime);

        time = maxTime;
        invTimeMax = 1.0f / (float)maxTime;
    }

    public bool Update()
    {
        time = Math.Max(--time, 0);
        return (0 < time);
    }

    public float GetNormalized()
    {
        return invTimeMax * (float)time;
    }
}