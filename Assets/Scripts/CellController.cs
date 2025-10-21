using UnityEngine;

public class CellController : MonoBehaviour
{
    // Ссылка на главный управляющий скрипт
    private GameManager gameManager;
    
    /// Инициализация при создании клетки
    void Start()
    {
        // Нахожу GameManager в сцене
        // FindFirstObjectByType ищу первый объект указанного типа
        gameManager = FindFirstObjectByType<GameManager>();
    }
    
    /// Обработчик клика мышью по клетке
    /// Вызывается автоматически Unity при клике на объект с Collider
    void OnMouseDown()
    {
        // Проверяю, что GameManager найден и готов к работе
        if (gameManager != null)
        {
            // Получаю координаты этой клетки в игровой сетке
            Vector2Int gridPos = gameManager.GetGridPosition(gameObject);
            
            // Проверяю, что координаты валидные
            if (gridPos.x >= 0 && gridPos.y >= 0)
            {
                // Вызываю метод GameManager для изменения состояния клетки
                gameManager.ToggleCell(gridPos.x, gridPos.y);
            }
        }
    }
}

