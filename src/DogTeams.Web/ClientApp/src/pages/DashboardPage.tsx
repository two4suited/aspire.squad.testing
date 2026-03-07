import { useEffect, useState, FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getTeams, createTeam, deleteTeam } from '../api/teams';
import { Team } from '../types';
import Modal from '../components/Modal';
import ErrorAlert from '../components/ErrorAlert';
import LoadingSpinner from '../components/LoadingSpinner';

export default function DashboardPage() {
  const { user, logout } = useAuth();
  const [teams, setTeams] = useState<Team[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [newTeamName, setNewTeamName] = useState('');
  const [newTeamDesc, setNewTeamDesc] = useState('');
  const [isCreating, setIsCreating] = useState(false);
  const [loadingTeamId, setLoadingTeamId] = useState<string | null>(null);

  const loadTeams = () => {
    getTeams()
      .then(setTeams)
      .catch(err => setError(err instanceof Error ? err.message : 'Failed to load teams'));
  };

  useEffect(() => {
    loadTeams();
  }, []);

  async function handleCreateTeam(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setIsCreating(true);
    try {
      await createTeam({ name: newTeamName, description: newTeamDesc });
      setNewTeamName('');
      setNewTeamDesc('');
      setIsModalOpen(false);
      loadTeams();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create team');
    } finally {
      setIsCreating(false);
    }
  }

  async function handleDeleteTeam(id: string) {
    if (!window.confirm('Are you sure you want to delete this team?')) return;
    setLoadingTeamId(id);
    try {
      await deleteTeam(id);
      loadTeams();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete team');
    } finally {
      setLoadingTeamId(null);
    }
  }

  return (
    <div style={{ padding: 24, maxWidth: 1024, margin: '0 auto' }}>
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 32 }}>
        <h1 style={{ margin: 0 }}>🐕 Dog Teams</h1>
        <div>
          <span style={{ marginRight: 16 }}>Welcome, <strong>{user?.name}</strong></span>
          <button onClick={logout} style={{
            padding: '8px 16px',
            backgroundColor: '#f5f5f5',
            border: '1px solid #ddd',
            borderRadius: 4,
            cursor: 'pointer',
          }}>
            Sign out
          </button>
        </div>
      </header>

      {error && <ErrorAlert message={error} onDismiss={() => setError(null)} />}

      <section>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
          <h2 style={{ margin: 0 }}>Teams</h2>
          <button onClick={() => setIsModalOpen(true)} style={{
            padding: '8px 16px',
            backgroundColor: '#007bff',
            color: 'white',
            border: 'none',
            borderRadius: 4,
            cursor: 'pointer',
            fontSize: 14,
          }}>
            + Add Team
          </button>
        </div>

        {teams.length === 0 && !error && <p style={{ color: '#666' }}>No teams yet. Create one to get started!</p>}

        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))',
          gap: 16,
        }}>
          {teams.map(team => (
            <div key={team.id} style={{
              border: '1px solid #e0e0e0',
              borderRadius: 8,
              padding: 16,
              backgroundColor: '#fafafa',
            }}>
              <h3 style={{ margin: '0 0 8px 0' }}>
                <Link to={`/teams/${team.id}`} style={{ textDecoration: 'none', color: '#007bff' }}>
                  {team.name}
                </Link>
              </h3>
              {team.description && <p style={{ margin: '8px 0', color: '#666', fontSize: 14 }}>{team.description}</p>}
              <div style={{ marginTop: 12, display: 'flex', gap: 8 }}>
                <Link to={`/teams/${team.id}`} style={{
                  flex: 1,
                  padding: '6px 12px',
                  backgroundColor: '#007bff',
                  color: 'white',
                  textDecoration: 'none',
                  borderRadius: 4,
                  textAlign: 'center',
                  fontSize: 13,
                }}>
                  View
                </Link>
                <button
                  onClick={() => handleDeleteTeam(team.id)}
                  disabled={loadingTeamId === team.id}
                  style={{
                    padding: '6px 12px',
                    backgroundColor: loadingTeamId === team.id ? '#ccc' : '#dc3545',
                    color: 'white',
                    border: 'none',
                    borderRadius: 4,
                    cursor: loadingTeamId === team.id ? 'default' : 'pointer',
                    fontSize: 13,
                  }}
                >
                  {loadingTeamId === team.id ? '⟳' : 'Delete'}
                </button>
              </div>
            </div>
          ))}
        </div>
      </section>

      <Modal isOpen={isModalOpen} title="Create Team" onClose={() => setIsModalOpen(false)}>
        <form onSubmit={handleCreateTeam}>
          <div style={{ marginBottom: 12 }}>
            <label htmlFor="team-name" style={{ display: 'block', marginBottom: 4 }}>Team Name *</label>
            <input
              id="team-name"
              type="text"
              value={newTeamName}
              onChange={e => setNewTeamName(e.target.value)}
              required
              style={{
                width: '100%',
                padding: 8,
                border: '1px solid #ddd',
                borderRadius: 4,
                boxSizing: 'border-box',
              }}
              placeholder="e.g., City Dog Club"
            />
          </div>
          <div style={{ marginBottom: 16 }}>
            <label htmlFor="team-desc" style={{ display: 'block', marginBottom: 4 }}>Description</label>
            <textarea
              id="team-desc"
              value={newTeamDesc}
              onChange={e => setNewTeamDesc(e.target.value)}
              style={{
                width: '100%',
                padding: 8,
                border: '1px solid #ddd',
                borderRadius: 4,
                boxSizing: 'border-box',
                minHeight: 80,
                fontFamily: 'inherit',
              }}
              placeholder="Team description (optional)"
            />
          </div>
          <div style={{ display: 'flex', gap: 8 }}>
            <button
              type="submit"
              disabled={isCreating}
              style={{
                flex: 1,
                padding: 10,
                backgroundColor: '#007bff',
                color: 'white',
                border: 'none',
                borderRadius: 4,
                cursor: isCreating ? 'default' : 'pointer',
                opacity: isCreating ? 0.7 : 1,
              }}
            >
              {isCreating ? <><LoadingSpinner />Creating...</> : 'Create Team'}
            </button>
            <button
              type="button"
              onClick={() => setIsModalOpen(false)}
              style={{
                flex: 1,
                padding: 10,
                backgroundColor: '#f5f5f5',
                border: '1px solid #ddd',
                borderRadius: 4,
                cursor: 'pointer',
              }}
            >
              Cancel
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
