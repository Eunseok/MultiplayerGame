using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace Managers
{
    public class NetworkManager : MonoBehaviour
    {
        public NetworkRunner networkRunner;

        void Start()
        {
            _ = StartGame();
        }

        async Task StartGame()
        {
            if (networkRunner == null)
            {
                networkRunner = gameObject.GetComponent<NetworkRunner>() ?? gameObject.AddComponent<NetworkRunner>();
            }

            var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>() ??
                               gameObject.AddComponent<NetworkSceneManagerDefault>();

            // 현재 활성화된 씬을 참조 가능한 SceneRef로 변환
            var sceneRef = sceneManager.GetSceneRef(SceneManager.GetActiveScene().name);

            var startGameArgs = new StartGameArgs
            {
                GameMode = GameMode.AutoHostOrClient,
                SessionName = "MultiplayerRoom",
                Scene = sceneRef,
                SceneManager = sceneManager
            };

            var result = await networkRunner.StartGame(startGameArgs);

            if (!result.Ok)
            {
                Debug.LogError($"Failed to start the game: {result.ShutdownReason}");
            }
        }
    }
}