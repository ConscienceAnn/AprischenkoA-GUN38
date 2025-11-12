using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class TurnDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnText; 
    [SerializeField] private BattleController battleController; 

    void Start()
    {
        battleController = FindObjectOfType<BattleController>();

        if (battleController != null)
        {
            battleController.OnTurnChanged += UpdateTurnDisplay;
        }

        UpdateTurnDisplay(Team.Player1);
    }

    void UpdateTurnDisplay(Team currentTeam)
    {
        if (turnText != null)
        {
            string playerName = currentTeam == Team.Player1 ? "Игрок 1" : "Игрок 2";
          
            turnText.text = $"Ход: {playerName} ";

            turnText.color = currentTeam == Team.Player1 ? Color.white : Color.blue;
        }
    }

    void OnDestroy()
    {
        if (battleController != null)
        {
            battleController.OnTurnChanged -= UpdateTurnDisplay;
        }
    }
}