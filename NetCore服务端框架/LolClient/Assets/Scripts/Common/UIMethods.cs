using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIMethods : InstanceMono<UIMethods> {

    public void PanelAndTextAppear(GameObject go,Text Text=null,string content=null) {
        go.SetActive(true);
        if (Text != null && content != null && content != "") {
            Text.text = content;
        }
        StartCoroutine("PanelAndTextDisappear", go);
    }
    IEnumerator PanelAndTextDisappear(GameObject go) {
        yield return new WaitForSeconds(1.5f);
        if (go != null)//防止场景在这个等待时间内切换导致go变为空
            go.SetActive(false);
    }
    public void SpriteColorShow(Image img) {
        if (img.sprite != null)
            img.color = Color.white;
        else img.color = Color.clear;
    }
}
