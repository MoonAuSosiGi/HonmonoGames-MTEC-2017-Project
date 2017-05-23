using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialTalk : MonoBehaviour {

    public Text m_message = null;
    private AudioSource m_source = null;

    void Start()
    {
        // test 
        ShowTween();
    }

    // --------------------------------------------------------------------------//

    void Update()
    {
        if(!m_source.isPlaying)
        {

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

    }

    void HideTweenEnd()
    {

    }

    void HideTween()
    {
        iTween.ValueTo(gameObject ,
            iTween.Hash("from" , 0.0f , "to" , 1.0f ,
            "time" , 1.0f ,
            //"easetype","easeOutElastic",
            "onupdatetarget" , gameObject ,
            "onupdate" , "Effect" ,
            "oncompletetarget" , gameObject ,
            "oncomplete" , "HidTweenEnd"));
    }
}
