using System.Collections;
using UnityEngine;

namespace Netologia.Homework
{
    public class Player : MonoBehaviour
    {
        private bool _ready;
        private Rigidbody _ball;

        [SerializeField]
        private Rigidbody _ballPrefab;
        [SerializeField]
        private float _startVelocity;
        [SerializeField]
        private float _lifetime;

        [SerializeField]
        private float _respawnDelay;

        private void Update()
        {
            if (!_ready) return;
            if (_ready && _ball != null) //конструкция для компенсации scale у Player, чтобы Ball оставлся сферой, а не принимал Scale родителя. 
            {
                _ball.transform.localPosition = Vector3.zero;
                _ball.transform.localScale = new Vector3(
                    1f / transform.localScale.x,
                    1f / transform.localScale.y,
                    1f / transform.localScale.z
                );
            }

            if (Input.GetKey(KeyCode.Space))
            {
                StartCoroutine(Reloader());
                _ball.isKinematic = false;
                _ball.transform.parent = null;
                _ball.velocity = transform.forward * _startVelocity;
                Destroy(_ball.gameObject, _lifetime);
            }
        }

        private IEnumerator Reloader()
        {
            _ready = false;
            yield return new WaitForSeconds(_respawnDelay);
            Spawn();
        }

        private void Spawn()
        {
            _ball = Instantiate(_ballPrefab, transform);
            _ball.isKinematic = true;
            _ready = true;
        }

        private void Start()
        {
            Spawn();
        }
    }
}