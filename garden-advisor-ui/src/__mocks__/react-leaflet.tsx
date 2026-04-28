import React from 'react';

export const MapContainer = ({ children }: { children?: React.ReactNode }) => (
  <div data-testid="map-container">{children}</div>
);
export const TileLayer = () => null;
export const Polygon = () => null;
export const CircleMarker = () => null;
export const useMap = jest.fn(() => ({}));
export const useMapEvent = jest.fn();
export const useMapEvents = jest.fn(() => ({}));
