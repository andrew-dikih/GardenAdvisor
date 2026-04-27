import axios from 'axios';
import { Address, Plant, PlantCategory, GardenRequest } from '../types';

const API_BASE = process.env.REACT_APP_API_URL || 'http://localhost:5001/api';

const api = axios.create({
  baseURL: API_BASE,
  withCredentials: true,
});

export const searchAddress = async (query: string): Promise<Address[]> => {
  const response = await api.get<Address[]>(`/address/search?query=${encodeURIComponent(query)}`);
  return response.data;
};

export const getPlantCategories = async (): Promise<PlantCategory[]> => {
  const response = await api.get<PlantCategory[]>('/plants/categories');
  return response.data;
};

export const getPlantsByCategory = async (categoryId: string): Promise<Plant[]> => {
  const response = await api.get<Plant[]>(`/plants/category/${categoryId}`);
  return response.data;
};

export const startGardenDesign = async (request: GardenRequest): Promise<{ sessionId: string; message: string }> => {
  const response = await api.post<{ sessionId: string; message: string }>('/garden/design', request);
  return response.data;
};

export const registerEmail = async (email: string, name?: string): Promise<{ sessionId: string; email: string; name: string }> => {
  const response = await api.post('/auth/register-email', { email, name });
  return response.data;
};
