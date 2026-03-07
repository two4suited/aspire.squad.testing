import { useState, useEffect } from 'react';
import { Owner } from '../types';
import { getOwner } from '../api/owners';

export function useOwner(id: string | undefined) {
  const [owner, setOwner] = useState<Owner | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    setLoading(true);
    getOwner(id)
      .then(setOwner)
      .catch(err => setError(err instanceof Error ? err.message : 'Error'))
      .finally(() => setLoading(false));
  }, [id]);

  return { owner, loading, error };
}
