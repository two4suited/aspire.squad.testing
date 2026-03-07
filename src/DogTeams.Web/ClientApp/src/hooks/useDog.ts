import { useState, useEffect } from 'react';
import { Dog } from '../types';
import { getDog } from '../api/dogs';

export function useDog(id: string | undefined) {
  const [dog, setDog] = useState<Dog | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    setLoading(true);
    getDog(id)
      .then(setDog)
      .catch(err => setError(err instanceof Error ? err.message : 'Error'))
      .finally(() => setLoading(false));
  }, [id]);

  return { dog, loading, error };
}
