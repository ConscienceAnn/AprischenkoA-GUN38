using Game.GameEngine.Ecs;
using UnityEngine;
using System.Text;

public class TeamDebugger : MonoBehaviour
{
    [SerializeField] private bool logEveryFrame = false;
    [SerializeField] private KeyCode debugKey = KeyCode.F1;

    private void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            LogAllTeams();
        }
    }

    [ContextMenu("Log All Teams")]
    public void LogAllTeams()
    {
        var allEntities = FindObjectsOfType<Entity>();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("\n========== TEAM COMPONENT DEBUG ==========");

        foreach (var entity in allEntities)
        {
            if (entity == null || !entity.IsExists()) continue;

            string teamInfo = "NO TEAM COMPONENT!";
            if (entity.HasData<TeamComponent>())
            {
                ref var team = ref entity.GetData<TeamComponent>();
                teamInfo = $"Team: {team.playerId} ({(team.playerId == 1 ? "PLAYER" : team.playerId == 2 ? "ENEMY" : "UNKNOWN")})";
            }

            sb.AppendLine($"- {entity.name} (ID:{entity.Id}) - {teamInfo}");
        }

        sb.AppendLine("========================================\n");
        Debug.Log(sb.ToString());
    }
}