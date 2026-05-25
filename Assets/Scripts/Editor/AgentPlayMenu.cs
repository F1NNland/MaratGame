#if UNITY_EDITOR
using MaratGame.Agent;
using UnityEditor;
using UnityEngine;

namespace MaratGame.EditorTools
{
    static class AgentPlayMenu
    {
        [MenuItem("Tools/Marat Game/Agent - Get Play State")]
        static void LogPlayState()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("[AgentPlay] Enter Play Mode first.");
                return;
            }

            Debug.Log(AgentPlayBridge.GetPlayState());
        }

        [MenuItem("Tools/Marat Game/Agent - Get Play State", true)]
        static bool LogPlayStateValidate() => EditorApplication.isPlaying;
    }
}
#endif
