import { apiFetch } from './client';
import { Dog } from '../types';

export function getDogs(ownerId?: string): Promise<Dog[]> {
  const qs = ownerId ? `?ownerId=${ownerId}` : '';
  return apiFetch<Dog[]>(`/api/dogs${qs}`);
}

export function getDog(id: string): Promise<Dog> {
  return apiFetch<Dog>(`/api/dogs/${id}`);
}

export function createDog(ownerId: string, teamId: string, data: Omit<Dog, 'id' | 'createdAt' | 'ownerId' | 'teamId'>): Promise<Dog> {
  return apiFetch<Dog>('/api/dogs', {
    method: 'POST',
    body: JSON.stringify({ ...data, ownerId, teamId }),
  });
}

export function updateDog(id: string, data: Partial<Omit<Dog, 'id' | 'createdAt'>>): Promise<Dog> {
  return apiFetch<Dog>(`/api/dogs/${id}`, { method: 'PUT', body: JSON.stringify(data) });
}

export function deleteDog(id: string): Promise<void> {
  return apiFetch<void>(`/api/dogs/${id}`, { method: 'DELETE' });
}
