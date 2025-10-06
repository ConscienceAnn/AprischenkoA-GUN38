using System;
using UnityEngine;

namespace DefaultNamespace
{
	[RequireComponent(typeof(PositionSaver))]
	public class ReplayMover : MonoBehaviour
	{
		[ReadOnly] private PositionSaver _save;

        [ReadOnly] private int _index;

        [ReadOnly] private PositionSaver.Data _prev;

        [ReadOnly] private float _duration;

		private void Start()
		{
            ////todo comment: зачем нужны эти проверки?
            //убедиться, что компонент готов к работе и есть данные для воспроизведения.Если компонента нет - воспроизводить нечего, если записей нет - воспроизводить нечего
            if (!TryGetComponent(out _save) || _save.Records.Count == 0)
			{
				Debug.LogError("Records incorrect value", this);
                //todo comment: Для чего выключается этот компонент?
                //т.е. после условий, если чего то нет, то выключаем, чтобы не было ошибок во время выполнения
                enabled = false;
				return;
			}
		}

		private void Update()
		{
			var curr = _save.Records[_index];
            //todo comment: Что проверяет это условие (с какой целью)?
            //условие проверяет, настало ли время переходить к следующей точке маршрута при воспроизведении записи
            if (Time.time > curr.Time)
			{
				_prev = curr;
				_index++;
                //todo comment: Для чего нужна эта проверка?
                //проверка нужна, чтобы определить, когда воспроизведение записи закончилось, когда мы дошли до последней точки
                if (_index >= _save.Records.Count)
				{
					enabled = false;
					Debug.Log($"<b>{name}</b> finished", this);
				}
			}
            //todo comment: Для чего производятся эти вычисления (как в дальнейшем они применяются)?
            //вычисления определяют прогресс движения между двумя точками записи, так достигается плавное движение между точками записи с правильным таймингом
            var delta = (Time.time - _prev.Time) / (curr.Time - _prev.Time);
			//todo comment: Зачем нужна эта проверка?
			//избежать ошибки, деление на ноль. 
			if (float.IsNaN(delta)) delta = 0f;
            //todo comment: Опишите, что происходит в этой строчке так подробно, насколько это возможно
            //Эта строка плавно перемещает объект из предыдущей позиции в текущую позицию на определенный процент пути.
            //Vector3.Lerp указывает координаты объекта между предыдущей (Prev) и текущей точкой (curr), на расстоянии delta от начала
			//засчет изменения delta объект как бы "едет", плавно перемещается.
            transform.position = Vector3.Lerp(_prev.Position, curr.Position, delta);
		}
	}
}