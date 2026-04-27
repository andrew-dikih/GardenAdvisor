import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { PlantCategory, Plant } from '../types';
import { getPlantCategories, getPlantsByCategory } from '../services/apiService';

function getCategoryEmoji(name: string): string {
  const n = name.toLowerCase();
  if (n.includes('vegetable')) return '🥦';
  if (n.includes('fruit')) return '🍓';
  if (n.includes('flower')) return '🌸';
  if (n.includes('herb')) return '🌿';
  return '🌱';
}

function getPlantEmoji(plant: Plant): string {
  const n = plant.name.toLowerCase();
  if (n.includes('tomato')) return '🍅';
  if (n.includes('carrot')) return '🥕';
  if (n.includes('lettuce') || n.includes('kale')) return '🥬';
  if (n.includes('cucumber')) return '🥒';
  if (n.includes('pepper')) return '🌶️';
  if (n.includes('broccoli')) return '🥦';
  if (n.includes('pea') || n.includes('bean')) return '🫛';
  if (n.includes('strawberry')) return '🍓';
  if (n.includes('blueberry')) return '🫐';
  if (n.includes('raspberry')) return '🍇';
  if (n.includes('watermelon')) return '🍉';
  if (n.includes('cantaloupe')) return '🍈';
  if (n.includes('pumpkin')) return '🎃';
  if (n.includes('sunflower')) return '🌻';
  if (n.includes('marigold')) return '🌸';
  if (n.includes('lavender')) return '💐';
  if (n.includes('basil')) return '🌿';
  const cat = plant.category?.toLowerCase() || '';
  if (cat.includes('vegetable')) return '🥦';
  if (cat.includes('fruit')) return '🍓';
  if (cat.includes('flower')) return '🌸';
  if (cat.includes('herb')) return '🌿';
  return '🌱';
}

export default function PlantSelectionPage() {
  const navigate = useNavigate();
  const [categories, setCategories] = useState<PlantCategory[]>([]);
  const [selectedCategoryIds, setSelectedCategoryIds] = useState<Set<string>>(new Set());
  const [plants, setPlants] = useState<Plant[]>([]);
  const [selectedPlantIds, setSelectedPlantIds] = useState<Set<string>>(new Set());
  const [loadingCategories, setLoadingCategories] = useState(true);
  const [loadingPlants, setLoadingPlants] = useState(false);

  useEffect(() => {
    getPlantCategories()
      .then(setCategories)
      .catch(() => setCategories([]))
      .finally(() => setLoadingCategories(false));
  }, []);

  useEffect(() => {
    if (selectedCategoryIds.size === 0) {
      setPlants([]);
      return;
    }
    setLoadingPlants(true);
    Promise.all(
      Array.from(selectedCategoryIds).map(id => getPlantsByCategory(id).catch(() => [] as Plant[]))
    )
      .then(results => {
        const merged = results.flat();
        const unique = Array.from(new Map(merged.map(p => [p.id, p])).values());
        setPlants(unique);
      })
      .finally(() => setLoadingPlants(false));
  }, [selectedCategoryIds]);

  const toggleCategory = (id: string) => {
    setSelectedCategoryIds(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const togglePlant = (id: string) => {
    setSelectedPlantIds(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const handleContinue = () => {
    sessionStorage.setItem('selectedPlantIds', JSON.stringify(Array.from(selectedPlantIds)));
    navigate('/login');
  };

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
        <div className="step completed">
          <div className="step-num">✓</div>
          Location
        </div>
        <div className="step-divider" />
        <div className="step active">
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
          <h2>🌿 Choose Plant Categories</h2>
          <p className="hint">Select one or more categories to see available plants.</p>

          {loadingCategories ? (
            <div className="loading-text">Loading categories...</div>
          ) : categories.length === 0 ? (
            <div className="loading-text">No categories available. Please check API connection.</div>
          ) : (
            <div className="category-grid">
              {categories.map(cat => (
                <div
                  key={cat.id}
                  className={`category-card${selectedCategoryIds.has(cat.id) ? ' selected' : ''}`}
                  onClick={() => toggleCategory(cat.id)}
                >
                  <div className="icon">{cat.icon || getCategoryEmoji(cat.name)}</div>
                  <h3>{cat.name}</h3>
                  <p>{cat.description}</p>
                </div>
              ))}
            </div>
          )}
        </div>

        {selectedCategoryIds.size > 0 && (
          <div className="card">
            <div style={{ display: 'flex', alignItems: 'center', marginBottom: '1rem' }}>
              <h2 style={{ margin: 0 }}>🌱 Select Plants to Grow</h2>
              <span className="selected-count">{selectedPlantIds.size} selected</span>
            </div>

            {loadingPlants ? (
              <div className="loading-text">Loading plants...</div>
            ) : plants.length === 0 ? (
              <div className="loading-text">No plants found for selected categories.</div>
            ) : (
              <div className="plant-grid">
                {plants.map(plant => (
                  <div
                    key={plant.id}
                    className={`plant-card${selectedPlantIds.has(plant.id) ? ' selected' : ''}`}
                    onClick={() => togglePlant(plant.id)}
                  >
                    <div className="plant-icon">{getPlantEmoji(plant)}</div>
                    <h4>{plant.name}</h4>
                    <div className="plant-meta">☀️ {plant.sunRequirement}</div>
                    <div className="plant-meta">⏱ {plant.daysToMaturity}d</div>
                    {selectedPlantIds.has(plant.id) && (
                      <div className="selected-check">✓</div>
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        <div className="btn-row">
          <button className="btn btn-secondary" onClick={() => navigate('/')}>
            ← Back
          </button>
          <button
            className="btn btn-primary btn-large"
            disabled={selectedPlantIds.size === 0}
            onClick={handleContinue}
          >
            Continue to Login →
          </button>
        </div>
      </div>
    </div>
  );
}
