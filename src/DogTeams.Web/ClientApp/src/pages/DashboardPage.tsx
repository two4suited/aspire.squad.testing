import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getTeams } from '../api/teams';
import { Team } from '../types';

export default function DashboardPage() {
  const { user, logout } = useAuth();
  const [teams, setTeams] = useState<Team[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getTeams()
      .then(setTeams)
      .catch(err => setError(err instanceof Error ? err.message : 'Failed to load teams'));
  }, []);

  return (
    <div style={{ padding: 24 }}>
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Dog Teams</h1>
        <div>
          <span>Welcome, {user?.name}</span>
          <button onClick={logout} style={{ marginLeft: 12 }}>Sign out</button>
        </div>
      </header>

      <section>
        <h2>Teams</h2>
        {error && <p style={{ color: 'red' }}>{error}</p>}
        {teams.length === 0 && !error && <p>No teams yet.</p>}
        <ul>
          {teams.map(team => (
            <li key={team.id}>
              <Link to={`/teams/${team.id}`}>{team.name}</Link>
              {team.description && <span> — {team.description}</span>}
            </li>
          ))}
        </ul>
      </section>
    </div>
  );
}
