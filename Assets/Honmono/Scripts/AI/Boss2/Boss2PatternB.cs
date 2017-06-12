using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class Boss2PatternB : PatternState {

    public Boss2PatternB(SkeletonAnimation ani , string moveAni , string attackAni , string aiTarget) : base(ani , moveAni , attackAni , aiTarget)
    {
        //홀수 번호의 몸통에서 2초마다 탄환 발사 
        // 플레이어 좌표  -0.3 ~ 0.3 범위에서 랜덤 추적
        // 파괴되면 나오지 않는다
    }

    public override float Attack(GameObject hero , GameObject me , int index)
    {
        Stage2Boss boss = me.GetComponent<Stage2Boss>();

        int[] seed = new int[] { 0 , 2 , 4 , 6 , 8 };

        boss.ShootBullet(new int[] { seed[Random.Range(0 , seed.Length)] });

        return 4.0f;
    }

    public override void Move(GameObject target , GameObject hero)
    {
        
    }

    public override float PreProcessedDamge()
    {
        return 1.0f;
    }
}
