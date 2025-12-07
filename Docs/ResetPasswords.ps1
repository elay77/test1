# Скрипт для установки паролей пользователям в базе данных
# Используйте этот скрипт, если нужно установить известные пароли для тестирования

# Параметры подключения к базе данных
$connectionString = "Data Source=localhost;Initial Catalog=work;Integrated Security=True;TrustServerCertificate=True"

# Функция для хеширования пароля (SHA256 + Base64)
function Hash-Password {
    param([string]$password)
    
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($password)
    $hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash($bytes)
    return [Convert]::ToBase64String($hash)
}

# Устанавливаем пароли для пользователей
# Измените пароли по необходимости

Write-Host "Установка паролей для пользователей..." -ForegroundColor Cyan

$passwords = @{
    "1" = "1"                    # Пароль для пользователя "1"
    "eco_lover" = "password123"  # Пароль для пользователя "eco_lover"
    "nature_friend" = "password123"
    "green_shop" = "password123"
    "organic_buyer" = "password123"
    "eco_admin" = "admin123"
}

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    foreach ($username in $passwords.Keys) {
        $password = $passwords[$username]
        $hashedPassword = Hash-Password -password $password
        
        $query = "UPDATE CP_Users SET password_hash = @hash WHERE username = @username"
        $command = New-Object System.Data.SqlClient.SqlCommand($query, $connection)
        $command.Parameters.AddWithValue("@hash", $hashedPassword) | Out-Null
        $command.Parameters.AddWithValue("@username", $username) | Out-Null
        
        $rowsAffected = $command.ExecuteNonQuery()
        
        if ($rowsAffected -gt 0) {
            Write-Host "✓ Пароль установлен для пользователя: $username (пароль: $password)" -ForegroundColor Green
        } else {
            Write-Host "✗ Пользователь не найден: $username" -ForegroundColor Yellow
        }
    }
    
    $connection.Close()
    Write-Host "`nГотово! Теперь вы можете войти с этими паролями." -ForegroundColor Green
    Write-Host "`nСписок пользователей и паролей:" -ForegroundColor Cyan
    foreach ($username in $passwords.Keys) {
        Write-Host "  - $username : $($passwords[$username])" -ForegroundColor White
    }
}
catch {
    Write-Host "Ошибка: $_" -ForegroundColor Red
}

