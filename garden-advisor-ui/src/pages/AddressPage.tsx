import React, { useState, useCallback, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import { MapContainer, TileLayer, Polygon, CircleMarker, useMapEvents } from 'react-leaflet';
import { LatLng, Address, GardenArea } from '../types';
import { searchAddress } from '../services/apiService';
import type { Map as LeafletMap } from 'leaflet';

function calculateArea(points: LatLng[]): number {
  if (points.length < 3) return 0;
  const R = 6371000;
  let area = 0;
  const n = points.length;
  for (let i = 0; i < n; i++) {
    const j = (i + 1) % n;
    const xi = (points[i].lng * Math.PI) / 180 * R * Math.cos((points[i].lat * Math.PI) / 180);
    const yi = (points[i].lat * Math.PI) / 180 * R;
    const xj = (points[j].lng * Math.PI) / 180 * R * Math.cos((points[j].lat * Math.PI) / 180);
    const yj = (points[j].lat * Math.PI) / 180 * R;
    area += xi * yj - xj * yi;
  }
  return Math.abs(area / 2);
}

interface MapClickHandlerProps {
  onMapClick: (latlng: LatLng) => void;
  onMapDblClick: () => void;
  mapRef: React.MutableRefObject<LeafletMap | null>;
}

function MapClickHandler({ onMapClick, onMapDblClick, mapRef }: MapClickHandlerProps) {
  const map = useMapEvents({
    click(e) {
      onMapClick({ lat: e.latlng.lat, lng: e.latlng.lng });
    },
    dblclick() {
      onMapDblClick();
    },
  });

  React.useEffect(() => {
    mapRef.current = map;
  }, [map, mapRef]);

  return null;
}

export default function AddressPage() {
  const navigate = useNavigate();
  const [query, setQuery] = useState('');
  const [suggestions, setSuggestions] = useState<Address[]>([]);
  const [selectedAddress, setSelectedAddress] = useState<Address | null>(null);
  const [searching, setSearching] = useState(false);
  const [polygonPoints, setPolygonPoints] = useState<LatLng[]>([]);
  const [finalized, setFinalized] = useState(false);
  const mapRef = useRef<LeafletMap | null>(null);

  const area = calculateArea(polygonPoints);
  const areasqft = area * 10.7639;

  const handleSearch = useCallback(async () => {
    if (!query.trim()) return;
    setSearching(true);
    try {
      const results = await searchAddress(query);
      setSuggestions(results);
    } catch {
      setSuggestions([]);
    } finally {
      setSearching(false);
    }
  }, [query]);

  const handleSelectAddress = (addr: Address) => {
    setSelectedAddress(addr);
    setSuggestions([]);
    setQuery(addr.displayName);
    if (mapRef.current) {
      mapRef.current.flyTo([addr.latitude, addr.longitude], 18);
    }
  };

  const handleMapClick = (latlng: LatLng) => {
    if (finalized) return;
    setPolygonPoints(prev => [...prev, latlng]);
  };

  const handleMapDblClick = () => {
    if (finalized) return;
    setPolygonPoints(prev => {
      if (prev.length >= 2) {
        return prev.slice(0, -2);
      }
      return prev;
    });
    setFinalized(true);
  };

  const handleClearArea = () => {
    setPolygonPoints([]);
    setFinalized(false);
  };

  const handleContinue = () => {
    if (!selectedAddress || polygonPoints.length < 3) return;
    const gardenArea: GardenArea = { polygon: polygonPoints, areaSquareMeters: area };
    sessionStorage.setItem('gardenAddress', JSON.stringify(selectedAddress));
    sessionStorage.setItem('gardenArea', JSON.stringify(gardenArea));
    navigate('/plants');
  };

  const canContinue = !!selectedAddress && polygonPoints.length >= 3;

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
        <div className="step active">
          <div className="step-num">1</div>
          Location
        </div>
        <div className="step-divider" />
        <div className="step">
          <div className="step-num">2</div>
          Plants
        </div>
        <div className="step-divider" />
        <div className="step">
          <div className="step-num">3</div>
          Login
        </div>
        <div className="step-divider" />
        <div className="step">
          <div className="step-num">4</div>
          Results
        </div>
      </div>

      <div className="page-content">
        <div className="card">
          <h2>📍 Enter Your Home Address</h2>
          <p className="hint">Search for your address to find the best plants for your climate zone.</p>

          <div className="input-group">
            <input
              type="text"
              value={query}
              onChange={e => setQuery(e.target.value)}
              onKeyDown={e => e.key === 'Enter' && handleSearch()}
              placeholder="e.g. 123 Main St, Springfield, IL"
            />
            <button className="btn btn-primary" onClick={handleSearch} disabled={searching}>
              {searching ? 'Searching...' : 'Search'}
            </button>
          </div>

          {suggestions.length > 0 && (
            <div className="suggestions">
              {suggestions.map((addr, i) => (
                <div key={i} className="suggestion-item" onClick={() => handleSelectAddress(addr)}>
                  {addr.displayName}
                </div>
              ))}
            </div>
          )}

          {selectedAddress && (
            <div className="selected-address">
              ✅ {selectedAddress.displayName}
            </div>
          )}
        </div>

        <div className="card">
          <h2>🗺️ Select Your Garden Area</h2>
          <div className="map-instructions">
            📌 Click on the map to draw the outline of your garden area. Click each corner of the area, then double-click to complete the selection.
          </div>

          <div className="map-container">
            <MapContainer
              center={[39, -95]}
              zoom={4}
              doubleClickZoom={false}
              style={{ height: '100%', width: '100%' }}
            >
              <TileLayer
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
              />
              <MapClickHandler
                onMapClick={handleMapClick}
                onMapDblClick={handleMapDblClick}
                mapRef={mapRef}
              />
              {polygonPoints.length >= 3 && (
                <Polygon
                  positions={polygonPoints.map(p => [p.lat, p.lng] as [number, number])}
                  pathOptions={{ color: '#2d6a4f', fillColor: '#52b788', fillOpacity: 0.3, weight: 2 }}
                />
              )}
              {polygonPoints.map((pt, i) => (
                <CircleMarker
                  key={i}
                  center={[pt.lat, pt.lng]}
                  radius={5}
                  pathOptions={{ color: '#2d6a4f', fillColor: '#52b788', fillOpacity: 1 }}
                />
              ))}
            </MapContainer>
          </div>

          {polygonPoints.length >= 3 && (
            <div className="area-info">
              📐 Garden Area: <strong>{area.toFixed(2)} m²</strong> ({areasqft.toFixed(2)} sq ft)
            </div>
          )}

          {polygonPoints.length > 0 && (
            <div className="btn-row">
              <button className="btn btn-danger" onClick={handleClearArea}>
                🗑️ Clear Area
              </button>
            </div>
          )}
        </div>

        <div className="btn-row">
          <button
            className="btn btn-primary btn-large"
            disabled={!canContinue}
            onClick={handleContinue}
          >
            Continue to Plant Selection →
          </button>
        </div>
      </div>
    </div>
  );
}
