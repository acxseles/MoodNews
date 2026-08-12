import React from 'react';
import { NewsRewriter } from './components/NewsRewriter';

function App() {
  return (
    <div style={{ minHeight: '100vh', backgroundColor: '#f1f5f9', padding: '40px 20px' }}>
      <NewsRewriter newsId={1} />
    </div>
  );
}

export default App;