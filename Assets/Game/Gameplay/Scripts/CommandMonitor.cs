using Game.GameEngine.Ecs;
using UnityEngine;
using System.Collections.Generic;

public class CommandMonitor : MonoBehaviour
{
    [SerializeField] private KeyCode monitorKey = KeyCode.F2;
    [SerializeField] private bool autoLogEveryFrame = false;
    [SerializeField] private float logInterval = 2f;

    private float lastLogTime;

    private void Update()
    {
        if (Input.GetKeyDown(monitorKey))
        {
            LogAllCommands();
        }

        if (autoLogEveryFrame && Time.time - lastLogTime >= logInterval)
        {
            lastLogTime = Time.time;
            LogAllCommands();
        }
    }

    [ContextMenu("Log All Commands")]
    public void LogAllCommands()
    {
        var allEntities = FindObjectsOfType<Entity>();

        Debug.Log("\n========== COMMAND MONITOR ==========");

        foreach (var entity in allEntities)
        {
            if (entity == null || !entity.IsExists()) continue;

            string teamInfo = "No Team";
            if (entity.HasData<TeamComponent>())
            {
                ref var team = ref entity.GetData<TeamComponent>();
                teamInfo = $"Team:{team.playerId}";
            }

            string commandInfo = "No Command";
            if (entity.HasData<CommandRequest>())
            {
                ref var cmd = ref entity.GetData<CommandRequest>();
                commandInfo = $"Command:{cmd.type} Status:{cmd.status}";

                // Дополнительная информация о цели атаки
                if (cmd.type == CommandType.ATTACK_TARGET && cmd.args is Entity target)
                {
                    string targetTeam = "No Team";
                    if (target.HasData<TeamComponent>())
                    {
                        ref var tt = ref target.GetData<TeamComponent>();
                        targetTeam = $"Team:{tt.playerId}";
                    }
                    commandInfo += $" Target:{target.Id} ({targetTeam})";
                }
            }

            Debug.Log($"{entity.name} (ID:{entity.Id}) - {teamInfo} - {commandInfo}");
        }

        Debug.Log("=====================================\n");
    }
}