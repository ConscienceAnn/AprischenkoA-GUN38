
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEngine;

public class ObjectsMover : MonoBehaviour
{
    [SerializeField]
    private MovingGroupManager movingGroupManager;

    private MoveAgent[] agents;

    private void Awake()
    {
        this.agents = FindObjectsOfType<MoveAgent>();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(1))
        {
            return; 
        }

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit))
        {
            return;
        }

        if (hit.transform.CompareTag("Ground"))
        {
            this.MoveToPosition(hit.point);
        }
    }

    private void MoveToPosition(Vector3 targetPosition)
    {
        targetPosition.y = 0;

        //var center = this.CalculateCenter(this.agents.Select(it -> it.transform.position).ToArray());
        this.movingGroupManager.AddGroup(this.agents, targetPosition);

        foreach (var agent in this.agents)
        {
            agent.MoveToPosition(targetPosition);

            //var offset = agent.transform.position - center;
            //agent.MoveToPosition(targetPosition + offset);
        }
    }

    private Vector3 CalculateCenter(Vector3[] points)
    {
        var result = Vector3.zero;
        foreach (var point in points)
        {
            result += point;
        }

        return result / points.Length;
    }

}
