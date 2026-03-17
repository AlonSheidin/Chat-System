import { useEffect, useState, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import { API_BASE_URL } from '../api/axiosInstance';

export interface MessageResponse {
  id: string;
  chatId: string;
  senderId: string;
  senderName: string;
  content: string;
  sentAt: string;
}

export const useSignalR = (token: string | null) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [onlineUsers, setOnlineUsers] = useState<Set<string>>(new Set());
  const [typingUsers, setTypingUsers] = useState<{ [chatId: string]: string[] }>({});
  const [messages, setMessages] = useState<MessageResponse[]>([]);

  useEffect(() => {
    if (!token) return;

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/ws`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);
  }, [token]);

  useEffect(() => {
    if (!connection) return;

    const startConnection = async () => {
      try {
        await connection.start();
        console.log('SignalR Connected');

        connection.on('ReceiveMessage', (message: MessageResponse) => {
          setMessages((prev) => [...prev, message]);
        });

        connection.on('UserOnline', (userId: string) => {
          setOnlineUsers((prev) => new Set([...prev, userId]));
        });

        connection.on('UserOffline', (userId: string) => {
          setOnlineUsers((prev) => {
            const next = new Set(prev);
            next.delete(userId);
            return next;
          });
        });

        connection.on('InitialOnlineUsers', (userIds: string[]) => {
          setOnlineUsers(new Set(userIds));
        });

        connection.on('UserTyping', (data: { chatId: string; userId: string; username: string }) => {
          setTypingUsers((prev) => {
            const currentChatTyping = prev[data.chatId] || [];
            if (currentChatTyping.includes(data.username)) return prev;
            
            const next = { ...prev, [data.chatId]: [...currentChatTyping, data.username] };
            
            // Remove after 3 seconds
            setTimeout(() => {
              setTypingUsers((p) => {
                const updated = (p[data.chatId] || []).filter((u) => u !== data.username);
                return { ...p, [data.chatId]: updated };
              });
            }, 3000);

            return next;
          });
        });
      } catch (err) {
        console.error('SignalR Connection Error: ', err);
      }
    };

    startConnection();

    return () => {
      connection.stop();
    };
  }, [connection]);

  const joinChat = useCallback(async (chatId: string) => {
    if (connection) {
      await connection.invoke('JoinChat', chatId);
    }
  }, [connection]);

  const sendMessage = useCallback(async (chatId: string, message: string) => {
    if (connection) {
      await connection.invoke('SendMessage', chatId, message);
    }
  }, [connection]);

  const sendTyping = useCallback(async (chatId: string) => {
    if (connection) {
      await connection.invoke('SendTyping', chatId);
    }
  }, [connection]);

  return {
    onlineUsers,
    typingUsers,
    messages,
    setMessages,
    joinChat,
    sendMessage,
    sendTyping,
    isConnected: connection?.state === signalR.HubConnectionState.Connected,
  };
};
