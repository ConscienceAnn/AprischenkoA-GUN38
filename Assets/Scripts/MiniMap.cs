using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class MiniMap : MonoBehaviour
{
   
    void Start()
    {
        _player = Camera.main.transform;
        transform.parent = null;
        transform.rotation= Quaternion.Euler(90, 0 , 0);
        transform.position = _player.position + new Vector3(0, 5, 0);

        var rt = Resources.Load<RenderTexture>("MiniMap/NinimapTexture");

        GetComponent<Camera>().targetTexture = rt;
        Observable.EveryLateUpdate().Subscribe(OnLateUpdate);
    }

    void OnLateUpdate(long param)
    {
        var newPosition = _player.position;
        newPosition.y = transform.position.y;
        transform.position = newPosition;
        transform.rotation = Quaternion.Euler(90, _player.eulerAngles.y, 0);
    }
}
