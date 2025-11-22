# Скрипт для проверки подключения к базе данных
# Замените параметры подключения на свои

param(
    [string]$Server = "localhost",
    [string]$Database = "work",
    [string]$UserId = "",
    [string]$Password = "",
    [switch]$UseWindowsAuth = $true
)

Write-Host "=== Проверка подключения к базе данных ===" -ForegroundColor Cyan
Write-Host ""

# Формируем строку подключения
if ($UseWindowsAuth) {
    $connectionString = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
    Write-Host "Используется Windows Authentication" -ForegroundColor Yellow
} else {
    if ([string]::IsNullOrEmpty($UserId) -or [string]::IsNullOrEmpty($Password)) {
        Write-Host "Ошибка: Для SQL Authentication необходимо указать UserId и Password" -ForegroundColor Red
        exit 1
    }
    $connectionString = "Server=$Server;Database=$Database;User Id=$UserId;Password=$Password;TrustServerCertificate=True;"
    Write-Host "Используется SQL Authentication" -ForegroundColor Yellow
}

Write-Host "Строка подключения: $connectionString" -ForegroundColor Gray
Write-Host ""

try {
    # Загружаем модуль SQL Server
    $assembly = [System.Reflection.Assembly]::LoadWithPartialName("Microsoft.Data.SqlClient")
    if (-not $assembly) {
        Write-Host "Установка пакета Microsoft.Data.SqlClient..." -ForegroundColor Yellow
        dotnet add package Microsoft.Data.SqlClient --version 5.1.5
    }
    
    Add-Type -Path "$PSScriptRoot\bin\Debug\net8.0-windows\Microsoft.Data.SqlClient.dll" -ErrorAction SilentlyContinue
    
    $connection = New-Object Microsoft.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "✓ Подключение успешно!" -ForegroundColor Green
    Write-Host "  Сервер: $($connection.DataSource)" -ForegroundColor White
    Write-Host "  База данных: $($connection.Database)" -ForegroundColor White
    Write-Host "  Статус: $($connection.State)" -ForegroundColor White
    Write-Host ""
    
    # Проверяем таблицы
    $command = $connection.CreateCommand()
    $command.CommandText = @"
        SELECT TABLE_NAME 
        FROM INFORMATION_SCHEMA.TABLES 
        WHERE TABLE_SCHEMA = 'dbo' 
        AND TABLE_NAME LIKE 'CP_%'
        ORDER BY TABLE_NAME
"@
    
    $reader = $command.ExecuteReader()
    $tables = @()
    while ($reader.Read()) {
        $tables += $reader["TABLE_NAME"]
    }
    $reader.Close()
    
    Write-Host "Найденные таблицы с префиксом CP_:" -ForegroundColor Cyan
    if ($tables.Count -eq 0) {
        Write-Host "  ⚠ Таблицы не найдены!" -ForegroundColor Red
    } else {
        foreach ($table in $tables) {
            Write-Host "  ✓ $table" -ForegroundColor Green
        }
    }
    Write-Host ""
    
    # Проверяем необходимые таблицы
    $requiredTables = @("CP_Users", "CP_Products", "CP_Categories", "CP_Orders", "CP_OrderItems")
    $missingTables = $requiredTables | Where-Object { $tables -notcontains $_ }
    
    if ($missingTables.Count -eq 0) {
        Write-Host "✓ Все необходимые таблицы найдены!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Теперь можно выполнить Scaffold-DbContext:" -ForegroundColor Cyan
        Write-Host ""
        if ($UseWindowsAuth) {
            Write-Host "Scaffold-DbContext `"$connectionString`" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -ContextDir Data -Context ApplicationDbContext -Tables CP_Categories,CP_OrderItems,CP_Orders,CP_Products,CP_Users -Force" -ForegroundColor Yellow
        } else {
            Write-Host "Scaffold-DbContext `"$connectionString`" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -ContextDir Data -Context ApplicationDbContext -Tables CP_Categories,CP_OrderItems,CP_Orders,CP_Products,CP_Users -Force" -ForegroundColor Yellow
        }
    } else {
        Write-Host "⚠ Отсутствуют следующие таблицы:" -ForegroundColor Red
        foreach ($table in $missingTables) {
            Write-Host "  - $table" -ForegroundColor Red
        }
    }
    
    $connection.Close()
    
} catch {
    Write-Host "✗ Ошибка подключения: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Проверьте:" -ForegroundColor Yellow
    Write-Host "  1. Правильность имени сервера" -ForegroundColor White
    Write-Host "  2. Что база данных '$Database' существует" -ForegroundColor White
    Write-Host "  3. Что SQL Server запущен" -ForegroundColor White
    Write-Host "  4. Права доступа к базе данных" -ForegroundColor White
    exit 1
}

