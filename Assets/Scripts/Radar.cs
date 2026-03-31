using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;

public class Radar : MonoBehaviour
{

    private readonly float _mapScale = 2;
    private Transform _playerPos;
    public static List<RadarObject> RadObjects = new();

    
    void Start()
    {
        _playerPos = Camera.main.transform;
    }

    [Inject]
    private void Inject([Inject(Id = "Camera")] Transform mainCamera)
    {
        _playerPos = mainCamera;
        Observable
            .EveryUpdate().Where((l => Time.frameCount % 2 == 0)).Subscribe((l => DrawRadarDots())).AddTo(this);
    }

    public static void RegisterRadarObject(GameObject o, Image i)
    {
        var image = Instantiate(i);
        RadObjects.Add(new RadarObjects { Owner = o, Icon  = image });
    }

    public static void RemoveRadarObject(GameObject o)
    {
        List<RadarObject> newList = new List<RadarObject>();
        foreach (RadarObject t in RadObjects)
        {
            if (t.Owner == o)
            {
                Destroy(t.Icon);
                continue;
            }
            newList.Add(t);

        }
        RadObjects.RemoveRange(0, RadObjects.Count);
        RadObjects.AddRange(newList);
    }

    private void DrawRadarDots()
    {
        foreach (RadarObject radObject in RadObjects)
        {
            Vector3 radarPos = (radObject.Owner.transform.position - _playerPos.position);
            float distToObject = Vector3.Distance(_playerPos.position, radObject.Owner.transform.position) * _mapScale;
            float deltay = Mathf.Atan2(radarPos.x, radarPos.z) * Mathf.Rad2Deg - 270 - _playerPos.eulerAngles.y;
            radarPos.x = distToObject * Mathf.Cos(deltay * Mathf.Deg2Rad) * -1;
            radarPos.z = distToObject * Mathf.Sin(deltay * Mathf.Deg2Rad);
            radObject.Icon.transform.SetParent(transform);
            radObject.Icon.transform.position = new Vector3(radarPos.x, radarPos.z, 0) + transform.position;

        }
    }


    public sealed class RadarObject
    {
        public Image Icon;
        public GameObject Owner;
    }


}
