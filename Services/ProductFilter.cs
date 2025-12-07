namespace Test1.Services
{
    /// <summary>
    /// Типы фильтров для товаров в каталоге
    /// Используются для сортировки и фильтрации товаров
    /// </summary>
    public enum FilterType
    {
        None,   // Фильтр не применен
        Brand,  // Фильтр по бренду
        Price,  // Фильтр по цене (сортировка)
        Sale    // Фильтр по рейтингу (товары с высоким рейтингом)
    }

    /// <summary>
    /// Менеджер фильтрации и поиска товаров
    /// Хранит состояние текущего фильтра и поискового запроса
    /// Используется для управления отображением товаров в каталоге
    /// </summary>
    public static class ProductFilter
    {
        // Текущий активный фильтр
        private static FilterType _currentFilter = FilterType.None;
        // Текущий поисковый запрос
        private static string _searchQuery = string.Empty;

        /// <summary>
        /// Получить текущий активный фильтр
        /// </summary>
        /// <returns>Тип текущего фильтра</returns>
        public static FilterType GetCurrentFilter()
        {
            return _currentFilter;
        }

        /// <summary>
        /// Установить фильтр для товаров
        /// </summary>
        /// <param name="filterType">Тип фильтра, который нужно применить</param>
        public static void SetFilter(FilterType filterType)
        {
            _currentFilter = filterType;
        }

        /// <summary>
        /// Сбросить текущий фильтр
        /// Устанавливает фильтр в состояние None (без фильтра)
        /// </summary>
        public static void ClearFilter()
        {
            _currentFilter = FilterType.None;
        }

        /// <summary>
        /// Проверить, активен ли какой-либо фильтр
        /// </summary>
        /// <returns>true, если фильтр активен, иначе false</returns>
        public static bool IsFilterActive()
        {
            return _currentFilter != FilterType.None;
        }

        /// <summary>
        /// Установить поисковый запрос
        /// Используется для поиска товаров по названию или описанию
        /// </summary>
        /// <param name="query">Текст поискового запроса</param>
        public static void SetSearchQuery(string query)
        {
            _searchQuery = query ?? string.Empty; // Если query null, используем пустую строку
        }

        /// <summary>
        /// Получить текущий поисковый запрос
        /// </summary>
        /// <returns>Текст поискового запроса</returns>
        public static string GetSearchQuery()
        {
            return _searchQuery;
        }

        /// <summary>
        /// Очистить поисковый запрос
        /// Устанавливает поисковый запрос в пустую строку
        /// </summary>
        public static void ClearSearchQuery()
        {
            _searchQuery = string.Empty;
        }

        /// <summary>
        /// Проверить, есть ли активный поисковый запрос
        /// </summary>
        /// <returns>true, если поисковый запрос не пустой, иначе false</returns>
        public static bool HasSearchQuery()
        {
            return !string.IsNullOrWhiteSpace(_searchQuery);
        }
    }
}

