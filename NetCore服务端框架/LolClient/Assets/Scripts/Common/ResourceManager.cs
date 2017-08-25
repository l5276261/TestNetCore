using UnityEngine;
using System.Collections;
using System.Text;

public enum ResFolder { Main, Forge,Sword ,Skill, Ornament,Item,SkillAnd, SelectSword, SelectSwordSlim,Head, Res, RankingList, SwordFull,Team , TeamMark }
public class ResourceManager : InstanceMono<ResourceManager> {
    public Hashtable ResHashScene = new Hashtable();
    public Hashtable ResHashScenes = new Hashtable();
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
    public void Initialize() {

    }
    //必须约束T继承自UnityEngine.Object，当然由于没有歧义性，没有引用System，只写Object也行
    public T Load<T>(ResFolder folder, string path, bool toHash = false,bool isOneScene=false) where T:Object {
        StringBuilder totalPath = new StringBuilder();
        Hashtable resHash = new Hashtable();
        if (isOneScene)
            resHash = ResHashScene;
        else resHash = ResHashScenes;
        switch (folder) {
            case ResFolder.Forge:
                totalPath.Append("Forge/");
                break;
            case ResFolder.Res:
                totalPath.Append("Bag/Res/");
                break;
            case ResFolder.Sword:
                totalPath.Append("Bag/Sword/");
                break;
            case ResFolder.Skill:
                totalPath.Append("Bag/Skill/");
                break;
            case ResFolder.SkillAnd:
                totalPath.Append("Bag/SkillAnd/");
                break;
            case ResFolder.Ornament:
                totalPath.Append("Bag/Ornament/");
                break;
            case ResFolder.Item:
                totalPath.Append("Bag/Item/");
                break;
            case ResFolder.SelectSword:
                totalPath.Append("SelectSword/");
                break;
            case ResFolder.SelectSwordSlim:
                totalPath.Append("SelectSwordSlim/");
                break;
            case ResFolder.Head:
                totalPath.Append("Head/");
                break;
            case ResFolder.RankingList:
                totalPath.Append("RankingList/");
                break;
            case ResFolder.SwordFull:
                totalPath.Append("Bag/SwordFull/");
                break;
            case ResFolder.Team:
                totalPath.Append("TeamSword/");
                break;
            case ResFolder.TeamMark:
                totalPath.Append("Bag/TeamMark/");
                break;
        }
        totalPath.Append(path);
        if (!resHash.Contains(path)) {
            T t = Resources.Load<T>(totalPath.ToString());
            if (toHash) {
                resHash[path] = t;
            }
            return t;
        } else return resHash[path] as T;
    }
    public void ReleaseResAll() {
        ResHashScene.Clear();
        ResHashScenes.Clear();
        Resources.UnloadUnusedAssets();
    }
    public void ReleaseResUnused() {
        ResHashScene.Clear();
        Resources.UnloadUnusedAssets();
    }
}
