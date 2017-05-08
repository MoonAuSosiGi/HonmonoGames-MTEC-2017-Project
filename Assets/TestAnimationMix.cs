using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

public class TestAnimationMix : MonoBehaviour {

    SkeletonAnimation m_ani;
    public string animationName = null;
	// Use this for initialization
	void Start () {
        m_ani = GetComponent<SkeletonAnimation>();
        m_ani.state.Complete += HandleEvent;
	}
    void HandleEvent(TrackEntry trackEntry)
    {
        if(trackEntry.animation.name == "attack_A")
        {
            m_ani.state.SetAnimation(0 , "move_fast_open",true);
        }
    }

    // Update is called once per frame
    void Update () {
		
        if(Input.GetKeyDown(KeyCode.Space))
        {
            m_ani.state.SetAnimation(0 , animationName , false);
        }
	}
}
