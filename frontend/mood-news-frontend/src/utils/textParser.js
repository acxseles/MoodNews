export const parseRewrittenContent = (rawText) => {
  if (!rawText) return '';

  let cleaned = String(rawText).trim();

  // 1. Убираем markdown-обертки ```json ... ```
  cleaned = cleaned.replace(/^```(?:json)?\s*/i, '').replace(/\s*```$/i, '');

  // 2. Пробуем распарсить как JSON
  try {
    const parsed = JSON.parse(cleaned);

    // Если внутри JSON объект с полями title / text / content
    if (typeof parsed === 'object' && parsed !== null) {
      return parsed.text || parsed.content || parsed.rewrittenText || parsed.message || JSON.stringify(parsed);
    }
    
    // Если распарсилась просто строка в кавычках
    if (typeof parsed === 'string') {
      return parsed;
    }
  } catch {
    // Не JSON — работаем как с обычным текстом
  }

  // 3. Дополнительная чистка от случайных кавычек по краям
  cleaned = cleaned.replace(/^"|"$/g, '').replace(/^'|'$/g, '');

  return cleaned;
};