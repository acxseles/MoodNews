import { useState, useEffect } from 'react';
import { fetchAllNews, fetchNewsRewrite } from '../api/newsApi';

export const useNewsRewrite = () => {
  const [newsList, setNewsList] = useState([]);
  const [selectedNewsId, setSelectedNewsId] = useState('');
  const [selectedMood, setSelectedMood] = useState('joyful');
  
  const [loadingList, setLoadingList] = useState(true);
  const [loadingRewrite, setLoadingRewrite] = useState(false);
  
  const [rewrittenData, setRewrittenData] = useState(null);
  const [error, setError] = useState(null);

  // Загружаем список новостей при старте
  useEffect(() => {
    const loadNews = async () => {
      setLoadingList(true);
      try {
        const data = await fetchAllNews();
        setNewsList(data);
        if (data.length > 0) {
          setSelectedNewsId(data[0].id); // По умолчанию выбираем первую новость
        }
      } catch (err) {
        setError(err.message || 'Ошибка при загрузке новостей');
      } finally {
        setLoadingList(false);
      }
    };

    loadNews();
  }, []);

  const handleRewrite = async () => {
    if (!selectedNewsId) return;

    setLoadingRewrite(true);
    setError(null);

    try {
      const data = await fetchNewsRewrite(selectedNewsId, selectedMood);
      setRewrittenData(data);
    } catch (err) {
      setError(err.message || 'Не удалось выполнить рерайт');
    } finally {
      setLoadingRewrite(false);
    }
  };

  return {
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
  };
};