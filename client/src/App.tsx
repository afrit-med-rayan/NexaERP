import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';

function App() {
  return (
    <Router>
      <div className="app-container">
        <Routes>
          <Route path="/" element={<Navigate to="/dashboard" />} />
          <Route path="/dashboard" element={<div className="glass-panel" style={{margin: '2rem', padding: '2rem'}}><h1>Dashboard placeholder</h1></div>} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
