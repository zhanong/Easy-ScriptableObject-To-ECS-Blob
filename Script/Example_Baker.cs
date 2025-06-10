/*
    This workflow is designed for creating a blob that holds configuration data for a list of scriptable objects.
    For example, you have a list of scenes (Level1, Level2, etc.) and you want to store their configurations in a blob.

    To create a new "scriptable -> blob" config, follow these steps:

    0. Create an enum to represent the scriptable object type, for example, `public enum SceneID { Level1, Level2, ... }`.

    1. Create a structure to store blob data, place blobArrays as needed. See `SceneConfigBlob` for an example.

    2. Create a struct that implements `IConfigComponent<T>` where T is the blob data structure. 
       This is the `IComponentData` you use to access the blob. See `SceneConfig` for an example.

    3. Create a scriptable object that inherits from `ConfigAsset<TEnum>` where `TEnum` is from step 0.
        - The scriptable object should be placed alone for correct serialization.
        - Add fields for configuration as needed. You can also create a struct to hold the configuration data. See `SceneConfigData` for an example.
       See `SceneConfigAsset` for an example.

    4. Create a class that inherits from `ABlobConfig<TAsset, TConfig, TBlob, TEnum>` where:
        - `TAsset` is from step 3.
        - `TComponent` is from step 2.
        - `TBlob`  is from step 1.
        - `TEnum` is from step 0.
       See ASceneConfig for an example.
*/

using Unity.Entities;
using UnityEngine;
using ZhTool.Entities;

namespace ECSFramework
{
    public class ASceneConfig : ABlobConfig<SceneConfigAsset, SceneConfig, SceneConfigBlob, SceneID>
    {
        [SerializeField] string loadPath;

        public class Baker : Baker<ASceneConfig>
        {
            public override void Bake(ASceneConfig authoring)
            {
                authoring.Bake(this);
            }
        }

        protected override void LoadAsset(ref SceneConfigAsset[] assets)
        {
            assets = Resources.LoadAll<SceneConfigAsset>(loadPath);
        }

        protected override void TransferData<ASceneConfig>(ref SceneConfigBlob blob, ref BlobBuilder builder, Baker<ASceneConfig> baker)
        {
            Transfer(ref blob.generalData, ref builder, (asset) => asset.data);
        }
    }

    public struct SceneConfig : IConfigComponent<SceneConfigBlob>
    {
        public BlobAssetReference<SceneConfigBlob> blobRef;

        public void SetBlobRef(BlobAssetReference<SceneConfigBlob> blobRef)
        {
            this.blobRef = blobRef;
        }

        public SceneConfigData GetConfig(SceneID sceneID)
        {
            ref var blob = ref blobRef.Value;
            return blob.generalData[(int)sceneID];
        }
    }

    public struct SceneConfigBlob
    {
        // This is an example of "Array of Structure" (AoS). "Structure of Array" (SoA) is preferred for performance. 
        public BlobArray<SceneConfigData> generalData;
    }

    [System.Serializable]
    public struct SceneConfigData
    {
        public int exampleConfigValue; // Example value, replace with actual data
    }
}

