using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
public class Monster : MonoBehaviour {


    // -- 기본 정보 --------------------------------------------------------------------------------//

    // 누가 체크할것인지
    protected string m_originUser = "";
        
    // 패턴 정보
    protected PatternState m_pattern =  null;

    public PatternState PATTERN { get { return m_pattern; } }

    // 기본 스탯
    [SerializeField]
    protected float m_fullHp = 100.0f;  // 풀 체력
    [SerializeField]
    protected float m_hp = 100.0f;      // 체력
    [SerializeField]
    protected float m_moveSpeed = 3.0f; // 이동속도


    public static  int m_index = 0;

    protected SkeletonAnimation m_skeletonAnimation = null;


    protected string m_name = null;
    public string MONSTER_NAME { get { return m_name; } set { m_name = value; } }

    // --------------------------------------------------------------------------------------------//
    // -- 스파인 애니메이션용 -------------------------------------------------------//
    protected bool IsCurrentAnimation(int i ,string ani)
    {
        if (m_skeletonAnimation == null || m_skeletonAnimation.state.GetCurrent(i) == null)
            return false;
        return m_skeletonAnimation.state.GetCurrent(i).animation.name == ani;
    }

    protected void CheckAndSetAnimation(int i,string ani , bool loop)
    {
        if (IsCurrentAnimation(i,ani) == false)
            m_skeletonAnimation.state.SetAnimation(i , ani , loop);
    }

    //-------------------------------------------------------------------------------//
    // -- AI 관련 메서드 --------------------------------------------------------------------------//

    public virtual float Attack()
    {
        float coolTime = 0.0f;
        if (m_pattern != null)
            coolTime = m_pattern.Attack(GameManager.Instance().ROBO.gameObject,gameObject,m_index);
        return coolTime;
    }

    // 데미지를 입는 처리
    public virtual void Damage(float damage)
    {
        if (m_pattern == null)
            return;
        // 그전의 방어력 경감 효과가 있는 상태인지 체크
        
        float per = m_pattern.PreProcessedDamge();

        m_hp -= (damage * per);
        
        // UI 가 바뀌는 처리는 일단 여기서 하지 않는다.
    }

    // 이동 
    protected virtual void Move()
    {
        if (m_pattern != null)
            m_pattern.Move(gameObject, GameManager.Instance().ROBO.gameObject);//GameManager.Instance().PLAYER.PLAYER_HERO.gameObject
    }

    // -------------------------------------------------------------------------------------------//

}
