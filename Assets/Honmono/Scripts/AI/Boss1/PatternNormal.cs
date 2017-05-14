using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class PatternNormal : PatternState
{
    public PatternNormal(SkeletonAnimation ani , string moveAni , string attackAni,string aiTarget) : base(ani , moveAni , attackAni,aiTarget) { }
    private bool m_attack = false;


    public override float Attack(GameObject hero,GameObject me, int index)
    {
        // 기본공격
        MDebug.Log("기본 공격을 했다.");
        
        return GameSetting.BOSS1_DEF_ATTACK_COOLTIME;
    }

    public override float PreProcessedDamge()
    {
        return 1.0f;   
    }

    public override void Move(GameObject target, GameObject hero)
    {
        Vector3 dir = hero.transform.position - target.transform.position;
        dir.Normalize();

        // 임의로 랜덤이동
        target.transform.position += dir * Time.deltaTime;
    }
}
