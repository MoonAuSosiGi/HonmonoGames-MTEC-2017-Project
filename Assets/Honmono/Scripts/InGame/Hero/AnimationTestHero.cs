using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class AnimationTestHero : MonoBehaviour {

    private const string IDLE = "idle";
    private const string MOVE = "move";
    private const string REPAIR = "repair";

    private SkeletonAnimation m_skeletonAnimation = null;

    public string m_cur = MOVE;
    public string m_space = REPAIR;

	// Use this for initialization
	void Start () {
        m_skeletonAnimation = this.GetComponent<SkeletonAnimation>();

        m_skeletonAnimation.state.SetAnimation(0 , m_cur , true);

        m_skeletonAnimation.state.Complete += State_End;
    }

    private void State_End(Spine.TrackEntry trackEntry)
    {
        MDebug.Log(trackEntry.animation.name);
        if(trackEntry.animation.name.Equals(REPAIR))
        {
            m_skeletonAnimation.state.ClearTrack(1);
            m_skeletonAnimation.state.ClearTrack(0);
            m_skeletonAnimation.state.SetAnimation(0 , m_cur , true);
        }
    }

    // TEST CODE 
    void Update () {
		
        if(Input.GetKeyUp(KeyCode.Space))
        {
            m_skeletonAnimation.state.SetAnimation(1 , m_space , false);
        }

	}
}
