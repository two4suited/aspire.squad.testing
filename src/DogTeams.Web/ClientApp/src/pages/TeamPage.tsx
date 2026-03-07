import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getTeam } from '../api/teams';
import { getOwners } from '../api/owners';
import { Team, Owner } from '../types';

export default function TeamPage() {
  const { id } = useParams<{ id: string }>();
  const [team, setTeam] = useState<Team | null>(null);
  const [owners, setOwners] = useState<Owner[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    Promise.all([getTeam(id), getOwners(id)])
      .then(([t, o]) => { setTeam(t); setOwners(o); })
      .catch(err => setError(err instanceof Error ? err.message : 'Failed to load team'));
  }, [id]);

  if (error) return <p style={{ color: 'red', padding: 24 }}>{error}</p>;
  if (!team) return <p style={{ padding: 24 }}>Loading…</p>;

  return (
    <div style={{ padding: 24 }}>
      <Link to="/">← Back to dashboard</Link>
      <h1>{team.name}</h1>
      {team.description && <p>{team.description}</p>}

      <h2>Owners</h2>
      {owners.length === 0 && <p>No owners yet.</p>}
      <ul>
        {owners.map(owner => (
          <li key={owner.id}>
            <strong>{owner.name}</strong> ({owner.email})
            {owner.dogs.length > 0 && (
              <ul>
                {owner.dogs.map(dog => (
                  <li key={dog.id}>{dog.name} — {dog.breed}</li>
                ))}
              </ul>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
}
