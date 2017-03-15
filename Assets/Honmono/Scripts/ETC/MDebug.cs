using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MDebug  {
    public static bool DEBUG_MODE = true;

	public static void Log(object o)
    {
        if(DEBUG_MODE)
            Debug.Log(o);
    }
}
