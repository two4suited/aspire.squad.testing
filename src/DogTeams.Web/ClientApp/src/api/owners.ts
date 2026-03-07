import { apiFetch } from './client';
import { Owner } from '../types';

export function getOwners(teamId?: string): Promise<Owner[]> {
  const qs = teamId ? `?teamId=${teamId}` : '';
  return apiFetch<Owner[]>(`/api/owners${qs}`);
}

export function getOwner(id: string): Promise<Owner> {
  return apiFetch<Owner>(`/api/owners/${id}`);
}

export function createOwner(teamId: string, data: Omit<Owner, 'id' | 'dogs' | 'createdAt' | 'teamId' | 'userId'>): Promise<Owner> {
  return apiFetch<Owner>('/api/owners', {
    method: 'POST',
    body: JSON.stringify({ ...data, teamId }),
  });
}

export function updateOwner(id: string, data: Partial<Omit<Owner, 'id' | 'dogs' | 'createdAt'>>): Promise<Owner> {
  return apiFetch<Owner>(`/api/owners/${id}`, { method: 'PUT', body: JSON.stringify(data) });
}

export function deleteOwner(id: string): Promise<void> {
  return apiFetch<void>(`/api/owners/${id}`, { method: 'DELETE' });
}
