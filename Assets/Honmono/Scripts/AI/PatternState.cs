using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PatternState  {

    // Attack 의 경우 여기서 따로 처리해도 무방
    // 따로 쿨타임을 처리해야할 필요성이 있을 때를 대비해서 float 를 리턴한다.
    public abstract float Attack(GameObject hero);

    // 다만 데미지의 경우 실 계산은 해당 오브젝트에서 해야된다.
    // 데미지를 얼만큼 받을지에 대한 처리로 float 형으로 리턴하는 값을 퍼센트 적용해서 처리한다.
    public abstract float PreProcessedDamge();

    // 실제 이동 로직을 처리한다.
    // 이녀석은 Update 에서 호출되어야 한다.
    public abstract void Move(GameObject target,GameObject hero);
}
