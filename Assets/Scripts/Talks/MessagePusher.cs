using System.Runtime.CompilerServices;
using Netologia.Quest.Characters.Player;
using Netologia.Quest.Interfaces;
using Netologia.Quest.Objects;
using UnityEngine;

namespace Netologia.Quest.Talks
{
    public class MessagePusher : MonoBehaviour
    {

        private Transform _player;
        private Camera _camera;
        //переключатель true  - пауза между фразами / false - отображение фразы
        private bool _silence;
        private bool _lock;
        private float _delay;

        [SerializeField]
        private MessageElement _message;

        [SerializeField, Space(15f)]
        private WorkerFeatureFlag _workerFeature;
        [SerializeField]
        private CharacterFeatureFlag _characterFeature;

        [SerializeField]
        private float _talkDelay = 7f;
        [SerializeField]
        private Interval _silenceDelay = new(5f, 10f);
        [SerializeField]
        private float _talkRadius = 2f;

        public void LockMessage(bool value)
        {
            if (value)
            {
                _message.Disable();
                _lock = true;
            }
            else
            {
                _delay = TimeManager.Time + _silenceDelay.Random;
                _lock = false;

            }

            _silence = true;
        }

        private void Update()
        {
            if (!TimeManager.IsGame) return;
            if (_lock) return;

            var time = TimeManager.Time;

            if (_delay < time) //or _delay - time <= 0
            {
                //push new message
                if (_silence)
                {
                    _silence = false;
                    _delay = time + _talkDelay;

                    if (Vector3.SqrMagnitude(transform.position - _player.position) < _talkRadius)
                    {
                        _message.Enable();
                        _message.WorkPush(Director.Work[(int)_workerFeature]);
                    }
                    //может бликовать сообщение так как персонаж может быть в другой точке, а move push часть не выполнится
                    return;
                }
                //delay silence
                _silence = true;
                _delay = time + _silenceDelay.Random;
                _message.Disable();
            }

            //Move push
            if (!_silence)
                _message.Transform.position = _camera.WorldToScreenPoint(transform.position);//offset?
        }







        public void CharacterPush()
        {
            _message.Enable();
            _delay = TimeManager.Time + _talkDelay;
            _silence = false;

            _message.CharacterPush(Director.Personal[(int)_characterFeature]);
        }

        private void Awake()
        {
            //todo костыльно, но допустимо
            _player = FindObjectOfType<PlayerController>().transform;

            _message = Instantiate(_message, FindObjectOfType<MessageRootTag>().transform);
            _message.Disable();

            _silence = true;
            //first special long
            _delay = _silenceDelay.Random * 2;
            //set sqr
            _talkRadius *= _talkRadius;

            _camera = Camera.main;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            //Gizmos.DrawWireSphere(transform.position, _talkRadius);
        }

    }
}