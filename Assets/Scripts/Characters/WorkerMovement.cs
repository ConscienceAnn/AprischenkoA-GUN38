
using System;
using UnityEngine;

namespace Netologia.Quest.Characters
{
    // Поведение рабочего в фоновом режиме
    public class WorkerMovement : BaseMovement
    {
        [Serializable]
        public struct WorkPoint
        {
            public Vector3 Position;
            public Interval StayInterval;
        }

        private float? _delay;
        private int _index;

        private Character _character;

        [SerializeField]
        private WorkPoint[] _points;

        protected override void Awake()
        {
            base.Awake();
            _character = GetComponent<Character>();

            if (_points.Length <= 1)
            {
                _agent.enabled = false;
                enabled = false;
            }
        }

        private void Update()
        {
            if (!TimeManager.IsGame) return;
            //Если бот взаимодействует с игроком - он никуда не идет
            if (!_character.MoveAccess) return;


            //Movement state
            if (IsMove) return;

            var time = TimeManager.Time;

            //Arrived state
            if (!_delay.HasValue)
            {
                Debug.Log("Check: Arrived");
                _delay = time + _points[_index].StayInterval.Random;
            }

            //Change point stay
            if (_delay - time <= 0) //or _delay < time
            {
                _index = (_index + 1) % _points.Length;
                SetPosition(_points[_index].Position);

                _delay = null;
                Debug.Log("Check: Change point stay");
            }
        }




    }
}