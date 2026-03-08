import { useEffect, useState, FormEvent } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getTeam } from '../api/teams';
import { getOwners, createOwner, deleteOwner } from '../api/owners';
import { createDog, deleteDog } from '../api/dogs';
import { Team, Owner } from '../types';
import Modal from '../components/Modal';
import ErrorAlert from '../components/ErrorAlert';
import LoadingSpinner from '../components/LoadingSpinner';

export default function TeamPage() {
  const { id } = useParams<{ id: string }>();
  const [team, setTeam] = useState<Team | null>(null);
  const [owners, setOwners] = useState<Owner[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const [isOwnerModalOpen, setIsOwnerModalOpen] = useState(false);
  const [newOwnerName, setNewOwnerName] = useState('');
  const [newOwnerEmail, setNewOwnerEmail] = useState('');
  const [isCreatingOwner, setIsCreatingOwner] = useState(false);

  const [isDogModalOpen, setIsDogModalOpen] = useState(false);
  const [selectedOwnerId, setSelectedOwnerId] = useState('');
  const [newDogName, setNewDogName] = useState('');
  const [newDogBreed, setNewDogBreed] = useState('');
  const [newDogDOB, setNewDogDOB] = useState('');
  const [isCreatingDog, setIsCreatingDog] = useState(false);

  const [deletingId, setDeletingId] = useState<string | null>(null);

  const loadData = async () => {
    if (!id) {
      console.log('[TeamPage] No id in useParams');
      return;
    }
    setIsLoading(true);
    setError(null);
    console.log('[TeamPage] Loading team with id:', id);
    try {
      const [teamData, ownersData] = await Promise.all([getTeam(id), getOwners(id)]);
      console.log('[TeamPage] Successfully loaded team:', teamData);
      setTeam(teamData);
      setOwners(ownersData);
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Failed to load team data';
      console.log('[TeamPage] Error loading team:', errorMsg);
      setError(errorMsg);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [id]);

  async function handleCreateOwner(e: FormEvent) {
    e.preventDefault();
    if (!id) return;
    setError(null);
    setIsCreatingOwner(true);
    try {
      await createOwner(id, { name: newOwnerName, email: newOwnerEmail });
      setNewOwnerName('');
      setNewOwnerEmail('');
      setIsOwnerModalOpen(false);
      loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create owner');
    } finally {
      setIsCreatingOwner(false);
    }
  }

  async function handleCreateDog(e: FormEvent) {
    e.preventDefault();
    if (!id || !selectedOwnerId) return;
    setError(null);
    setIsCreatingDog(true);
    try {
      await createDog(selectedOwnerId, id, {
        name: newDogName,
        breed: newDogBreed,
        dateOfBirth: newDogDOB,
      });
      setNewDogName('');
      setNewDogBreed('');
      setNewDogDOB('');
      setSelectedOwnerId('');
      setIsDogModalOpen(false);
      loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create dog');
    } finally {
      setIsCreatingDog(false);
    }
  }

  async function handleDeleteOwner(ownerId: string) {
    if (!window.confirm('Delete this owner and all their dogs?')) return;
    setDeletingId(ownerId);
    try {
      await deleteOwner(ownerId);
      loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete owner');
    } finally {
      setDeletingId(null);
    }
  }

  async function handleDeleteDog(dogId: string) {
    if (!window.confirm('Delete this dog?')) return;
    setDeletingId(dogId);
    try {
      await deleteDog(dogId);
      loadData();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete dog');
    } finally {
      setDeletingId(null);
    }
  }

  if (isLoading) return <p style={{ padding: 24 }}>Loading…</p>;
  if (error && !team) return <p style={{ color: 'red', padding: 24 }}>❌ {error}</p>;
  if (!team) return <p style={{ padding: 24 }}>Team not found</p>;

  return (
    <div style={{ padding: 24, maxWidth: 1024, margin: '0 auto' }}>
      <Link to="/" style={{ color: '#007bff', textDecoration: 'none' }}>← Back to dashboard</Link>

      <h1 style={{ marginTop: 16, marginBottom: 8 }}>{team.name}</h1>
      {team.description && <p style={{ color: '#666', marginBottom: 24 }}>{team.description}</p>}

      {error && <ErrorAlert message={error} onDismiss={() => setError(null)} />}

      <section style={{ marginBottom: 32 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
          <h2 style={{ margin: 0 }}>👥 Owners</h2>
          <button onClick={() => setIsOwnerModalOpen(true)} style={{
            padding: '8px 16px',
            backgroundColor: '#28a745',
            color: 'white',
            border: 'none',
            borderRadius: 4,
            cursor: 'pointer',
            fontSize: 14,
          }}>
            + Add Owner
          </button>
        </div>

        {owners.length === 0 && <p style={{ color: '#666' }}>No owners yet.</p>}

        <div style={{ display: 'grid', gap: 12 }}>
          {owners.map(owner => (
            <div key={owner.id} style={{
              border: '1px solid #ddd',
              borderRadius: 6,
              padding: 16,
              backgroundColor: '#fafafa',
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start' }}>
                <div style={{ flex: 1 }}>
                  <h3 style={{ margin: '0 0 4px 0' }}>{owner.name}</h3>
                  <p style={{ margin: '0 0 8px 0', color: '#666', fontSize: 13 }}>✉️ {owner.email}</p>

                  <div style={{ marginTop: 12 }}>
                    <h4 style={{ margin: '0 0 8px 0', fontSize: 13, color: '#444' }}>🐕 Dogs ({owner.dogs.length})</h4>
                    {owner.dogs.length === 0 ? (
                      <p style={{ margin: 0, color: '#999', fontSize: 13 }}>No dogs yet</p>
                    ) : (
                      <ul style={{ margin: 0, paddingLeft: 16, fontSize: 13 }}>
                        {owner.dogs.map(dog => (
                          <li key={dog.id} style={{ marginBottom: 6, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                            <span><strong>{dog.name}</strong> ({dog.breed})</span>
                            <button
                              onClick={() => handleDeleteDog(dog.id)}
                              disabled={deletingId === dog.id}
                              style={{
                                padding: '2px 8px',
                                backgroundColor: deletingId === dog.id ? '#ccc' : '#dc3545',
                                color: 'white',
                                border: 'none',
                                borderRadius: 3,
                                cursor: deletingId === dog.id ? 'default' : 'pointer',
                                fontSize: 11,
                              }}
                            >
                              {deletingId === dog.id ? '⟳' : 'Delete'}
                            </button>
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                </div>
                <button
                  onClick={() => handleDeleteOwner(owner.id)}
                  disabled={deletingId === owner.id}
                  style={{
                    padding: '6px 12px',
                    backgroundColor: deletingId === owner.id ? '#ccc' : '#dc3545',
                    color: 'white',
                    border: 'none',
                    borderRadius: 4,
                    cursor: deletingId === owner.id ? 'default' : 'pointer',
                    fontSize: 13,
                    marginLeft: 12,
                  }}
                >
                  {deletingId === owner.id ? '⟳' : 'Delete'}
                </button>
              </div>
            </div>
          ))}
        </div>
      </section>

      <Modal isOpen={isOwnerModalOpen} title="Add Owner" onClose={() => setIsOwnerModalOpen(false)}>
        <form onSubmit={handleCreateOwner}>
          <div style={{ marginBottom: 12 }}>
            <label htmlFor="owner-name" style={{ display: 'block', marginBottom: 4 }}>Owner Name *</label>
            <input
              id="owner-name"
              type="text"
              value={newOwnerName}
              onChange={e => setNewOwnerName(e.target.value)}
              required
              style={{
                width: '100%',
                padding: 8,
                border: '1px solid #ddd',
                borderRadius: 4,
                boxSizing: 'border-box',
              }}
              placeholder="e.g., John Smith"
            />
          </div>
          <div style={{ marginBottom: 16 }}>
            <label htmlFor="owner-email" style={{ display: 'block', marginBottom: 4 }}>Email *</label>
            <input
              id="owner-email"
              type="email"
              value={newOwnerEmail}
              onChange={e => setNewOwnerEmail(e.target.value)}
              required
              style={{
                width: '100%',
                padding: 8,
                border: '1px solid #ddd',
                borderRadius: 4,
                boxSizing: 'border-box',
              }}
              placeholder="john@example.com"
            />
          </div>
          <div style={{ display: 'flex', gap: 8 }}>
            <button
              type="submit"
              disabled={isCreatingOwner}
              style={{
                flex: 1,
                padding: 10,
                backgroundColor: '#28a745',
                color: 'white',
                border: 'none',
                borderRadius: 4,
                cursor: isCreatingOwner ? 'default' : 'pointer',
                opacity: isCreatingOwner ? 0.7 : 1,
              }}
            >
              {isCreatingOwner ? <><LoadingSpinner />Adding...</> : 'Add Owner'}
            </button>
            <button
              type="button"
              onClick={() => setIsOwnerModalOpen(false)}
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

      <Modal isOpen={isDogModalOpen} title="Add Dog" onClose={() => setIsDogModalOpen(false)}>
        <form onSubmit={handleCreateDog}>
          <div style={{ marginBottom: 12 }}>
            <label htmlFor="dog-owner" style={{ display: 'block', marginBottom: 4 }}>Owner *</label>
            <select
              id="dog-owner"
              value={selectedOwnerId}
              onChange={e => setSelectedOwnerId(e.target.value)}
              required
              style={{
                width: '100%',
                padding: 8,
                border: '1px solid #ddd',
                borderRadius: 4,
                boxSizing: 'border-box',
              }}
            >
              <option value="">Select an owner</option>
              {owners.map(owner => (
                <option key={owner.id} value={owner.id}>{owner.name}</option>
              ))}
            </select>
          </div>
          <div style={{ marginBottom: 12 }}>
            <label htmlFor="dog-name" style={{ display: 'block', marginBottom: 4 }}>Dog Name *</label>
            <input
              id="dog-name"
              type="text"
              value={newDogName}
              onChange={e => setNewDogName(e.target.value)}
              required
              style={{
                width: '100%',
                padding: 8,
                border: '1px solid #ddd',
                borderRadius: 4,
                boxSizing: 'border-box',
              }}
              placeholder="e.g., Max"
            />
          </div>
          <div style={{ marginBottom: 12 }}>
            <label htmlFor="dog-breed" style={{ display: 'block', marginBottom: 4 }}>Breed *</label>
            <input
              id="dog-breed"
              type="text"
              value={newDogBreed}
              onChange={e => setNewDogBreed(e.target.value)}
              required
              style={{
                width: '100%',
                padding: 8,
                border: '1px solid #ddd',
                borderRadius: 4,
                boxSizing: 'border-box',
              }}
              placeholder="e.g., Golden Retriever"
            />
          </div>
          <div style={{ marginBottom: 16 }}>
            <label htmlFor="dog-dob" style={{ display: 'block', marginBottom: 4 }}>Date of Birth</label>
            <input
              id="dog-dob"
              type="date"
              value={newDogDOB}
              onChange={e => setNewDogDOB(e.target.value)}
              style={{
                width: '100%',
                padding: 8,
                border: '1px solid #ddd',
                borderRadius: 4,
                boxSizing: 'border-box',
              }}
            />
          </div>
          <div style={{ display: 'flex', gap: 8 }}>
            <button
              type="submit"
              disabled={isCreatingDog || !selectedOwnerId}
              style={{
                flex: 1,
                padding: 10,
                backgroundColor: '#28a745',
                color: 'white',
                border: 'none',
                borderRadius: 4,
                cursor: isCreatingDog || !selectedOwnerId ? 'default' : 'pointer',
                opacity: isCreatingDog || !selectedOwnerId ? 0.7 : 1,
              }}
            >
              {isCreatingDog ? <><LoadingSpinner />Adding...</> : 'Add Dog'}
            </button>
            <button
              type="button"
              onClick={() => setIsDogModalOpen(false)}
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

      <button onClick={() => setIsDogModalOpen(true)} disabled={owners.length === 0} style={{
        padding: '8px 16px',
        backgroundColor: owners.length === 0 ? '#ccc' : '#007bff',
        color: 'white',
        border: 'none',
        borderRadius: 4,
        cursor: owners.length === 0 ? 'default' : 'pointer',
        fontSize: 14,
        marginTop: 16,
      }}>
        + Add Dog
      </button>
    </div>
  );
}
