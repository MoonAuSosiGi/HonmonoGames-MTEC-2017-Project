using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternC : PatternState
{

    private float m_tick = 0;

    public override float Attack(GameObject hero)
    {
        m_tick += Time.deltaTime;

        // 패턴 A
        // 플레이어 위치를 추적하여 직선으로 날아가는 탄환 발사

        // 탄환 3발 발사
        // 2초마다 한번씩
        if (m_tick >= GameSetting.BOSS1_PATTERN_C_ATTACK_COOLTIME)
        {
           // BulletManager.Instance().AddBullet(BulletManager.BULLET_TYPE.B_BOSS1_P1);
            // 총알은 2.5초에 걸쳐 플레이어에게 도착함
            // 총알의 계산은 총알에서 하도록 함
            m_tick = 0.0f;
        }
        return 0.0f;
    }

    public override float PreProcessedDamge()
    {
        // 데미지의 연산을 어떻게 처리할 것인가에 대한 것.
        // 패턴 A 에선 들어오는 데미지의 100%를 받는다.

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
