using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class NetworkStage1BOSS : MonoBehaviour,NetworkManager.NetworkMessageEventListenrer
{
    private string m_bossName = null;

    // pattern D 를 위한 객체
    private float m_tick = 0;
    private GameObject m_rotate = null;
    private bool m_effect = false;
    private bool m_moveEffect = false;
    private string m_curPattern = null;
    private float m_angle = 0.0f;

    public string BOSS_NAME
    {
        get { return m_bossName; }
        set { m_bossName = value; }
    }

    private SkeletonAnimation m_skeletonAnimation = null;
    private Stage1BOSS m_boss = null;
    private int m_hp = 100;
    
    // Use this for initialization
    void Start()
    {
        m_boss = this.GetComponent<Stage1BOSS>();
        m_skeletonAnimation = this.GetComponent<SkeletonAnimation>();
        m_skeletonAnimation.state.Complete += State_Complete;
        NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
        this.transform.GetChild(3).GetComponent<TextMesh>().text = "BOSS hp : " + m_hp + "/100";
    }

    private void State_Complete(Spine.TrackEntry trackEntry)
    {
        if (trackEntry.animation.name.Equals("attack_C_charge"))
        {
            m_boss.m_laser.gameObject.SetActive(true);
            m_boss.m_laser.Play("boss_laser");

        }
        else if (trackEntry.animation.name == "transform" && !string.IsNullOrEmpty(m_curPattern) && m_curPattern.Equals("D"))
        {
            m_rotate = m_boss.m_patternDRotate;
            m_rotate.transform.position = this.transform.position;
            m_rotate.gameObject.SetActive(true);
            m_rotate.transform.localScale = new Vector3(3 , 3);

            // 다시 생겼을 때 
            if (m_moveEffect)
            {
                m_effect = false;
                iTween.ScaleTo(m_rotate , iTween.Hash("x" , 0.0f , "y" , 0.0f));
                m_skeletonAnimation.state.SetAnimation(2 , "move_fast_close" , true);
            }
            else
            {
                m_effect = true;
                m_skeletonAnimation.state.SetAnimation(2 , "attack_D_in" , false);
            }
            
        }
        if (trackEntry.animation.name == "attack_D_in")
        {   

        }
    }

    // Update is called once per frame
    void Update()
    {

        // 패턴 D일때만
        if (m_effect)
        {
            m_rotate.transform.Rotate(0.0f , 0.0f , m_angle);
            m_angle += 0.1f;
            m_rotate.transform.position = transform.position;
            m_tick += Time.deltaTime;

            if (m_tick >= GameSetting.BOSS1_PATTERN_D_SPECIAL && !m_moveEffect)
            {

                transform.position = GameManager.Instance().ROBO.transform.position;

                m_moveEffect = true;
                //m_tick = 0.0f;
                m_skeletonAnimation.state.SetAnimation(2 , "transform" , false);
                m_skeletonAnimation.state.Complete += State_Complete;

            }
            
            //NetworkManager.Instance().SendEnemyMoveMessage(JSONMessageTool.ToJsoinEnemyMove(m_me.GetComponent<Stage1BOSS>().m_BOSS_NAME , p.x , p.y , p.z , false));
        }
    }

    public void ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        switch (e.msgType)
        {
            case NetworkManager.DAMAGE:
                {
                    if (!e.targetName.Equals(m_bossName))
                        return;
                }
                break;
            case NetworkManager.HP_UPDATE:
                {
                    if (!e.targetName.Equals("boss1"))
                        return;
                    m_hp = (int)e.msg.GetField(NetworkManager.HP_UPDATE).i;
                    this.transform.GetChild(3).GetComponent<TextMesh>().text = "BOSS hp : " + m_hp + "/100";
                }
                break;
            case NetworkManager.AI_ANI_NAME:
                {
                    if (e.targetName.Equals(m_bossName))
                    {

                        switch (e.msg.GetField(NetworkManager.AI_PATTERN_NAME).str)
                        {
                            //A 패턴과 B패턴은 단순 이동 / 공격 애니메이션 처리만 함
                            case "A":
                            case "B":
                            case "D":
                                m_skeletonAnimation.state.SetAnimation(0 , 
                                    e.msg.GetField(NetworkManager.AI_ANI_NAME).str ,
                                    e.msg.GetField(NetworkManager.AI_ANI_LOOP).b);
                                break;
                            case "C":
                                m_skeletonAnimation.state.SetAnimation(2 , e.msg.GetField(NetworkManager.AI_ANI_NAME)[0].str , false);
                                for (int i = 1; i < e.msg.GetField(NetworkManager.AI_ANI_NAME).Count; i++)
                                {
                                    m_skeletonAnimation.state.AddAnimation(2 , e.msg.GetField(NetworkManager.AI_ANI_NAME)[i].str , false , 0.0f);
                                }                            
                                break;
                        }
                        m_curPattern = e.msg.GetField(NetworkManager.AI_PATTERN_NAME).str;
                    }
                }
                break;
            case NetworkManager.AI_PATTERN_EXIT:
                {
                    if (e.targetName.Equals(m_bossName))
                    {

                        switch (e.msg.GetField(NetworkManager.AI_PATTERN_NAME).str)
                        {
                            case "C":

                                m_boss.m_laser.SetInteger("laser" , 1);
                                m_boss.m_laser.Play("Wait");
                                m_boss.m_laser.gameObject.SetActive(false);
                                m_skeletonAnimation.state.ClearTrack(2);
                                break;
                        }
                    }
                    break;
                }
        }
    }
}
