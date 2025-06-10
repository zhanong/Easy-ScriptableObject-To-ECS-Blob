using UnityEngine;
using Unity.Entities;
using System.Linq;
using Unity.Collections;
using System;

namespace ZhTool.Entities
{
    public class ABlobConfig<TAsset, TComponent, TBlob, TEnum> : MonoBehaviour
    where TAsset : ConfigAsset<TEnum>
    where TComponent : unmanaged, IConfigComponent<TBlob>
    where TBlob : unmanaged
    where TEnum : unmanaged, Enum
    {
        protected TAsset[] assets;

        public void Bake<T>(Baker<T> baker) where T : ABlobConfig<TAsset, TComponent, TBlob, TEnum>
        {
            LoadAsset(ref assets);
            assets = assets.OrderBy(asset => asset.Type).ToArray();

            var blobRef = CreatePool(baker);
            TComponent config = new();
            config.SetBlobRef(blobRef);

            // Register the blob asset to the baker for de-duplication and reverting 
            baker.AddBlobAsset(ref blobRef, out var hash);

            var entity = baker.GetEntity(TransformUsageFlags.None);
            baker.AddComponent(entity, config);
        }

        /// <summary>
        /// The assets will be sorted automatically after this method
        /// </summary>
        protected virtual void LoadAsset(ref TAsset[] assets)
        {

        }

        private BlobAssetReference<TBlob> CreatePool<T>(Baker<T> baker) where T : ABlobConfig<TAsset, TComponent, TBlob, TEnum>
        {
            // Get blob handle.
            var builder = new BlobBuilder(Allocator.Temp);
            ref var blob = ref builder.ConstructRoot<TBlob>();

            TransferData(ref blob, ref builder, baker);

            // Build blob.
            var result = builder.CreateBlobAssetReference<TBlob>(Allocator.Persistent);
            builder.Dispose();
            return result;
        }

        protected virtual void TransferData<T>(ref TBlob blob, ref BlobBuilder builder, Baker<T> baker) where T : ABlobConfig<TAsset, TComponent, TBlob, TEnum>
        {

        }

        #region helper
        protected void LoadFromResources(string path)
        {
            assets = Resources.LoadAll<TAsset>(path).OrderBy(asset => asset.Type).ToArray();
        }

        protected void Transfer<T>(ref BlobArray<T> blobArray, ref BlobBuilder builder, Func<TAsset, T> func) where T : unmanaged
        {
            var array = builder.Allocate(ref blobArray, assets.Length);

            for (int i = 0; i < assets.Length; i++)
            {
                array[i] = func(assets[i]);
            }
        }

        protected void TransferString(ref BlobArray<BlobString> blobArray, ref BlobBuilder builder, Func<TAsset, string> func)
        {
            var array = builder.Allocate(ref blobArray, assets.Length);

            for (int i = 0; i < assets.Length; i++)
            {
                builder.AllocateString(ref array[i], func(assets[i]));
            }
        }
        #endregion
    }

    public interface IConfigComponent<TBlob> : IComponentData where TBlob : unmanaged 
    {
        public void SetBlobRef(BlobAssetReference<TBlob> blobRef);
    }

    public abstract class ConfigAsset<TEnum> : ScriptableObject where TEnum : System.Enum
    {
        public abstract TEnum Type { get; }
    }
}
