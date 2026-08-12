import React from 'react';
import { useNewsRewrite } from '../hooks/useNewsRewrite';
import { NewsSelector } from './NewsSelector';
import { MoodSelector } from './MoodSelector';
import { RewriteResult } from './RewriteResult';

export const NewsRewriter = () => {
  const {
    newsList,
    selectedNewsId,
    setSelectedNewsId,
    selectedMood,
    setSelectedMood,
    loadingList,
    loadingRewrite,
    rewrittenData,
    error,
    handleRewrite,
  } = useNewsRewrite();

  if (loadingList) {
    return (
      <div className="flex flex-col items-center justify-center py-16 space-y-3 text-stone-400 font-light">
        <div className="w-5 h-5 border-2 border-stone-400 border-t-transparent rounded-full animate-spin"></div>
        <p className="text-sm tracking-wide">Загрузка списка новостей...</p>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto my-8 p-8 bg-white/90 backdrop-blur-md rounded-3xl border border-stone-200/70 shadow-sm space-y-6">
      <div className="border-b border-stone-100 pb-4">
        <span className="text-xs font-semibold uppercase tracking-wider text-stone-400 block mb-1">
          Сервис рерайтинга
        </span>
        <h2 className="text-3xl font-serif font-normal text-stone-900 tracking-tight">
          AI Рерайтер Новостей
        </h2>
      </div>

      {/* Селектор новостей с защитой от undefined */}
      <NewsSelector
        newsList={newsList || []}
        selectedNewsId={selectedNewsId}
        onSelectNews={setSelectedNewsId}
        disabled={loadingRewrite}
      />

      {/* Селектор эмоций */}
      <div className="pt-2">
        <MoodSelector
          selectedMood={selectedMood}
          onSelectMood={setSelectedMood}
          disabled={loadingRewrite}
        />
      </div>

      {/* Кнопка генерации */}
      <button
        onClick={handleRewrite}
        disabled={loadingRewrite || !selectedNewsId}
        className={`w-full py-3.5 px-6 rounded-2xl font-medium text-sm transition-all duration-200 flex items-center justify-center gap-2 cursor-pointer ${
          loadingRewrite || !selectedNewsId
            ? 'bg-stone-100 text-stone-400 cursor-not-allowed border border-stone-200/50'
            : 'bg-stone-900 hover:bg-stone-800 text-stone-50 shadow-sm hover:shadow active:scale-[0.99]'
        }`}
      >
        {loadingRewrite ? (
          <>
            <span className="w-4 h-4 border-2 border-stone-400 border-t-transparent rounded-full animate-spin"></span>
            <span>GigaChat генерирует новость...</span>
          </>
        ) : (
          <span>Переписать выбранную новость</span>
        )}
      </button>

      {/* Вывод ошибки */}
      {error && (
        <div className="p-4 rounded-2xl bg-rose-50 border border-rose-200/60 text-rose-800 text-sm flex items-center gap-2">
          <span>⚠️</span>
          <span>{error}</span>
        </div>
      )}

      {/* Результат */}
      {!loadingRewrite && (
        <div className="pt-4 border-t border-stone-100">
          <RewriteResult data={rewrittenData} />
        </div>
      )}
    </div>
  );
};