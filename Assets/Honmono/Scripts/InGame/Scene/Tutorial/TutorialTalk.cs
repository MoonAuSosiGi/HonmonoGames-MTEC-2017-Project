using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialTalk : MonoBehaviour {

    public Text m_message = null;
    private AudioSource m_source = null;
    private float m_tick = 0.0f;
    private float m_time = 0.0f;
    private bool m_tutoTalkAlive = false;
    private bool m_tweenAnimation = false;
    private string m_interactionObjName = null;

    // 표시할 것들
    public TutorialController m_controller = null;
    public AudioClip m_buzzer = null;
    public AudioClip m_tuto_announce = null;
    public AudioClip m_tip = null;

    public bool TUTP_TALK_ALIVE
    {
        get { return m_tutoTalkAlive; }
    }
    
    
    void Start()
    {
        // test 
        ShowTween();
        m_source = this.GetComponent<AudioSource>();
    }

    // --------------------------------------------------------------------------//

    public void ShowTutorial(string sound , string message , float time = -1.0f , string interactionObjName = null)
    {
        // 사운드 재생
        Color col = m_message.color;
        m_message.color = new Color(col.r , col.g , col.b , 0.0f);
        m_message.text = message;
        
        m_time = time;
        m_interactionObjName = interactionObjName;
        AudioClip clip = m_tuto_announce;
        m_source.PlayOneShot(m_tuto_announce);
        m_tutoTalkAlive = true;
        m_tweenAnimation = true;
        
    }

    void Update()
    {
        if(m_tutoTalkAlive)
        {
            if (!m_source.isPlaying && m_tweenAnimation)
            {
                m_tweenAnimation = false;
                ShowTween();
            }
        }

    }

    void ShowTween()
    {
        iTween.ValueTo(gameObject , 
            iTween.Hash("from" , 0.0f , "to" , 1.0f ,
            "time",1.0f, 
            //"easetype","easeOutElastic",
            "onupdatetarget",gameObject,
            "onupdate","Effect",
            "oncompletetarget",gameObject,
            "oncomplete","ShowTweenEnd"));

        //iTween.ValueTo(gameObject , iTween.Hash("from" , 1.0f , "to" , 1.7f , "time" , 0.3f , "onupdatetarget" , gameObject , "onupdate" , "LightUpdate" , "oncompletetarget" , gameObject , "oncomplete" , "LightRangeDown"));
    }

    void Effect(float v)
    {
        Color color = m_message.color;
        color.a = v;
        m_message.color = color;
    }

    void ShowTweenEnd()
    {
        if (m_time > 1.0f)
            Invoke("HideTween" , m_time);
    }

    void HideTweenEnd()
    {
        m_tutoTalkAlive = false;

        if(m_time <= 0.0f && !string.IsNullOrEmpty(m_interactionObjName))
        {

        }
        else
            m_controller.TutorialAction_ShoTimeEnd();

    }

    void HideTween()
    {
        iTween.ValueTo(gameObject ,
            iTween.Hash("from" , 1.0f , "to" , 0.0f ,
            "time" , 1.0f ,
            //"easetype","easeOutElastic",
            "onupdatetarget" , gameObject ,
            "onupdate" , "Effect" ,
            "oncompletetarget" , gameObject ,
            "oncomplete" , "HideTweenEnd"));
    }
}
