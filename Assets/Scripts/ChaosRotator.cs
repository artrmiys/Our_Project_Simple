using UnityEngine;

public class ChaosRotator : MonoBehaviour
{
    [Header("»грок")]
    public Transform player;     // ссылка на игрока

    [Header("ќбъекты дл€ вращени€")]
    public Transform[] targets;  // 5 объектов

    [Header("ѕараметры движени€")]
    public float moveRadius = 1f;      // радиус хаотичного движени€
    public float moveSpeed = 1f;       // скорость перемещени€
    public float rotationSpeed = 20f;  // скорость вращени€

    private Vector3[] baseOffsets;     // фиксированное смещение каждой точки
    private Vector3[] randomOffsets;   // временные хаотичные смещени€

    void Start()
    {
        baseOffsets = new Vector3[targets.Length];
        randomOffsets = new Vector3[targets.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            // сохран€ем стартовое положение как смещение от игрока
            baseOffsets[i] = targets[i].position - player.position;

            // добавл€ем небольшое хаотичное смещение
            randomOffsets[i] = Random.insideUnitSphere * moveRadius;
        }
    }

    void Update()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null) continue;

            // вращение на месте
            targets[i].Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            targets[i].Rotate(Vector3.right, rotationSpeed * 0.5f * Time.deltaTime);

            // итогова€ цель = позици€ игрока + индивидуальное смещение + случайное "шевеление"
            Vector3 targetPos = player.position + baseOffsets[i] + randomOffsets[i];

            // плавное движение к цели
            targets[i].position = Vector3.MoveTowards(
                targets[i].position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            // если дошли Ч назначаем новое хаотичное смещение
            if (Vector3.Distance(targets[i].position, targetPos) < 0.1f)
            {
                randomOffsets[i] = Random.insideUnitSphere * moveRadius;
            }
        }
    }
}


