using UnityEngine;

public class CharacterAiming : MonoBehaviour 
{
    public float aimDuration = 0.18f;
    public Transform cameraLookAt;
    public Cinemachine.AxisState xAxis = new Cinemachine.AxisState(-180, 180, true, false, 500, 0.02f, 0.02f, "Mouse X", false);
    public Cinemachine.AxisState yAxis = new Cinemachine.AxisState(-85, 85, false, false, 300, 0.02f, 0.02f, "Mouse Y", true);
    public bool isAiming;

    Animator animator;
    ActiveWeapon activeWeapon;
    int isAimingParam = Animator.StringToHash("isAiming");
    Vector3 cameraRotation;

    int frame = 0;

    // Start is called before the first frame update
    void Start() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        animator = GetComponent<Animator>();
        activeWeapon = GetComponent<ActiveWeapon>();

        xAxis.Value = transform.eulerAngles.y;
        yAxis.Value = 0;
    }

    void Update() {
        isAiming = Input.GetMouseButton(1);
        animator.SetBool(isAimingParam, isAiming);

        var weapon = activeWeapon.GetActiveWeapon();
        if (weapon) {
            weapon.recoil.recoilModifier = isAiming ? 0.3f : 1.0f;
        }

        RotateCamera(Time.deltaTime);
    }

    void RotateCamera(float deltaTime) {

        // Это для предотвращения резких скачков мыши при старте в редакторе.
        if (frame++ > 5)
        {
            xAxis.Update(deltaTime);
            yAxis.Update(deltaTime);
        }

        // Поворачиваем точку, за которой следит камера, по вертикали (вверх/вниз)
        cameraRotation.x = yAxis.Value;
        cameraLookAt.localEulerAngles = cameraRotation;

        // --- ИЗМЕНЕНИЯ ЗДЕСЬ ---
        // 1. Находим компонент AutoAim на этом же объекте (игроке).
        AutoAim autoAim = GetComponent<AutoAim>();

        // 2. Проверяем, активен ли авто-прицел.
        bool isAutoAiming = autoAim != null && autoAim.IsAutoAimEnabled;

        // Поворот персонажа влево/вправо
        // Поворачиваем ТОЛЬКО если:
        // - Не зажата клавиша F (старое условие) И
        // - Не активен авто-прицел (НОВОЕ УСЛОВИЕ)
        // Используем логическое ИЛИ (||), так как нам нужно поворачивать, если НЕ зажата F И НЕ активен авто-прицел.
        // Проще проверить условие, когда мы НЕ должны поворачивать.
        if (Input.GetKey(KeyCode.F) || isAutoAiming)
        {
            // Если зажата F ИЛИ активен авто-прицел, мы НЕ поворачиваем персонажа мышью.
            // В случае авто-прицела, AutoAim сам повернет персонажа.
        }
        else
        {
            // Иначе (F не зажата и авто-прицел выключен) - поворачиваем как обычно.
            var euler = transform.eulerAngles;
            euler.y = xAxis.Value;
            transform.eulerAngles = euler;
        }
    }
}