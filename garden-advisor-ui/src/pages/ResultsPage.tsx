import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { MapContainer, TileLayer, Polygon, CircleMarker, Tooltip } from 'react-leaflet';
import { GardenDesignResult, LatLng } from '../types';

function getCategoryEmoji(name: string): string {
  const n = name.toLowerCase();
  if (n.includes('tomato')) return '🍅';
  if (n.includes('carrot')) return '🥕';
  if (n.includes('lettuce') || n.includes('kale')) return '🥬';
  if (n.includes('cucumber')) return '🥒';
  if (n.includes('pepper')) return '🌶️';
  if (n.includes('broccoli')) return '🥦';
  if (n.includes('pea') || n.includes('bean')) return '🫛';
  if (n.includes('strawberry')) return '🍓';
  if (n.includes('blueberry')) return '🫐';
  if (n.includes('sunflower')) return '🌻';
  if (n.includes('vegetable')) return '🥦';
  if (n.includes('fruit')) return '🍓';
  if (n.includes('flower')) return '🌸';
  return '🌱';
}

export default function ResultsPage() {
  const navigate = useNavigate();
  const [result, setResult] = useState<GardenDesignResult | null>(null);
  const [gardenPolygon, setGardenPolygon] = useState<LatLng[]>([]);
  const [userEmail, setUserEmail] = useState<string | null>(null);

  useEffect(() => {
    const resultRaw = sessionStorage.getItem('gardenResult');
    const areaRaw = sessionStorage.getItem('gardenArea');
    const sessionRaw = sessionStorage.getItem('userSession');

    if (resultRaw) setResult(JSON.parse(resultRaw));
    if (areaRaw) {
      const area = JSON.parse(areaRaw);
      setGardenPolygon(area.polygon || []);
    }
    if (sessionRaw) {
      const s = JSON.parse(sessionRaw);
      setUserEmail(s.email || null);
    }
  }, []);

  const handleStartOver = () => {
    sessionStorage.clear();
    navigate('/');
  };

  if (!result) {
    return (
      <div style={{ textAlign: 'center', padding: '3rem' }}>
        <p>No results found. <button className="btn btn-primary" onClick={() => navigate('/')}>Start Over</button></p>
      </div>
    );
  }

  const mapCenter: [number, number] = gardenPolygon.length > 0
    ? [
        gardenPolygon.reduce((s, p) => s + p.lat, 0) / gardenPolygon.length,
        gardenPolygon.reduce((s, p) => s + p.lng, 0) / gardenPolygon.length,
      ]
    : [39, -95];

  const plantPositions = result.schedule
    .filter(s => s.recommendedPlotPosition)
    .map(s => ({ plant: s.plant, pos: s.recommendedPlotPosition! }));

  return (
    <div>
      <header className="page-header">
        <span className="garden-logo">🌱</span>
        <div>
          <h1>GardenAdvisor</h1>
          <div className="subtitle">Your personalized garden planning assistant</div>
        </div>
      </header>

      <div className="step-indicator">
        {['Location', 'Plants', 'Login', 'Results'].map((label, i) => (
          <React.Fragment key={label}>
            {i > 0 && <div className="step-divider" />}
            <div className="step completed">
              <div className="step-num">✓</div>
              {label}
            </div>
          </React.Fragment>
        ))}
      </div>

      <div className="page-content">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
          <h1 style={{ color: '#2d6a4f', fontSize: '1.8rem' }}>Your Garden Plan 🌿</h1>
          <div style={{ display: 'flex', gap: '0.75rem' }}>
            <button className="btn btn-secondary" onClick={() => window.print()}>🖨️ Print / Download</button>
            <button className="btn btn-danger" onClick={handleStartOver}>Start Over</button>
          </div>
        </div>

        {/* Climate Zone */}
        <div className="results-section">
          <h2>🌡️ Your Climate Zone</h2>
          <div className="climate-zone-card">
            <div className="zone-badge">{result.climateZone.zoneCode}</div>
            <div className="zone-info">
              <h3>{result.climateZone.zoneName}</h3>
              <p>{result.climateZone.description}</p>
              <div className="zone-details">
                <span className="zone-detail-chip">🌸 Last frost: {result.climateZone.lastFrostMonth}</span>
                <span className="zone-detail-chip">🍂 First frost: {result.climateZone.firstFrostMonth}</span>
                <span className="zone-detail-chip">☀️ {result.climateZone.averageFrostFreeDays} frost-free days</span>
                <span className="zone-detail-chip">🌍 {result.climateZone.koppenClassification}</span>
              </div>
            </div>
          </div>
        </div>

        {/* Planting Schedule */}
        {result.schedule.length > 0 && (
          <div className="results-section card">
            <h2>📅 Planting Schedule</h2>
            <div style={{ overflowX: 'auto' }}>
              <table className="schedule-table">
                <thead>
                  <tr>
                    <th>Plant</th>
                    <th>Start Indoors</th>
                    <th>Direct Sow</th>
                    <th>Transplant</th>
                    <th>Harvest</th>
                    <th>Notes</th>
                  </tr>
                </thead>
                <tbody>
                  {result.schedule.map((item, i) => (
                    <tr key={i}>
                      <td><strong>{item.plant.name}</strong></td>
                      <td>{item.startIndoorsMonth ? <span className="month-tag">{item.startIndoorsMonth}</span> : '—'}</td>
                      <td>{item.directSowMonth ? <span className="month-tag">{item.directSowMonth}</span> : '—'}</td>
                      <td>{item.transplantMonth ? <span className="month-tag">{item.transplantMonth}</span> : '—'}</td>
                      <td>
                        {item.harvestStartMonth && item.harvestEndMonth
                          ? <span className="month-tag">{item.harvestStartMonth} – {item.harvestEndMonth}</span>
                          : '—'}
                      </td>
                      <td style={{ fontSize: '0.82rem', color: '#666' }}>{item.notes}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* Garden Map */}
        {gardenPolygon.length >= 3 && (
          <div className="results-section card">
            <h2>🗺️ Where to Plant</h2>
            <div className="map-container">
              <MapContainer center={mapCenter} zoom={18} style={{ height: '100%', width: '100%' }}>
                <TileLayer
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                  attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                />
                <Polygon
                  positions={gardenPolygon.map(p => [p.lat, p.lng] as [number, number])}
                  pathOptions={{ color: '#2d6a4f', fillColor: '#52b788', fillOpacity: 0.3, weight: 2 }}
                />
                {plantPositions.map((pp, i) => (
                  <CircleMarker
                    key={i}
                    center={[pp.pos.lat, pp.pos.lng]}
                    radius={8}
                    pathOptions={{ color: '#c2410c', fillColor: '#fb923c', fillOpacity: 0.9 }}
                  >
                    <Tooltip permanent={false}>{pp.plant.name}</Tooltip>
                  </CircleMarker>
                ))}
              </MapContainer>
            </div>
          </div>
        )}

        {/* Additional Recommendations */}
        {result.additionalRecommendations.length > 0 && (
          <div className="results-section card">
            <h2>💡 Additional Recommendations</h2>
            <div className="recommendation-grid">
              {result.additionalRecommendations.map((plant, i) => (
                <div key={i} className="recommendation-card">
                  <div className="icon">{getCategoryEmoji(plant.name)}</div>
                  <h4>{plant.name}</h4>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Warnings */}
        {result.warnings.length > 0 && (
          <div className="results-section card">
            <h2>⚠️ Warnings</h2>
            <div className="warning-list">
              {result.warnings.map((w, i) => (
                <div key={i} className="warning-item">
                  <span className="warn-icon">⚠️</span>
                  <div>
                    <strong>{w.plant.name}</strong>
                    <p>{w.reason}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Email Notification */}
        {userEmail && (
          <div className="card" style={{ background: '#f0f9f4', border: '1px solid #b7e4c7' }}>
            <p style={{ color: '#2d6a4f', fontSize: '0.95rem' }}>
              📧 Your schedule will be emailed to <strong>{userEmail}</strong>
            </p>
          </div>
        )}
      </div>
    </div>
  );
}
