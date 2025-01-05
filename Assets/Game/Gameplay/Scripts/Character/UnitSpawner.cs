using Game.GameEngine.Ecs;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public GameObject unitPrefab; // Префаб юнита
    public int unitCount = 20;    // Количество юнитов
    public Vector3 spawnAreaSize; // Размер области спауна
    public Vector3 spawnAreaCenter; // Центр области спауна
    public GameObject characterPool; // Контейнер для юнитов

    private void Start()
    {
        // Проверка на правильность установки объектов в инспекторе
        if (unitPrefab == null)
        {
            Debug.LogError("unitPrefab не установлен!");
            return;
        }

        if (characterPool == null)
        {
            Debug.LogError("CharacterPool не установлен!");
            return;
        }

        // Проверка на корректные размеры области спауна
        if (spawnAreaSize.x <= 0 || spawnAreaSize.z <= 0)
        {
            Debug.LogError("Неверный размер области спауна!");
            return;
        }

        Debug.Log("Начало спауна юнитов");

        // Спавним юнитов
        for (int i = 0; i < unitCount; i++)
        {
            // Генерация случайной позиции для спауна
            Vector3 spawnPosition = spawnAreaCenter + new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                0, // Если спавн на земле (всегда на одном уровне по Y)
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
            );

            Debug.Log($"Создаём юнита {i + 1} на позиции {spawnPosition}");

            // Создаем новый юнит и помещаем в контейнер
            GameObject newUnit = Instantiate(unitPrefab, spawnPosition, Quaternion.identity);
            newUnit.transform.parent = characterPool.transform; // Устанавливаем родителя для юнитов
        }

        Debug.Log("Спаунинг завершен");
    }
}