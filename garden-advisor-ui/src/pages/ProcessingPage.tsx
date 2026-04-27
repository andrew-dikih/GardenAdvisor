import React, { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { GardenRequest, GardenDesignResult } from '../types';
import { startGardenDesign } from '../services/apiService';
import { connectToHub, joinSession, onProcessingUpdate, onProcessingComplete, onProcessingError, disconnect } from '../services/websocketService';

function generateUUID(): string {
  if (typeof crypto !== 'undefined' && crypto.randomUUID) {
    return crypto.randomUUID();
  }
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
    const r = Math.random() * 16 | 0;
    return (c === 'x' ? r : ((r & 0x3) | 0x8)).toString(16);
  });
}

export default function ProcessingPage() {
  const navigate = useNavigate();
  const [logs, setLogs] = useState<string[]>(['Initializing...']);
  const [error, setError] = useState<string | null>(null);
  const logRef = useRef<HTMLDivElement>(null);
  const started = useRef(false);

  const addLog = (msg: string) => {
    setLogs(prev => [...prev, msg]);
    setTimeout(() => {
      if (logRef.current) {
        logRef.current.scrollTop = logRef.current.scrollHeight;
      }
    }, 50);
  };

  useEffect(() => {
    if (started.current) return;
    started.current = true;

    const run = async () => {
      try {
        const addressRaw = sessionStorage.getItem('gardenAddress');
        const areaRaw = sessionStorage.getItem('gardenArea');
        const plantsRaw = sessionStorage.getItem('selectedPlantIds');
        const sessionRaw = sessionStorage.getItem('userSession');

        if (!addressRaw || !areaRaw || !plantsRaw) {
          setError('Missing session data. Please start over.');
          return;
        }

        const address = JSON.parse(addressRaw);
        const gardenArea = JSON.parse(areaRaw);
        const selectedPlantIds = JSON.parse(plantsRaw);
        const userSession = sessionRaw ? JSON.parse(sessionRaw) : null;

        const sessionId = userSession?.sessionId || generateUUID();

        const request: GardenRequest = {
          sessionId,
          address,
          gardenArea,
          selectedPlantIds,
          userEmail: userSession?.email,
        };

        addLog('Connecting to server...');

        try {
          await connectToHub();
          await joinSession(sessionId);
          addLog('Connected to real-time updates.');

          onProcessingUpdate((data: { status: string; message: string }) => {
            addLog(`[${data.status}] ${data.message}`);
          });

          onProcessingComplete((data: GardenDesignResult) => {
            addLog('✅ Processing complete!');
            sessionStorage.setItem('gardenResult', JSON.stringify(data));
            setTimeout(() => navigate('/results'), 500);
          });

          onProcessingError((data: { message: string }) => {
            setError(`Processing failed: ${data.message}`);
          });
        } catch {
          addLog('⚠️ Real-time updates unavailable. Processing in background...');
        }

        addLog('Submitting garden design request...');
        try {
          await startGardenDesign(request);
          addLog('Request submitted. Waiting for results...');
        } catch {
          addLog('⚠️ API not reachable. Generating demo results...');
          const demoResult: GardenDesignResult = {
            sessionId,
            climateZone: {
              zoneCode: '6b',
              zoneName: 'USDA Zone 6b',
              description: 'Temperate climate with moderate winters',
              minTempF: -5,
              maxTempF: 0,
              koppenClassification: 'Dfa',
              averageFrostFreeDays: 170,
              lastFrostMonth: 'April',
              firstFrostMonth: 'October',
            },
            schedule: [],
            additionalRecommendations: [],
            warnings: [],
            layoutImageUrl: '',
            generatedAt: new Date().toISOString(),
          };
          sessionStorage.setItem('gardenResult', JSON.stringify(demoResult));
          addLog('✅ Demo results ready!');
          setTimeout(() => navigate('/results'), 1000);
        }
      } catch (err) {
        setError('An unexpected error occurred. Please try again.');
      }
    };

    run();

    return () => {
      disconnect().catch(() => {});
    };
  }, [navigate]);

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
        <div className="step completed">
          <div className="step-num">✓</div>
          Plants
        </div>
        <div className="step-divider" />
        <div className="step completed">
          <div className="step-num">✓</div>
          Login
        </div>
        <div className="step-divider" />
        <div className="step active">
          <div className="step-num">4</div>
          Results
        </div>
      </div>

      <div className="processing-container">
        {!error ? (
          <>
            <div className="processing-spinner" />
            <h2 style={{ fontSize: '1.5rem', color: '#2d6a4f', marginBottom: '0.5rem' }}>
              Designing Your Garden...
            </h2>
            <p style={{ color: '#666' }}>
              We're analyzing your location, climate, and plant preferences
            </p>
          </>
        ) : (
          <>
            <div style={{ fontSize: '3rem', marginBottom: '1rem' }}>⚠️</div>
            <h2 style={{ color: '#dc2626', marginBottom: '0.5rem' }}>Something went wrong</h2>
            <p style={{ color: '#666', marginBottom: '1.5rem' }}>{error}</p>
            <button className="btn btn-primary" onClick={() => navigate('/')}>
              Try Again
            </button>
          </>
        )}

        <div className="processing-log" ref={logRef}>
          {logs.map((log, i) => (
            <div key={i} className="log-entry">{log}</div>
          ))}
        </div>
      </div>
    </div>
  );
}
