import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { useAuth } from './AuthContext';
import { useSignalR } from '../hooks/useSignalR';
import type { MessageResponse } from '../hooks/useSignalR';
import axiosInstance from '../api/axiosInstance';

interface Chat {
  id: string;
  name: string;
  isGroup: boolean;
  memberIds: string[];
}

interface ChatContextType {
  chats: Chat[];
  activeChat: Chat | null;
  setActiveChat: (chat: Chat | null) => void;
  messages: MessageResponse[];
  onlineUsers: Set<string>;
  typingUsers: string[];
  sendMessage: (content: string) => Promise<void>;
  sendTyping: () => Promise<void>;
  createChat: (name: string, isGroup: boolean, memberIds: string[]) => Promise<void>;
  refreshChats: () => Promise<void>;
}

const ChatContext = createContext<ChatContextType | undefined>(undefined);

export const ChatProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { token } = useAuth();
  const { 
    onlineUsers, 
    typingUsers: allTypingUsers, 
    messages: signalRMessages, 
    setMessages,
    joinChat, 
    sendMessage: signalRSendMessage, 
    sendTyping: signalRSendTyping 
  } = useSignalR(token);

  const [chats, setChats] = useState<Chat[]>([]);
  const [activeChat, setActiveChat] = useState<Chat | null>(null);
  const [historyMessages, setHistoryMessages] = useState<MessageResponse[]>([]);

  const refreshChats = useCallback(async () => {
    if (!token) return;
    try {
      const response = await axiosInstance.get('/users/me/chats');
      setChats(response.data);
    } catch (err) {
      console.error('Failed to fetch chats', err);
    }
  }, [token]);

  useEffect(() => {
    refreshChats();
  }, [refreshChats]);

  useEffect(() => {
    if (activeChat) {
      const loadHistory = async () => {
        try {
          const response = await axiosInstance.get(`/chats/${activeChat.id}/messages`);
          // Backend returns newest first, we want oldest first for chat flow
          setHistoryMessages(response.data.reverse());
          setMessages([]); // Clear real-time buffer when switching chats
          await joinChat(activeChat.id);
        } catch (err) {
          console.error('Failed to load history', err);
        }
      };
      loadHistory();
    }
  }, [activeChat, joinChat, setMessages]);

  const sendMessage = async (content: string) => {
    if (activeChat) {
      await signalRSendMessage(activeChat.id, content);
    }
  };

  const sendTyping = async () => {
    if (activeChat) {
      await signalRSendTyping(activeChat.id);
    }
  };

  const createChat = async (name: string, isGroup: boolean, memberIds: string[]) => {
    try {
      await axiosInstance.post('/chats', { name, isGroup, memberIds });
      await refreshChats();
    } catch (err) {
      console.error('Failed to create chat', err);
    }
  };

  const currentChatMessages = [...historyMessages, ...signalRMessages.filter(m => m.chatId === activeChat?.id)];
  const currentChatTyping = activeChat ? (allTypingUsers[activeChat.id] || []) : [];

  return (
    <ChatContext.Provider value={{ 
      chats, 
      activeChat, 
      setActiveChat, 
      messages: currentChatMessages, 
      onlineUsers, 
      typingUsers: currentChatTyping,
      sendMessage,
      sendTyping,
      createChat,
      refreshChats
    }}>
      {children}
    </ChatContext.Provider>
  );
};

export const useChat = () => {
  const context = useContext(ChatContext);
  if (context === undefined) {
    throw new Error('useChat must be used within a ChatProvider');
  }
  return context;
};
