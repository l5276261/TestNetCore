using UnityEngine;
using System.Collections;

public class InstanceMono<T> : MonoBehaviour where T:MonoBehaviour {
    private static T i;
    public static T I {
        get { if (i == null) {
                GameObject a = new GameObject(typeof(T).ToString());
                i = a.AddComponent<T>();
                DontDestroyOnLoad(a);
            }
            return i;
        }
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public virtual void Initial() {

    }
}
