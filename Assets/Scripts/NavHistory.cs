using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 导航历史栈：前进时压入(toShow, toHide)，返回时弹栈反转
/// </summary>
public static class NavHistory
{
    private static readonly Stack<Entry> stack = new();

    public static void Push(GameObject[] willShow, GameObject[] willHide)
    {
        if (willShow == null && willHide == null) return;
        stack.Push(new Entry { willShow = willShow, willHide = willHide });
    }

    public static bool Pop(out GameObject[] toShow, out GameObject[] toHide)
    {
        if (stack.Count > 0)
        {
            var e = stack.Pop();
            toShow = e.willHide;
            toHide = e.willShow;
            return true;
        }
        toShow = null;
        toHide = null;
        return false;
    }

    private class Entry
    {
        public GameObject[] willShow;
        public GameObject[] willHide;
    }
    //666666
}
