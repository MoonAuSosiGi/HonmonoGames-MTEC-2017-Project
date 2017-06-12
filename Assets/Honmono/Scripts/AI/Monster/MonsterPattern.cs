using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class MonsterPattern : PatternState
{
    float m_tick = 0.0f;

    public MonsterPattern(SkeletonAnimation ani , string moveAni , string attackAni,string aiTarget) : base(ani , moveAni , attackAni,aiTarget)
    {
    }


    public override float Attack(GameObject hero, GameObject me, int index)
    {
        //m_tick += Time.deltaTime;

        //if(m_tick >= 1000.0f)
        //{
            m_skletonAnimation.state.SetAnimation(0 , m_attackAni , false);
        //    m_tick = 0.0f;
        //}
        TutoRobo r = hero.GetComponent<TutoRobo>();
        if(r != null)
            r.Damage(1);
        else
        {
            HeroRobo robo = hero.GetComponent<HeroRobo>();
            if (robo != null)
                robo.Damage(1);
        }
        
        return 1.0f;
    }

    public override void Move(GameObject target, GameObject hero)
    {
        Vector3 dir = hero.transform.position - target.transform.position;
        dir.Normalize();

        // 임의로 랜덤이동
        target.transform.position += dir * Time.deltaTime;

        //m_skletonAnimation.state.SetAnimation(0 , m_moveAni , true);
    }

    public override float PreProcessedDamge()
    {
        return 1.0f;
    }
}
