const API_BASE_URL = 'https://localhost:7239/api';

// Получение списка всех новостей
export const fetchAllNews = async () => {
  const response = await fetch(`${API_BASE_URL}/News`);
  if (!response.ok) {
    throw new Error(`Не удалось загрузить список новостей: ${response.status}`);
  }
  return await response.json();
};

// Запрос на рерайт конкретной новости
export const fetchNewsRewrite = async (newsId, mood) => {
  const response = await fetch(`${API_BASE_URL}/News/${newsId}/rewrite?mood=${mood}`);
  if (!response.ok) {
    throw new Error(`Ошибка сервера: ${response.status}`);
  }
  return await response.json();
};