using UnityEngine;

namespace Progression
{
    public class ProgressionDebugUI : MonoBehaviour
    {
        [SerializeField] private PlayerProgression _player;
        [SerializeField] private int _debugXp = 100;

        private void OnGUI()
        {
            if (_player == null) return;

            var labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 24 };
            var buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 24 };

            GUILayout.BeginArea(new Rect(10, 10, 1000, 500));
            GUILayout.Label($"Level: {_player.Level} ({_player.Rank})", labelStyle);
            GUILayout.Label($"XP: {_player.CurrentXp} / Next: {_player.XpToNextLevel}", labelStyle);
            GUILayout.Label($"Progress: {_player.LevelProgress:P0}", labelStyle);
            GUILayout.Label($"Attack: {_player.Combatant.Stats.AttackDamage.Value:F0}", labelStyle);

            if (GUILayout.Button($"Add {_debugXp} XP", buttonStyle, GUILayout.Height(50)))
                _player.AddExperience(_debugXp);

            GUILayout.EndArea();
        }
    }
}
