using Entities;
using Game.GameEngine.Ecs;
using System.Collections.Generic;
using UnityEngine;

public sealed class UnitSelectionController : MonoBehaviour
{
    private List<Entity> selectedUnits = new List<Entity>();
    private Vector3 selectionStartPosition;
    private bool isSelecting;

    [SerializeField]
    private RectTransform selectionBox;

    private void Update()
    {
        // Начало выделения
        if (Input.GetMouseButtonDown(0))
        {
            isSelecting = true;
            selectionStartPosition = Input.mousePosition;
            selectionBox.gameObject.SetActive(true);
        }

        // Процесс выделения
        if (isSelecting)
        {
            Vector3 currentMousePosition = Input.mousePosition;
            Vector3 lowerLeft = new Vector3(
                Mathf.Min(selectionStartPosition.x, currentMousePosition.x),
                Mathf.Min(selectionStartPosition.y, currentMousePosition.y)
            );
            Vector3 upperRight = new Vector3(
                Mathf.Max(selectionStartPosition.x, currentMousePosition.x),
                Mathf.Max(selectionStartPosition.y, currentMousePosition.y)
            );

            // Обновляем визуальное отображение области выделения
            selectionBox.position = lowerLeft;
            selectionBox.sizeDelta = upperRight - lowerLeft;
        }

        // Завершение выделения
        if (Input.GetMouseButtonUp(0))
        {
            isSelecting = false;
            selectionBox.gameObject.SetActive(false);
            SelectUnitsInBox();
        }
    }

    private void SelectUnitsInBox()
    {
        selectedUnits.Clear();

        // Находим всех юнитов в сцене
        CharacterEntity[] allUnits = FindObjectsOfType<CharacterEntity>();

        foreach (var unit in allUnits)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(unit.transform.position);
            if (IsPointInSelectionBox(screenPosition))
            {
                selectedUnits.Add(unit);
            }
        }
    }

    private bool IsPointInSelectionBox(Vector3 screenPoint)
    {
        return screenPoint.x >= selectionBox.position.x &&
               screenPoint.x <= (selectionBox.position.x + selectionBox.sizeDelta.x) &&
               screenPoint.y >= selectionBox.position.y &&
               screenPoint.y <= (selectionBox.position.y + selectionBox.sizeDelta.y);
    }

    // Метод для отправки команды выбранным юнитам
    public void CommandSelectedUnits(Vector3 destination)
    {
        foreach (var unit in selectedUnits)
        {
            unit.SetData(new CommandRequest
            {
                type = CommandType.MOVE_TO_POSITION,
                args = destination,
                status = CommandStatus.IDLE
            });
        }
    }
}