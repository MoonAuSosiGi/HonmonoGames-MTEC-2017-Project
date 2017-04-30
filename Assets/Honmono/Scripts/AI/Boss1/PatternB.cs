using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
public class PatternB : PatternState
{
    public PatternB(SkeletonAnimation ani) : base(ani) { }
    private float m_tick = 0;
    private bool m_attack = false;

    public override bool GetAttack()
    {
        return m_attack;
    }

    public override float Attack(GameObject hero,GameObject me, int index)
    {
        m_tick += Time.deltaTime;

        // 패턴 A
        // 플레이어 위치를 추적하여 직선으로 날아가는 탄환 발사

        // 탄환 3발 발사
        // 2초마다 한번씩
        if (m_tick >= GameSetting.BOSS1_PATTERN_B_ATTACK_COOLTIME)
        {
            //  BulletManager.Instance().AddBullet(BulletManager.BULLET_TYPE.B_BOSS1_P1);
            // 총알은 2.5초에 걸쳐 플레이어에게 도착함
            // 총알의 계산은 총알에서 하도록 함
            GameObject bullet = BulletManager.Instance().AddBullet(BulletManager.BULLET_TYPE.B_BOSS1_P1);
            Bullet b = bullet.GetComponent<Bullet>();
            Vector3 dir = hero.transform.position - me.transform.position;
            dir.Normalize();
            float distance = Vector3.Distance(hero.transform.position, me.transform.position);
            string name = GameManager.Instance().PLAYER.USER_NAME + "_boss_B_" + Monster.m_index++;
            b.SetupBullet(name, false, dir);
            b.BULLET_SPEED = 20.0f;
            b.transform.position = me.transform.parent.position;
            Vector3 pos = me.transform.position;
            b.transform.position = pos;
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonCreateOrder(name, "boss1_bullet", pos.x, pos.y, b.transform.rotation.eulerAngles.y, bullet.GetComponent<SpriteRenderer>().flipX));
            m_tick = 0.0f;
            Animator ani = me.GetComponent<Stage1BOSS>().m_attackEffect;
            ani.Play("boss_shotEffect");
            return GameSetting.BOSS1_PATTERN_B_ATTACK_COOL;
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
        target.transform.position += dir * GameSetting.BOSS1_SPEED * Time.deltaTime;
    }
}
