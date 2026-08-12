using Microsoft.EntityFrameworkCore;
using MoodNews.Data;
using MoodNews.Services.Rss;
using MoodNews.Services.Ai;

var builder = WebApplication.CreateBuilder(args);

// 1. Настройка подключения к БД
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

// 2. Сервисы
builder.Services.AddScoped<IRssService, RssService>();

// 3. HttpClient для GigaChat: Увеличиваем таймаут и отключаем SSL-проверки
builder.Services.AddHttpClient<NewsRewriterService>(client =>
{
    // Даем GigaChat до 60 секунд на ответ (при генерации больших новостей)
    client.Timeout = TimeSpan.FromSeconds(60);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Игнорируем проверки сертификатов Минцифры/Сбера
    ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4. Явный CORS для React (Vite)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "https://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

//ВАЖНО: CORS должен стоять В САМОМ НАЧАЛЕ пайплайна!
app.UseCors("AllowReactApp");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Закомментируйте редирект на HTTPS для локальной разработки, 
// если вы тестируете HTTP или из-за этого рвется HTTP/2 сокет
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();