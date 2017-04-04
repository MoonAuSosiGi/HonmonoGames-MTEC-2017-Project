using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternD : PatternState
{
    private float m_tick = 0;

    public override float Attack(GameObject hero)
    {
        m_tick += Time.deltaTime;

        // 패턴 D
        // 특수패턴

        // 일정 시간 지난 후
        if (m_tick >= GameSetting.BOSS1_PATTERN_D_SPECIAL)
        {
            // SPECIAL ATTACK

            // 웜홀로 사라지는 이펙트
            
            // 순간이동
        }
        return 0.0f;
    }

    public override float PreProcessedDamge()
    {
        // 데미지의 연산을 어떻게 처리할 것인가에 대한 것.
        // 데미지 경감

        return GameSetting.BOSS1_PATTERN_D_DEF;
    }

    public override void Move(GameObject target, GameObject hero)
    {
        Vector3 dir = hero.transform.position - target.transform.position;
        dir.Normalize();

        // 임의로 랜덤이동
        target.transform.position += dir * Time.deltaTime;
    }
}
