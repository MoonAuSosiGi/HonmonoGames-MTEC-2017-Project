using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class Boss2PatternD : PatternState {
    float lastSend = 0.0f;
    public Boss2PatternD(SkeletonAnimation ani , string moveAni , string attackAni , string aiTarget) : base(ani , moveAni , attackAni , aiTarget)
    {
        // 알까~기~

        lastSend = 0.0f;
    }

    public override void Update(GameObject me)
    {
        lastSend += Time.deltaTime;
    }

    public override float Attack(GameObject hero , GameObject me , int index)
    {
        if (lastSend < 2.0f)
            return 2.0f;
        lastSend = 0.0f;
        Stage2Boss boss = me.GetComponent<Stage2Boss>();
        boss.ShootBullet(new int[] {0,1,2,3,4,5},true);
        boss.ShootEgg(new int[] { 0 , 1 } , true);
        return 2.0f;
    }

    public override void Move(GameObject target , GameObject hero)
    {

    }

    public override float PreProcessedDamge()
    {
        return 1.0f;
    }
}
