using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOSS : MonoBehaviour {


    // -- 기본 정보 --------------------------------------------------------------------------------//
    // 패턴 정보
    protected PatternState m_pattern = null;

    // 기본 스탯
    [SerializeField]
    protected float m_fullHp = 100.0f;  // 풀 체력
    [SerializeField]
    protected float m_hp = 100.0f;      // 체력
    [SerializeField]
    protected float m_moveSpeed = 3.0f; // 이동속도


    // --------------------------------------------------------------------------------------------//

    // -- AI 관련 메서드 --------------------------------------------------------------------------//

    public float Attack()
    {
        float coolTime = 0.0f;
        if (m_pattern != null)
            coolTime = m_pattern.Attack(GameManager.Instance().PLAYER.PLAYER_HERO.gameObject);
        return coolTime;
    }

    // 데미지를 입는 처리
    public void Damage(float damage)
    {
        if (m_pattern == null)
            return;
        // 그전의 방어력 경감 효과가 있는 상태인지 체크
        float per = m_pattern.PreProcessedDamge();

        m_hp -= (damage * per);
        // UI 가 바뀌는 처리는 일단 여기서 하지 않는다.
    }

    // 이동 
    protected void Move()
    {
        if (m_pattern != null)
            m_pattern.Move(gameObject, GameManager.Instance().PLAYER.PLAYER_HERO.gameObject);
    }

    // -------------------------------------------------------------------------------------------//

}
