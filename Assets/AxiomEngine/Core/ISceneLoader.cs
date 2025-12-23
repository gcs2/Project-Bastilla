// ============================================================================
// RPGPlatform.Core - Scene Management Interface
// Abstraction for loading scenes (Unity SceneManager, Addressables, etc.)
// ============================================================================

using System.Threading.Tasks;

namespace RPGPlatform.Core
{
    public interface ISceneLoader
    {
        Task LoadSceneAsync(string sceneName);
    }
}
