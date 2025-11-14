using System;
using System.Threading.Tasks;

namespace Gameplay.Ability
{
    public interface IAsset<T> where T: IEquatable<T>
    {
        T GetAssetKey();
    }
    
    public interface IAssetManager<T> where T: IEquatable<T>
    {
        IAsset<T> GetAsset(T assetKey);
        Task<IAsset<T>> GetAssetAsync(T assetKey);
        void Release(T assetKey);
        void ReleaseAll();
    }
}