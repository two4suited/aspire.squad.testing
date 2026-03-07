import { apiFetch } from './client';
import { Team } from '../types';

export function getTeams(): Promise<Team[]> {
  return apiFetch<Team[]>('/api/teams');
}

export function getTeam(id: string): Promise<Team> {
  return apiFetch<Team>(`/api/teams/${id}`);
}

export function createTeam(data: Omit<Team, 'id' | 'createdAt'>): Promise<Team> {
  return apiFetch<Team>('/api/teams', { method: 'POST', body: JSON.stringify(data) });
}

export function updateTeam(id: string, data: Partial<Omit<Team, 'id' | 'createdAt'>>): Promise<Team> {
  return apiFetch<Team>(`/api/teams/${id}`, { method: 'PUT', body: JSON.stringify(data) });
}

export function deleteTeam(id: string): Promise<void> {
  return apiFetch<void>(`/api/teams/${id}`, { method: 'DELETE' });
}
