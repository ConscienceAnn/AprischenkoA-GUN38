using UnityEngine;

namespace DefaultNamespace
{
	
	[RequireComponent(typeof(PositionSaver))]
	public class EditorMover : MonoBehaviour
	{
		[ReadOnly]
		private PositionSaver _save;

		[ReadOnly]
		private float _currentDelay;

		//todo comment: Что произойдёт, если _delay > _duration?
		//мммм, слишкам долгая задержка, если на простом примере: сделать кадр каждые 10 мин, а времени всего 5мин.. будет только один кадр, остальные не запишутся.
		[SerializeField, Range(0.2f, 1.0f)]
		private float _delay = 0.5f;

		[SerializeField, Min(0.2f)]
		private float _duration = 5f;

		private void Start()
		{
			if (_duration <= _delay)
			{
				_duration = _delay * 5f;
			}
			//todo comment: Почему этот поиск производится здесь, а не в начале метода Update?
			// в Старте он найдется один раз, в Апдейте будет каждый кадр находиться, будет тормозить игру
			_save = GetComponent<PositionSaver>();
			_save.Records.Clear();
		}

		private void Update()
		{
			_duration -= Time.deltaTime;
			if (_duration <= 0f)
			{
				enabled = false;
				Debug.Log($"<b>{name}</b> finished", this);
				return;
			}

            //todo comment: Почему не написать (_delay -= Time.deltaTime;) по аналогии с полем _duration?
            // _delay это фиксированный интервал, который не должен меняться, и при такой записи интервал бы постоянно увеличивался
            _currentDelay -= Time.deltaTime;
			if (_currentDelay <= 0f)
			{
				_currentDelay = _delay;
				_save.Records.Add(new PositionSaver.Data
				{
					Position = transform.position,
                    //todo comment: Для чего сохраняется значение игрового времени?
                    //воспроизведение идет в реальном времени, но с сохранением относительных временных интервалов между точками оригинальной записи
                    Time = Time.time,
				});
			}
		}
	}
}