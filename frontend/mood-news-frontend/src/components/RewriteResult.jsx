import React from 'react';

export const RewriteResult = ({ data }) => {
  if (!data) return null;

  // Безопасное извлечение полей с подстраховкой под любой регистр
  const title = data.rewrittenTitle || data.RewrittenTitle || data.title || data.Title || '';
  const text = data.rewrittenText || data.RewrittenText || data.text || data.Text || '';

  if (!title && !text) return null;

  return (
    <div className="space-y-4 pt-4 border-t border-stone-100">
      {title && (
        <h3 className="text-2xl font-serif text-stone-900 leading-snug">
          {title}
        </h3>
      )}
      {text && (
        <p className="text-stone-600 leading-relaxed text-base whitespace-pre-line font-light">
          {text}
        </p>
      )}
    </div>
  );
};