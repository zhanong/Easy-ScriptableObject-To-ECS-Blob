/*

This class stores the configuration of the scene.

*/


using ECSFramework;
using UnityEngine;
using ZhTool.Entities;

[CreateAssetMenu(fileName = "SceneConfigAsset", menuName = "Custom/SceneConfigAsset")]
public class SceneConfigAsset : ConfigAsset<SceneID>
{
    public SceneID sceneID;
    public SceneConfigData data;

    public override SceneID Type => sceneID;
}
