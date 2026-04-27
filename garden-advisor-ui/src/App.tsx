import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import L from 'leaflet';
import iconUrl from 'leaflet/dist/images/marker-icon.png';
import iconShadow from 'leaflet/dist/images/marker-shadow.png';
import 'leaflet/dist/leaflet.css';
import AddressPage from './pages/AddressPage';
import PlantSelectionPage from './pages/PlantSelectionPage';
import LoginPage from './pages/LoginPage';
import ProcessingPage from './pages/ProcessingPage';
import ResultsPage from './pages/ResultsPage';
import './App.css';

const DefaultIcon = L.icon({ iconUrl, shadowUrl: iconShadow });
L.Marker.prototype.options.icon = DefaultIcon;

function App() {
  return (
    <Router>
      <div className="App">
        <Routes>
          <Route path="/" element={<AddressPage />} />
          <Route path="/plants" element={<PlantSelectionPage />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/processing" element={<ProcessingPage />} />
          <Route path="/results" element={<ResultsPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
