import React, { useState, useEffect } from 'react';
import axiosInstance from '../api/axiosInstance';
import { Search } from 'lucide-react';

interface User {
  id: string;
  username: string;
  email: string;
}

interface UserSearchProps {
  onSelect: (user: User) => void;
  excludeIds?: string[];
}

const UserSearch: React.FC<UserSearchProps> = ({ onSelect, excludeIds = [] }) => {
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<User[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const searchUsers = async () => {
      if (query.length < 2) {
        setResults([]);
        return;
      }
      setLoading(true);
      try {
        const response = await axiosInstance.get(`/users/search?query=${query}`);
        setResults(response.data.filter((u: User) => !excludeIds.includes(u.id)));
      } catch (err) {
        console.error('Search failed', err);
      } finally {
        setLoading(false);
      }
    };

    const timeoutId = setTimeout(searchUsers, 300);
    return () => clearTimeout(timeoutId);
  }, [query, excludeIds]);

  return (
    <div style={{ width: '100%' }}>
      <div style={{ position: 'relative', marginBottom: '12px' }}>
        <Search size={16} style={{ position: 'absolute', left: '10px', top: '50%', transform: 'translateY(-50%)', color: '#b9bbbe' }} />
        <input
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder="Search by username or email..."
          style={{
            width: '100%',
            padding: '10px 10px 10px 35px',
            backgroundColor: '#202225',
            border: 'none',
            borderRadius: '4px',
            color: 'white'
          }}
        />
      </div>
      
      <div style={{ maxHeight: '200px', overflowY: 'auto' }}>
        {loading && <div style={{ padding: '8px', color: '#b9bbbe' }}>Searching...</div>}
        {results.map(user => (
          <div
            key={user.id}
            onClick={() => onSelect(user)}
            style={{
              padding: '8px 12px',
              cursor: 'pointer',
              borderRadius: '4px',
              display: 'flex',
              alignItems: 'center',
              gap: '10px',
              transition: 'background 0.2s'
            }}
            onMouseEnter={(e) => e.currentTarget.style.backgroundColor = 'rgba(255,255,255,0.05)'}
            onMouseLeave={(e) => e.currentTarget.style.backgroundColor = 'transparent'}
          >
            <div style={{ width: '32px', height: '32px', backgroundColor: '#4f545c', borderRadius: '50%' }} />
            <div>
              <div style={{ fontSize: '14px', fontWeight: 'bold' }}>{user.username}</div>
              <div style={{ fontSize: '12px', color: '#b9bbbe' }}>{user.email}</div>
            </div>
          </div>
        ))}
        {query.length >= 2 && !loading && results.length === 0 && (
          <div style={{ padding: '8px', color: '#b9bbbe' }}>No users found</div>
        )}
      </div>
    </div>
  );
};

export default UserSearch;
