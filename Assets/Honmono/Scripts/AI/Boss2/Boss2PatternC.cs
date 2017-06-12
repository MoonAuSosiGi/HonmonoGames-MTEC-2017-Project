﻿using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class Boss2PatternC : PatternState
{
    public Boss2PatternC(SkeletonAnimation ani , string moveAni , string attackAni , string aiTarget) : base(ani , moveAni , attackAni , aiTarget)
    {
        // 꼬리 분리
    }
    public override float Attack(GameObject hero , GameObject me , int index)
    {

        return 1.0f;
    }

    public override void Move(GameObject target , GameObject hero)
    {

    }

    public override float PreProcessedDamge()
    {
        return 1.0f;
    }
}
