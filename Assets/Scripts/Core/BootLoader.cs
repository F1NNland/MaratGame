using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MaratGame.Core
{
    /// <summary>
    /// Сцена Boot: короткая задержка и переход в главное меню.
    /// </summary>
    public sealed class BootLoader : MonoBehaviour
    {
        const float LoadDelaySeconds = 0.5f;

        IEnumerator Start()
        {
            yield return new WaitForSeconds(LoadDelaySeconds);
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
