using UnityEngine;
using System.Collections;

public class InstanceNormal<T> where T: class,new() {

    private static T i;
    public static T I {
        get {
            if (i == null) i = new T();
            return i;
        }
    }
    public virtual void Initial() {

    }
}
