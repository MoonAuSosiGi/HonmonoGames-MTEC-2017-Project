using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SpineAnimation : StateMachineBehaviour {

    public string animationName = null;
    public float speed = 1.0f;
    public bool loop = false;

    public override void OnStateEnter(Animator animator , AnimatorStateInfo stateInfo , int layerIndex)
    {
        SkeletonAnimation anim = animator.GetComponent<SkeletonAnimation>();
        anim.state.SetAnimation(0 , animationName , loop).TimeScale = speed;
        
    }
}
