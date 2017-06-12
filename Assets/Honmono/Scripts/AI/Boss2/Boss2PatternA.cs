using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class Boss2PatternA : PatternState
{
    // -- 기본 정보 --------------------------------------------------------------------------------
    private float m_patternTick = 0.0f;
    private Vector3 m_targetPos = Vector3.zero;

    public Boss2PatternA(SkeletonAnimation ani , string moveAni , string attackAni , string aiTarget) : base(ani , moveAni , attackAni , aiTarget)
    {
        // 플레이어 위치를 추적 직선으로 날아가서 돌격
        // 플레이어를 추적하며 5초간 날아감
        // 데미지 처리는 해당 위치에서 

    }   

    public void SetHeroPos(Vector3 targetPos)
    {
        m_targetPos = targetPos;
    }

    public override float Attack(GameObject hero , GameObject me , int index)
    {

        // 사용되지 않음
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
