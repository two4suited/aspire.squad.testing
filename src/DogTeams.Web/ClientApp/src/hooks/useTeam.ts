import { useState, useEffect } from 'react';
import { Team } from '../types';
import { getTeam } from '../api/teams';

export function useTeam(id: string | undefined) {
  const [team, setTeam] = useState<Team | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    setLoading(true);
    getTeam(id)
      .then(setTeam)
      .catch(err => setError(err instanceof Error ? err.message : 'Error'))
      .finally(() => setLoading(false));
  }, [id]);

  return { team, loading, error };
}
