import React from 'react';

export const NewsSelector = ({ 
  newsList = [], 
  selectedNewsId, 
  onSelectNews, 
  disabled 
}) => {
  const safeNewsList = newsList || [];

  return (
    <div className="space-y-2">
      <label 
        htmlFor="news-select" 
        className="block text-xs font-semibold uppercase tracking-wider text-stone-400"
      >
        Выберите новость из списка
      </label>

      <div className="relative">
        <select
          id="news-select"
          value={selectedNewsId || ''}
          onChange={(e) => {
            const val = e.target.value;
            // Передаем точно тот ID (число или строку), который ждет ваш хук
            if (onSelectNews) {
              onSelectNews(val && !isNaN(val) ? Number(val) : val);
            }
          }}
          disabled={disabled || safeNewsList.length === 0}
          className="w-full appearance-none bg-stone-50/80 border border-stone-200 text-stone-800 text-sm rounded-2xl p-4 pr-10 focus:outline-none focus:ring-2 focus:ring-stone-400 focus:bg-white transition-all duration-200 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed font-medium"
        >
          <option value="" disabled>
            {safeNewsList.length === 0 ? 'Загрузка новостей...' : '-- Выберите новость --'}
          </option>

          {safeNewsList.map((item) => (
            <option key={item.id} value={item.id} className="text-stone-800 py-1">
              {item.title}
            </option>
          ))}
        </select>

        <div className="pointer-events-none absolute inset-y-0 right-0 flex items-center px-4 text-stone-400">
          <svg className="w-4 h-4 fill-current" viewBox="0 0 20 20">
            <path d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" />
          </svg>
        </div>
      </div>
    </div>
  );
};