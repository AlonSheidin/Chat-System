import React from 'react';
import { useChat } from '../context/ChatContext';
import { X } from 'lucide-react';
import UserSearch from './UserSearch';

interface AddMemberModalProps {
  onClose: () => void;
}

const AddMemberModal: React.FC<AddMemberModalProps> = ({ onClose }) => {
  const { activeChat, addMember } = useChat();

  const handleAdd = async (user: any) => {
    if (activeChat) {
      await addMember(activeChat.id, user.id);
      onClose();
    }
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
          <h2 style={{ fontSize: '20px' }}>Add Member to {activeChat?.name}</h2>
          <X size={20} onClick={onClose} style={{ cursor: 'pointer', color: '#b9bbbe' }} />
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '8px', color: '#b9bbbe', fontSize: '12px', fontWeight: 'bold', textTransform: 'uppercase' }}>Search User</label>
          <UserSearch onSelect={handleAdd} excludeIds={activeChat?.memberIds || []} />
        </div>

        <div style={{ display: 'flex', gap: '12px', marginTop: '10px' }}>
          <button onClick={onClose} style={{ flex: 1, padding: '12px', backgroundColor: 'transparent', color: 'white' }}>Cancel</button>
        </div>
      </div>
    </div>
  );
};

export default AddMemberModal;
