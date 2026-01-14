using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public float forceMultiplier = 300f; // Сила броска
    private Rigidbody rb;
    private Vector3 startPoint; // Точка начала drag
    private Vector3 endPoint;   // Точка конца drag
    private bool isDragging = false;
    private Camera mainCamera;
    private bool isLaunched = false; // Уже запущен?

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Отключаем физику, пока тянем
        mainCamera = Camera.main;

        // Находим точку спауна по тегу и ставим туда мяч
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("Respawn");
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
        }
    }

    void OnMouseDown()
    {
        if (isLaunched) return; // Если уже бросили, новый бросок нельзя
        isDragging = true;
        startPoint = GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;
        endPoint = GetMouseWorldPos();
        // Можно здесь визуализировать вектор силы (линию)
    }

    void OnMouseUp()
    {
        if (!isDragging || isLaunched) return;
        isDragging = false;
        isLaunched = true;

        Vector3 force = startPoint - endPoint; // Вектор от конца к началу (тянем назад, бросаем вперед)
        force.y = 0; // Обнуляем вертикальную составляющую, чтобы не подкидывать
        force.z = Mathf.Abs(force.z); // Гарантируем, что сила по Z всегда положительная (вперед)

        rb.isKinematic = false; // Включаем физику!
        rb.AddForce(force * forceMultiplier);

        // Проиграть звук броска (добавим позже)
    }

    Vector3 GetMouseWorldPos()
    {
        // Получаем позицию мыши в мире на плоскости Y=0
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    // Этот метод будет вызываться системой, чтобы сбросить мяч для нового броска
    public void ResetBall(Vector3 spawnPosition)
    {
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;
        isLaunched = false;
    }

}
