import React from 'react';

const DEFAULT_MOODS = [
  { id: 'joyful', label: 'Радостно', emoji: '☀️' },
  { id: 'sad', label: 'Печально', emoji: '🌧️' },
  { id: 'ironic', label: 'Иронично', emoji: '😏' },
  { id: 'dramatic', label: 'Драма', emoji: '🎭' },
  { id: 'neutral', label: 'Нейтрально', emoji: '😐' },
];

export const MoodSelector = ({ 
  moods = DEFAULT_MOODS, 
  selectedMood, 
  onSelectMood, 
  disabled 
}) => {
  const safeMoods = moods || DEFAULT_MOODS;

  return (
    <div className="space-y-3">
      <label className="block text-xs font-semibold uppercase tracking-wider text-stone-400">
        Выберите настроение
      </label>

      <div className="flex flex-wrap gap-2">
        {safeMoods.map((m) => {
          const moodId = typeof m === 'string' ? m : m.id;
          const moodLabel = typeof m === 'string' ? m : m.label;
          const moodEmoji = typeof m === 'object' ? m.emoji : '';

          const isActive = selectedMood === moodId;

          return (
            <button
              key={moodId}
              type="button"
              onClick={() => onSelectMood && onSelectMood(moodId)}
              disabled={disabled}
              className={`px-4 py-2 rounded-full text-xs font-medium border flex items-center gap-1.5 cursor-pointer transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed ${
                isActive
                  ? 'bg-stone-900 text-stone-50 border-stone-900 shadow-xs scale-[1.02]'
                  : 'bg-stone-50 border-stone-200/80 text-stone-600 hover:bg-stone-100'
              }`}
            >
              {moodEmoji && <span>{moodEmoji}</span>}
              <span>{moodLabel}</span>
            </button>
          );
        })}
      </div>
    </div>
  );
};