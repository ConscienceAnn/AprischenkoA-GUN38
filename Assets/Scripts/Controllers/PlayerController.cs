using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private BattleController battleController;
    private bool _inputBlocked = false;

    void Start()
    {
        if (battleController == null)
            battleController = FindObjectOfType<BattleController>();
    }

    
    public void MakeMove(Unit unit, Cell targetCell, bool isJump)
    {
        if (_inputBlocked)
        {
            Debug.LogWarning("Попытка сделать ход во время блокировки!");
            return;
        }

        StartCoroutine(ExecuteMoveWithAnimation(unit, targetCell, isJump));
    }

    private IEnumerator ExecuteMoveWithAnimation(Unit unit, Cell targetCell, bool isJump)
    {
        _inputBlocked = true;
        Debug.Log("Блокируем ввод - началась анимация");

        yield return StartCoroutine(AnimateUnitMove(unit, targetCell.transform.position, isJump));

        battleController.OnMoveAnimationComplete(unit, isJump);

        _inputBlocked = false;
        Debug.Log("Разблокируем ввод - анимация завершена");
    }

    private IEnumerator AnimateUnitMove(Unit unit, Vector3 targetPosition, bool isJump)
    {
        Vector3 startPosition = unit.transform.position;

        float fixedHeight = 1f;
        Vector3 finalTargetPosition = new Vector3(targetPosition.x, fixedHeight, targetPosition.z);

        float duration = isJump ? 0.5f : 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            if (isJump)
            {
                // Для прыжка - с аркой (подпрыгивание)
                Vector3 newPosition = Vector3.Lerp(startPosition, finalTargetPosition, t);
                float jumpHeight = 1.5f;
                newPosition.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;
                unit.transform.position = newPosition;
            }
            else
            {
                // Для обычного хода - просто плавное перемещение по XZ (Y остается = 1)
                Vector3 newPosition = Vector3.Lerp(startPosition, finalTargetPosition, t);
                unit.transform.position = newPosition;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }


        unit.transform.position = finalTargetPosition;
    }

    public bool IsInputBlocked()
    {
        return _inputBlocked;
    }
}