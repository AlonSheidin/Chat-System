import React, { useState, useEffect, useRef } from 'react';
import { useAuth } from '../context/AuthContext';
import { useChat } from '../context/ChatContext';
import { 
  MessageSquare, Users, Settings, Search, Send, Plus, 
  Phone, Video, MoreVertical, Paperclip, Smile, LogOut
} from 'lucide-react';
import { format } from 'date-fns';

const ChatPage: React.FC = () => {
  const { user, logout } = useAuth();
  const { 
    chats, activeChat, setActiveChat, messages, sendMessage, 
    sendTyping, typingUsers, onlineUsers 
  } = useChat();

  const [messageInput, setMessageInput] = useState('');
  const [searchQuery, setSearchQuery] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  };

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const handleSend = (e: React.FormEvent) => {
    e.preventDefault();
    if (messageInput.trim()) {
      sendMessage(messageInput);
      setMessageInput('');
    }
  };

  const handleTyping = () => {
    sendTyping();
  };

  return (
    <div className="app-container">
      {/* Sidebar 1: Discord-style Nav */}
      <div className="sidebar-nav">
        <div style={{ width: '48px', height: '48px', backgroundColor: '#5865f2', borderRadius: '16px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          <MessageSquare color="white" size={24} />
        </div>
        <div style={{ width: '32px', height: '2px', backgroundColor: 'rgba(255,255,255,0.1)', margin: '4px 0' }} />
        <div className="nav-icon" style={{ cursor: 'pointer', opacity: 0.7 }}><Users size={24} /></div>
        <div style={{ flex: 1 }} />
        <div className="nav-icon" style={{ cursor: 'pointer', opacity: 0.7 }}><Settings size={24} /></div>
        <div className="nav-icon" style={{ cursor: 'pointer', opacity: 0.7, color: '#f04747' }} onClick={logout}><LogOut size={24} /></div>
      </div>

      {/* Sidebar 2: Chat List */}
      <div className="sidebar-chats">
        <div style={{ padding: '16px', borderBottom: '1px solid rgba(0,0,0,0.2)' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
            <h2 style={{ fontSize: '18px' }}>Messages</h2>
            <Plus size={20} style={{ cursor: 'pointer' }} />
          </div>
          <div style={{ backgroundColor: '#202225', display: 'flex', alignItems: 'center', padding: '8px', borderRadius: '4px' }}>
            <Search size={16} color="#b9bbbe" style={{ marginRight: '8px' }} />
            <input 
              placeholder="Search conversations" 
              style={{ background: 'none', border: 'none', color: 'white', fontSize: '14px', width: '100%' }}
              value={searchQuery}
              onChange={e => setSearchQuery(e.target.value)}
            />
          </div>
        </div>

        <div style={{ flex: 1, overflowY: 'auto' }}>
          {chats.map(chat => (
            <div 
              key={chat.id} 
              onClick={() => setActiveChat(chat)}
              style={{
                padding: '12px 16px',
                display: 'flex',
                alignItems: 'center',
                cursor: 'pointer',
                backgroundColor: activeChat?.id === chat.id ? 'rgba(255,255,255,0.05)' : 'transparent'
              }}
            >
              <div style={{ position: 'relative', marginRight: '12px' }}>
                <div style={{ width: '48px', height: '48px', backgroundColor: '#4f545c', borderRadius: '50%' }} />
                {chat.memberIds.some(id => id !== user?.id && onlineUsers.has(id)) && (
                  <div style={{ 
                    position: 'absolute', bottom: 0, right: 0, width: '14px', height: '14px', 
                    backgroundColor: '#43b581', borderRadius: '50%', border: '3px solid #2f3136' 
                  }} />
                )}
              </div>
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px' }}>
                  <span style={{ fontWeight: 'bold', fontSize: '14px', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                    {chat.name || 'Private Chat'}
                  </span>
                  <span style={{ fontSize: '12px', color: '#b9bbbe' }}>12:45 PM</span>
                </div>
                <div style={{ fontSize: '13px', color: '#b9bbbe', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                  {typingUsers.length > 0 && activeChat?.id === chat.id ? (
                    <span style={{ color: '#43b581' }}>Typing...</span>
                  ) : 'Last message snippet...'}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Main Chat Area */}
      <div className="main-chat">
        {activeChat ? (
          <>
            {/* Header */}
            <div style={{ 
              height: '64px', backgroundColor: 'var(--bg-dark-4)', padding: '0 16px',
              display: 'flex', alignItems: 'center', justifyContent: 'space-between',
              borderBottom: '1px solid rgba(0,0,0,0.2)'
            }}>
              <div style={{ display: 'flex', alignItems: 'center' }}>
                <div style={{ width: '40px', height: '40px', backgroundColor: '#4f545c', borderRadius: '50%', marginRight: '12px' }} />
                <div>
                  <div style={{ fontWeight: 'bold' }}>{activeChat.name || 'Chat'}</div>
                  <div style={{ fontSize: '12px', color: '#43b581' }}>Online</div>
                </div>
              </div>
              <div style={{ display: 'flex', gap: '20px', color: '#b9bbbe' }}>
                <Phone size={20} style={{ cursor: 'pointer' }} />
                <Video size={20} style={{ cursor: 'pointer' }} />
                <Search size={20} style={{ cursor: 'pointer' }} />
                <MoreVertical size={20} style={{ cursor: 'pointer' }} />
              </div>
            </div>

            {/* Messages */}
            <div style={{ flex: 1, overflowY: 'auto', padding: '20px' }}>
              <div style={{ display: 'flex', justifyContent: 'center', marginBottom: '20px' }}>
                <span style={{ backgroundColor: 'rgba(0,0,0,0.2)', padding: '4px 12px', borderRadius: '4px', fontSize: '12px', color: '#b9bbbe' }}>TODAY</span>
              </div>

              {messages.map((msg, index) => {
                const isMe = msg.senderId === user?.id;
                return (
                  <div key={msg.id || index} style={{ 
                    display: 'flex', 
                    justifyContent: isMe ? 'flex-end' : 'flex-start',
                    marginBottom: '8px'
                  }}>
                    {!isMe && <div style={{ width: '32px', height: '32px', backgroundColor: '#4f545c', borderRadius: '50%', marginRight: '8px', marginTop: 'auto' }} />}
                    <div style={{ 
                      maxWidth: '60%', 
                      padding: '8px 12px', 
                      borderRadius: '8px',
                      backgroundColor: isMe ? 'var(--whatsapp-bubble-out)' : 'var(--whatsapp-bubble-in)',
                      position: 'relative'
                    }}>
                      {!isMe && <div style={{ fontSize: '12px', fontWeight: 'bold', color: '#00aff4', marginBottom: '4px' }}>{msg.senderName}</div>}
                      <div style={{ fontSize: '14px', lineHeight: '1.4' }}>{msg.content}</div>
                      <div style={{ fontSize: '10px', color: 'rgba(255,255,255,0.5)', textAlign: 'right', marginTop: '4px' }}>
                        {format(new Date(msg.sentAt), 'HH:mm')}
                      </div>
                    </div>
                  </div>
                );
              })}
              {typingUsers.length > 0 && (
                <div style={{ fontSize: '12px', color: '#b9bbbe', marginTop: '8px' }}>
                  {typingUsers.join(', ')} is typing...
                </div>
              )}
              <div ref={messagesEndRef} />
            </div>

            {/* Input Area */}
            <div style={{ padding: '16px', backgroundColor: 'var(--bg-dark-3)' }}>
              <form onSubmit={handleSend} style={{ 
                backgroundColor: '#40444b', padding: '10px 16px', borderRadius: '8px',
                display: 'flex', alignItems: 'center'
              }}>
                <Paperclip size={20} color="#b9bbbe" style={{ marginRight: '16px', cursor: 'pointer' }} />
                <Smile size={20} color="#b9bbbe" style={{ marginRight: '16px', cursor: 'pointer' }} />
                <input 
                  value={messageInput}
                  onChange={e => { setMessageInput(e.target.value); handleTyping(); }}
                  placeholder="Type a message..." 
                  style={{ flex: 1, background: 'none', border: 'none', color: 'white', fontSize: '15px' }}
                />
                <button type="submit" style={{ background: 'none', display: 'flex', alignItems: 'center' }}>
                  <Send size={20} color={messageInput.trim() ? '#00aff4' : '#b9bbbe'} />
                </button>
              </form>
              <div style={{ textAlign: 'center', fontSize: '10px', color: '#b9bbbe', marginTop: '8px' }}>
                Shift + Enter for new line
              </div>
            </div>
          </>
        ) : (
          <div style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', color: '#b9bbbe' }}>
            <MessageSquare size={64} style={{ marginBottom: '16px', opacity: 0.2 }} />
            <h3>Select a conversation to start chatting</h3>
          </div>
        )}
      </div>

      {/* Sidebar 3: Profile Info (Right) */}
      {activeChat && (
        <div className="sidebar-profile" style={{ padding: '32px 16px', display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
          <div style={{ position: 'relative', marginBottom: '16px' }}>
            <div style={{ width: '120px', height: '120px', backgroundColor: '#4f545c', borderRadius: '50%' }} />
            <div style={{ 
              position: 'absolute', bottom: '8px', right: '8px', width: '24px', height: '24px', 
              backgroundColor: '#43b581', borderRadius: '50%', border: '4px solid #2f3136' 
            }} />
          </div>
          <h2 style={{ fontSize: '20px', marginBottom: '4px' }}>{activeChat.name || 'Chat Profile'}</h2>
          <div style={{ color: '#b9bbbe', fontSize: '14px', marginBottom: '24px' }}>Senior UI Designer</div>
          
          <div style={{ display: 'flex', gap: '8px', width: '100%', marginBottom: '32px' }}>
            <button style={{ flex: 1, padding: '8px', backgroundColor: 'rgba(255,255,255,0.05)', borderRadius: '4px', color: 'white' }}>Profile</button>
            <button style={{ flex: 1, padding: '8px', backgroundColor: 'rgba(255,255,255,0.05)', borderRadius: '4px', color: 'white' }}>Mute</button>
          </div>

          <div style={{ width: '100%', marginBottom: '24px' }}>
            <div style={{ fontSize: '12px', fontWeight: 'bold', color: '#b9bbbe', textTransform: 'uppercase', marginBottom: '12px' }}>Shared Media</div>
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '8px' }}>
              <div style={{ aspectRatio: '1', backgroundColor: '#4f545c', borderRadius: '4px' }} />
              <div style={{ aspectRatio: '1', backgroundColor: '#4f545c', borderRadius: '4px' }} />
              <div style={{ aspectRatio: '1', backgroundColor: 'rgba(0,0,0,0.2)', borderRadius: '4px', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '14px' }}>+12</div>
            </div>
          </div>

          <div style={{ width: '100%' }}>
            <div style={{ fontSize: '12px', fontWeight: 'bold', color: '#b9bbbe', textTransform: 'uppercase', marginBottom: '12px' }}>About</div>
            <div style={{ fontSize: '14px', color: '#b9bbbe', lineHeight: '1.5' }}>
              Building the future of communication tools. Focus on accessibility and clean aesthetics.
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ChatPage;
