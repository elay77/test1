namespace Test1.Services
{
    /// <summary>
    /// Типы фильтров для товаров
    /// </summary>
    public enum FilterType
    {
        None,
        Brand,
        Price,
        Sale
    }

    /// <summary>
    /// Менеджер фильтрации товаров
    /// </summary>
    public static class ProductFilter
    {
        private static FilterType _currentFilter = FilterType.None;
        private static string _searchQuery = string.Empty;

        /// <summary>
        /// Получить текущий активный фильтр
        /// </summary>
        public static FilterType GetCurrentFilter()
        {
            return _currentFilter;
        }

        /// <summary>
        /// Установить фильтр
        /// </summary>
        public static void SetFilter(FilterType filterType)
        {
            _currentFilter = filterType;
        }

        /// <summary>
        /// Сбросить фильтр
        /// </summary>
        public static void ClearFilter()
        {
            _currentFilter = FilterType.None;
        }

        /// <summary>
        /// Проверить, активен ли фильтр
        /// </summary>
        public static bool IsFilterActive()
        {
            return _currentFilter != FilterType.None;
        }

        /// <summary>
        /// Установить поисковый запрос
        /// </summary>
        public static void SetSearchQuery(string query)
        {
            _searchQuery = query ?? string.Empty;
        }

        /// <summary>
        /// Получить поисковый запрос
        /// </summary>
        public static string GetSearchQuery()
        {
            return _searchQuery;
        }

        /// <summary>
        /// Очистить поисковый запрос
        /// </summary>
        public static void ClearSearchQuery()
        {
            _searchQuery = string.Empty;
        }

        /// <summary>
        /// Проверить, есть ли активный поисковый запрос
        /// </summary>
        public static bool HasSearchQuery()
        {
            return !string.IsNullOrWhiteSpace(_searchQuery);
        }
    }
}

