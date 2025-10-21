using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // Настраиваю
    [Header("Настройки игры")]
    public GameObject cellPrefab;// Префаб клетки (шаблон)
    public int gridWidth = 20;// Ширина игрового поля (в клетках)
    public int gridHeight = 20;// Высота игрового поля (в клетках)
    public float simulationSpeed = 0.5f; // Задержка между поколениями в секундах
    
    //Внутренние переменные
    private bool[,] currentState;// Текущее состояние клеток (true = жива, false = мертва)
    private bool[,] nextState;// Будущее состояние клеток (рассчитывается заранее)
    private GameObject[,] grid;// Массив игровых объектов клеток
    private bool isRunning = false;// Флаг работы симуляции
    private Coroutine simulationCoroutine; // Ссылка на корутину симуляции
    
    //Инициализация
    void Start()
    {
        InitializeGame(); // Вызов метода инициализации при старте сцены
    }
    
    /// Инициализация игрового поля и данных
    void InitializeGame()
    {
        // Создаю двумерные массивы для хранения состояний
        currentState = new bool[gridWidth, gridHeight];
        nextState = new bool[gridWidth, gridHeight];
        grid = new GameObject[gridWidth, gridHeight];
        
        CreateGrid();  // Создаю визуальную сетку клеток
        SetupCamera(); // Настраиваю камеру для обзора всего поля
    }
    
    /// Создает сетку игровых клеток на сцене
    void CreateGrid()
    {
        // Двойной цикл для прохода по всем координатам сетки
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Создаю позицию для клетки
                Vector3 position = new Vector3(x, y, 0);
                
                // Создаю объект клетки из префаба
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity);
                
                // Сохраняю ссылку на клетку в массив
                grid[x, y] = cell;
                
                // Даюшь понятное имя для отладки
                cell.name = $"Cell_{x}_{y}";
                
                // Изначально все клетки мертвые
                currentState[x, y] = false;
                
                // Обновляем внешний вид клетки
                UpdateCellVisual(x, y);
            }
        }
    }
    
    /// Настраивает камеру для просмотра всего игрового поля
    void SetupCamera()
    {
        // Позиционируем камеру в центр поля
        Camera.main.transform.position = new Vector3(gridWidth / 2, gridHeight / 2, -10);
        
        // Рассчитываем размер камеры чтобы видеть всё поле
        Camera.main.orthographicSize = Mathf.Max(gridWidth, gridHeight) / 2 + 2;
    }
    
    /// Обновляет визуальное отображение клетки по ее состоянию
    void UpdateCellVisual(int x, int y)
    {
        // Получила компонент отрисовки клетки
        SpriteRenderer renderer = grid[x, y].GetComponent<SpriteRenderer>();
        
        // Черный = живая, Белый = мертвая
        renderer.color = currentState[x, y] ? Color.black : Color.white;
    }
    
    //Основная логика
    
    /// Подсчитываю количество живых соседей вокруг клетки
    int CountNeighbors(int x, int y)
    {
        int count = 0; // Счетчик живых соседей
        
        // Проверяю все 8 соседних клеток (3x3 область без центра)
        for (int dx = -1; dx <= 1; dx++)   // Смещение по X
        {
            for (int dy = -1; dy <= 1; dy++) // Смещение по Y
            {
                if (dx == 0 && dy == 0) continue; // Пропускаю саму клетку
                
                // Координаты соседа
                int nx = x + dx;
                int ny = y + dy;
                
                // Проверяю, что сосед в пределах игрового поля
                if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight)
                {
                    // Если сосед живой, то увеличиваю счетчик
                    if (currentState[nx, ny]) count++;
                }
            }
        }
        
        return count;
    }
    
    /// Рассчитываю следующее поколение клеток по правилам Conway's Game of Life
    void NextGeneration()
    {
        // 1.Расчет следующего состояния для ВСЕХ клеток
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                int neighbors = CountNeighbors(x, y); // Подсчет соседей
                
                // Применяю правили, которые игра жизни
                if (currentState[x, y]) // Если клетка ЖИВАЯ
                {
                    nextState[x, y] = (neighbors == 2 || neighbors == 3);
                }
                else // Мертвая
                {
                    nextState[x, y] = (neighbors == 3);
                }
            }
        }
        
        // 2. Применение рассчитанного состояния
        // Сначала рассчитываем все состояния, потом обновляем
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                currentState[x, y] = nextState[x, y]; // Обновление состояния
                UpdateCellVisual(x, y);               // Обновление внешнего вида
            }
        }
    }
    
    //Управление симуляцие
    
    /// Запускает или останавливает симуляцию
    public void ToggleSimulation()
    {
        isRunning = !isRunning; // Переключаю флаг
        
        if (isRunning)
        {
            // Запускаю корутину для автоматического обновления
            simulationCoroutine = StartCoroutine(SimulationCoroutine());
        }
        else
        {
            // Останавливаю корутину, если она активна
            if (simulationCoroutine != null)
                StopCoroutine(simulationCoroutine);
        }
    }
    
    /// Корутина для автоматического выполнения симуляции
    IEnumerator SimulationCoroutine()
    {
        // Бесконечный цикл, пока симуляция активна
        while (isRunning)
        {
            NextGeneration(); // Расчет следующего поколения
            
            // Жду указанное время перед следующим шагом
            yield return new WaitForSeconds(simulationSpeed);
        }
    }
    
    // Методы для работы с полем
    
    /// Заполняю поле случайными живыми клетками
    public void RandomGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // 30% шанс, что клетка будет живой
                currentState[x, y] = (Random.Range(0, 100) < 30);
                UpdateCellVisual(x, y);
            }
        }
    }
    
    /// Очищение поля (все клетки становятся мертвыми)
    public void ClearGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                currentState[x, y] = false; // Мертвая клетка
                UpdateCellVisual(x, y);
            }
        }
    }
    
    /// Изменение состояния конкретной клетки по клику
    public void ToggleCell(int x, int y)
    {
        // Разрешаю изменение только когда симуляция остановлена
        if (!isRunning) 
        {
            currentState[x, y] = !currentState[x, y]; // Инвертирую состояние
            UpdateCellVisual(x, y);
        }
    }
    
    /// Нахожу координаты клетки в сетке по игровому объекту
    public Vector2Int GetGridPosition(GameObject cell)
    {
        // Поиск по всему массиву
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == cell)
                {
                    return new Vector2Int(x, y); // Нашли, тогда возвращаю координаты
                }
            }
        }
        return new Vector2Int(-1, -1); // Не нашла
    }
    
    // Интерфейс
    /// Отрисовка простого интерфейса управления
    void OnGUI()
    {
        // Кнопка Старт/Стоп
        if (GUI.Button(new Rect(10, 10, 100, 30), isRunning ? "Стоп" : "Старт"))
        {
            ToggleSimulation();
        }
        
        // Кнопка случайного заполнения
        if (GUI.Button(new Rect(120, 10, 100, 30), "Случайно"))
        {
            RandomGrid();
        }
        
        // Кнопка очистки поля
        if (GUI.Button(new Rect(230, 10, 100, 30), "Очистить"))
        {
            ClearGrid();
        }
        
        // Отображение и регулировка скорости
        GUI.Label(new Rect(350, 10, 150, 30), $"Скорость: {simulationSpeed:F1}");
        simulationSpeed = GUI.HorizontalSlider(new Rect(350, 40, 150, 30), simulationSpeed, 0.1f, 2f);
    }
}
