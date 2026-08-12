# 📰 MoodNews — AI-Powered News Journal

**MoodNews** — веб-приложение для чтения новостей с функцией эмоциональной адаптации контента с помощью искусственного интеллекта. Сервис загружает свежие RSS-новости и позволяет пользователю переписать любую статью в выбранной тональности (радостно, печально, иронично, драматично) с помощью **GigaChat API**.

---

## 🛠 Технологический стек

### **Backend (.NET 8 Web API)**
* **Платформа:** C# / .NET 8
* **База данных:** MySQL + Entity Framework Core (Code First)
* **Интеграция с ИИ:** GigaChat API (через `HttpClient` с авто-обновлением OAuth-токенов)
* **Архитектура:** Service Layer Pattern, DTO mapping, кастомный JSON-парсер ответов LLM, кэширование рерайтов в БД.

### **Frontend (React + Vite)**
* **Фреймворк:** React 18 (Vite)
* **Стилизация:** Tailwind CSS v4 + журнальная типографика (Editorial Minimalist)
* **Архитектура:** Модульные компоненты (`NewsSelector`, `MoodSelector`, `RewriteResult`) + кастомные хуки (`useNewsRewrite`).


---

## 📁 Структура проекта

```text
MoodNews/
├── backend/                  # .NET Web API
│   ├── Controllers/          # API контроллеры (NewsController)
│   ├── DTOs/                 # Data Transfer Objects (NewsRewriteDto)
│   ├── Entities/             # Сущности EF Core (News, NewsRewrite)
│   ├── Services/             # Бизнес-логика (RssService, NewsRewriterService)
│   ├── Data/                 # DbContext и миграции EF Core
│   └── Program.cs            # Конфигурация DI, CORS и Middleware
│
└── frontend/                 # React приложение (Vite)
    ├── src/
    │   ├── components/       # UI компоненты (NewsSelector, MoodSelector, etc.)
    │   ├── hooks/            # Кастомный хук (useNewsRewrite)
    │   ├── constants/        # Константы настроений и API
    │   └── index.css         # Tailwind v4 стили и шрифты
    └── vite.config.js
```

---

## 🚀 Быстрый запуск проекта

### **Предварительные требования**
* **.NET SDK 8.0**
* **Node.js v18+** и **npm**
* **MySQL Server** (локальный или удалённый)
* Авторизационные данные **GigaChat API** (Client Secret / Auth Key)

---

### **1. Настройка и запуск Бэкенда (.NET)**

1. Перейдите в папку бэкенда:
   ```bash
   cd MoodNews
   ```

2. Откройте `appsettings.json` и укажите настройки подключения к вашей БД(скрипт бд будет в папке script) и ключи GigaChat:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=moodnews_db;User=your_user;Password=your_password;"
     },
     "GigaChat": {
       "AuthData": "YOUR_GIGACHAT_AUTH_KEY_HERE",
       "Scope": "GIGACHAT_API_PERS"
     }
   }
   ```

3. Запустите API сервер:
   ```bash
   dotnet run
   ```

---

### **2. Настройка и запуск Фронтенда (React)**

1. Перейдите в папку фронтенда:
   ```bash
   cd frontend
   ```

2. Установите зависимости:
   ```bash
   npm install
   ```

3. Запустите dev-сервер Vite:
   ```bash
   npm run dev
   ```
   *Приложение будет доступно по адресу: `http://localhost:5173`.*

---

## 📡 Эндпоинты API

| Метод | Эндпоинт | Описание |
| :--- | :--- | :--- |
| `GET` | `/api/news` | Получить список всех исходных RSS-новостей |
| `GET` | `/api/news/{id}` | Получить новость по ID |
| `GET` | `/api/news/{id}/rewrite?mood={mood}` | Переписать новость под выбранное настроение (`joyful`, `sad`, `ironic`, `dramatic`, `neutral`) |

---

## 🎭 Поддерживаемые эмоции (Moods)

* ☀️ **Радостно (`joyful`)** — фокус на позитиве и надежде.
* 🌧️ **Печально (`sad`)** — меланхоличный тон.
* 😏 **Иронично (`ironic`)** — тонкая сатира и юмор.
* 🎭 **Драма (`dramatic`)** — экспрессивная подача.
* 😐 **Оригинал (`neutral`)** — исходный текст новости.
