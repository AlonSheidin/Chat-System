import React from 'react';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ChatProvider } from './context/ChatContext';
import AuthPage from './pages/AuthPage';
import ChatPage from './pages/ChatPage';
import './styles/index.css';

const AppContent: React.FC = () => {
  const { isAuthenticated } = useAuth();

  return isAuthenticated ? (
    <ChatProvider>
      <ChatPage />
    </ChatProvider>
  ) : (
    <AuthPage />
  );
};

function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

export default App;
