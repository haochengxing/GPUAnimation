using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter
{
    private int mRef = 0;
    public int ReferenceCount { get { return mRef; } }

    public Counter() { }

    public void Reference() { mRef++; }

    public bool Release()
    {
        if (--mRef <= 0)
        {
            Clear();
            return true;
        }
        return false;
    }

    public bool IsAlive { get { return mRef > 0; } }

    protected virtual void Clear() { }
}
