# Run all Hotel Management System microservices in parallel
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " Starting Online Hotel Management System Microservices..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$services = @(
    @{ Name = "AuthService"; Path = "AuthService"; Url = "https://localhost:7001" },
    @{ Name = "UserService"; Path = "UserService"; Url = "https://localhost:7002" },
    @{ Name = "RoomService"; Path = "RoomService"; Url = "https://localhost:7003" },
    @{ Name = "ReservationService"; Path = "ReservationService"; Url = "https://localhost:7004" },
    @{ Name = "PaymentService"; Path = "PaymentService"; Url = "https://localhost:7005" },
    @{ Name = "EmailService"; Path = "EmailService"; Url = "https://localhost:7006" },
    @{ Name = "ApiGateway"; Path = "ApiGateway"; Url = "https://localhost:7000" }
)

foreach ($s in $services) {
    Write-Host "Starting $($s.Name) on $($s.Url)..." -ForegroundColor Green
    Start-Process -FilePath "dotnet" -ArgumentList "run --project $($s.Path)" -NoNewWindow
}

Write-Host "`nAll services launched! Swagger UIs available at:" -ForegroundColor Yellow
Write-Host "  API Gateway:         https://localhost:7000/swagger" -ForegroundColor White
Write-Host "  AuthService:         https://localhost:7001/swagger" -ForegroundColor White
Write-Host "  UserService:         https://localhost:7002/swagger" -ForegroundColor White
Write-Host "  RoomService:         https://localhost:7003/swagger" -ForegroundColor White
Write-Host "  ReservationService:  https://localhost:7004/swagger" -ForegroundColor White
Write-Host "  PaymentService:      https://localhost:7005/swagger" -ForegroundColor White
Write-Host "  EmailService:        https://localhost:7006/swagger" -ForegroundColor White
