import React, { useState } from 'react';
import { useChat } from '../context/ChatContext';
import { X, XCircle } from 'lucide-react';
import UserSearch from './UserSearch';

interface CreateChatModalProps {
  onClose: () => void;
}

const CreateChatModal: React.FC<CreateChatModalProps> = ({ onClose }) => {
  const { createChat } = useChat();
  const [chatName, setChatName] = useState('');
  const [isGroup, setIsGroup] = useState(true);
  const [selectedUsers, setSelectedUsers] = useState<any[]>([]);

  const handleCreate = async () => {
    if (isGroup && !chatName) {
      alert('Please enter a chat name');
      return;
    }
    await createChat(chatName || 'Private Chat', isGroup, selectedUsers.map(u => u.id));
    onClose();
  };

  const addUser = (user: any) => {
    if (!selectedUsers.find(u => u.id === user.id)) {
      setSelectedUsers([...selectedUsers, user]);
    }
  };

  const removeUser = (id: string) => {
    setSelectedUsers(selectedUsers.filter(u => u.id !== id));
  };

  return (
    <div style={{
      position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
      backgroundColor: 'rgba(0,0,0,0.7)', display: 'flex', justifyContent: 'center', alignItems: 'center',
      zIndex: 1000
    }}>
      <div style={{
        backgroundColor: '#36393f', width: '440px', borderRadius: '8px', padding: '24px',
        display: 'flex', flexDirection: 'column', gap: '20px'
      }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2 style={{ fontSize: '20px' }}>Create New Chat</h2>
          <X size={20} onClick={onClose} style={{ cursor: 'pointer', color: '#b9bbbe' }} />
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '8px', color: '#b9bbbe', fontSize: '12px', fontWeight: 'bold', textTransform: 'uppercase' }}>Chat Type</label>
          <div style={{ display: 'flex', gap: '10px' }}>
            <button 
              onClick={() => setIsGroup(true)}
              style={{ flex: 1, padding: '8px', backgroundColor: isGroup ? '#5865f2' : '#4f545c', color: 'white', borderRadius: '4px' }}
            >Group Chat</button>
            <button 
              onClick={() => setIsGroup(false)}
              style={{ flex: 1, padding: '8px', backgroundColor: !isGroup ? '#5865f2' : '#4f545c', color: 'white', borderRadius: '4px' }}
            >Private Chat</button>
          </div>
        </div>

        {isGroup && (
          <div>
            <label style={{ display: 'block', marginBottom: '8px', color: '#b9bbbe', fontSize: '12px', fontWeight: 'bold', textTransform: 'uppercase' }}>Chat Name</label>
            <input 
              value={chatName}
              onChange={(e) => setChatName(e.target.value)}
              placeholder="e.g. Awesome Project"
              style={{ width: '100%', padding: '10px', backgroundColor: '#202225', border: 'none', color: 'white', borderRadius: '4px' }}
            />
          </div>
        )}

        <div>
          <label style={{ display: 'block', marginBottom: '8px', color: '#b9bbbe', fontSize: '12px', fontWeight: 'bold', textTransform: 'uppercase' }}>Add Members</label>
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px', marginBottom: '12px' }}>
            {selectedUsers.map(user => (
              <div key={user.id} style={{ backgroundColor: '#202225', padding: '4px 8px', borderRadius: '4px', display: 'flex', alignItems: 'center', gap: '5px', fontSize: '14px' }}>
                {user.username}
                <XCircle size={14} style={{ cursor: 'pointer', color: '#f04747' }} onClick={() => removeUser(user.id)} />
              </div>
            ))}
          </div>
          <UserSearch onSelect={addUser} excludeIds={selectedUsers.map(u => u.id)} />
        </div>

        <div style={{ display: 'flex', gap: '12px', marginTop: '10px' }}>
          <button onClick={onClose} style={{ flex: 1, padding: '12px', backgroundColor: 'transparent', color: 'white' }}>Cancel</button>
          <button 
            onClick={handleCreate}
            style={{ flex: 1, padding: '12px', backgroundColor: '#5865f2', color: 'white', borderRadius: '4px', fontWeight: 'bold' }}
          >Create Chat</button>
        </div>
      </div>
    </div>
  );
};

export default CreateChatModal;
