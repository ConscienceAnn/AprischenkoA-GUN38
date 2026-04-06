using UnityEngine;
using UnityEngine.AI;

namespace Netologia.Quest.Characters
{
	public abstract class BaseMovement : MonoBehaviour
	{
		protected NavMeshAgent _agent;

		public bool IsMove
		{
			get
			{
				if (!_agent.hasPath) return false;
				
				var pos = EndPosition;
				var range = _agent.stoppingDistance;
				range *= range;

				//В Юнити нет нормальной системы остановки агентов:
				//если stoppingDistance > 0, то hasPath всегда будет true
				//так как фактически мы не дошли до конечной точки
				
				//есть второй вариант: вручную корректировать путь, устанавливая конечную точку
				//на нужном расстоянии, а stoppingDistance оставлять нулевым
				return Vector3.SqrMagnitude(pos - transform.position) > range;
			}
		}

		public Vector3 EndPosition => _agent.pathEndPosition;
		public int GetPath(Vector3[] array) => _agent.path.GetCornersNonAlloc(array);

		public void SetPosition(Vector3 position)
		{
			_agent.SetDestination(position);
		}
		
		protected virtual void Awake()
		{
			_agent = GetComponent<NavMeshAgent>();
		}
	}
}