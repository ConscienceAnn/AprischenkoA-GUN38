using UnityEngine;
using System.Collections.Generic;

public class ObjectsMover : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private int unitsCount = 5;
    [SerializeField] private MovingGroupManager movingGroupManager;

    private List<MoveAgent> allAgents = new List<MoveAgent>();
    private Queue<MoveAgent> pool = new Queue<MoveAgent>();
    private bool areUnitsActive = true;

    private void Start()
    {
        // Создаём пул (все объекты созданы, но выключены)
        for (int i = 0; i < unitsCount; i++)
        {
            GameObject unit = Instantiate(unitPrefab);
            unit.SetActive(false);  // начинаем с выключенных
            MoveAgent agent = unit.GetComponent<MoveAgent>();
            pool.Enqueue(agent);
            allAgents.Add(agent);
        }

        // Включаем юнитов при старте (можно опционально)
        SetUnitsActive(true);
    }

    private void Update()
    {
        // ПРАВАЯ КНОПКА (1) - включить/выключить юнитов
        if (Input.GetMouseButtonDown(1))
        {
            areUnitsActive = !areUnitsActive;
            SetUnitsActive(areUnitsActive);
            Debug.Log(areUnitsActive ? "Юниты ВКЛЮЧЕНЫ" : "Юниты ВЫКЛЮЧЕНЫ (возвращены в пул)");
        }

        // ЛЕВАЯ КНОПКА (0) - движение (только если юниты активны)
        if (Input.GetMouseButtonDown(0) && areUnitsActive)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit)) return;

            if (hit.transform.CompareTag("Ground"))
            {
                MoveToPosition(hit.point);
            }
        }
    }

    private void SetUnitsActive(bool active)
    {
        if (active)
        {
            // Достаём из пула и включаем
            for (int i = 0; i < unitsCount; i++)
            {
                MoveAgent agent = pool.Dequeue();
                agent.gameObject.SetActive(true);

                // Ставим в стартовую позицию (строй)
                float spacing = 1.5f;
                int cols = Mathf.CeilToInt(Mathf.Sqrt(unitsCount));
                int row = i / cols;
                int col = i % cols;
                float x = (col - cols / 2f) * spacing;
                float z = (row - cols / 2f) * spacing;
                agent.transform.position = new Vector3(x, 0, z);
            }
        }
        else
        {
            // Выключаем и возвращаем в пул
            foreach (var agent in allAgents)
            {
                if (agent.gameObject.activeSelf)
                {
                    agent.StopMoving();
                    agent.gameObject.SetActive(false);
                    pool.Enqueue(agent);
                }
            }
        }
    }

    private void MoveToPosition(Vector3 targetPosition)
    {
        targetPosition.y = 0;

        // Все АКТИВНЫЕ юниты идут к цели
        List<MoveAgent> activeAgents = new List<MoveAgent>();
        foreach (var agent in allAgents)
        {
            if (agent.gameObject.activeSelf)
            {
                agent.MoveToPosition(targetPosition);
                activeAgents.Add(agent);
            }
        }

        if (activeAgents.Count > 0)
        {
            movingGroupManager.AddGroup(activeAgents, targetPosition);
        }
    }
}