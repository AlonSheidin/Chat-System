import React, { useState } from 'react';
import axiosInstance from '../api/axiosInstance';
import { useAuth } from '../context/AuthContext';

const AuthPage: React.FC = () => {
  const [isLogin, setIsLogin] = useState(true);
  const [email, setEmail] = useState('');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const { login } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      const endpoint = isLogin ? '/auth/login' : '/auth/register';
      const data = isLogin ? { email, password } : { username, email, password };
      const response = await axiosInstance.post(endpoint, data);
      login(response.data, response.data.token);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Authentication failed');
    }
  };

  return (
    <div style={{
      display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh',
      backgroundColor: '#202225'
    }}>
      <form onSubmit={handleSubmit} style={{
        backgroundColor: '#36393f', padding: '32px', borderRadius: '8px', width: '400px',
        boxShadow: '0 2px 10px rgba(0,0,0,0.2)'
      }}>
        <h2 style={{ marginBottom: '24px', textAlign: 'center' }}>
          {isLogin ? 'Welcome back!' : 'Create an account'}
        </h2>
        
        {error && <div style={{ color: '#f04747', marginBottom: '16px', fontSize: '14px' }}>{error}</div>}

        <div style={{ marginBottom: '16px' }}>
          <label style={{ display: 'block', marginBottom: '8px', color: '#b9bbbe', textTransform: 'uppercase', fontSize: '12px', fontWeight: 'bold' }}>Email</label>
          <input type="email" value={email} onChange={e => setEmail(e.target.value)} required style={{
            width: '100%', padding: '10px', backgroundColor: '#202225', border: 'none', color: 'white', borderRadius: '3px'
          }} />
        </div>

        {!isLogin && (
          <div style={{ marginBottom: '16px' }}>
            <label style={{ display: 'block', marginBottom: '8px', color: '#b9bbbe', textTransform: 'uppercase', fontSize: '12px', fontWeight: 'bold' }}>Username</label>
            <input type="text" value={username} onChange={e => setUsername(e.target.value)} required style={{
              width: '100%', padding: '10px', backgroundColor: '#202225', border: 'none', color: 'white', borderRadius: '3px'
            }} />
          </div>
        )}

        <div style={{ marginBottom: '24px' }}>
          <label style={{ display: 'block', marginBottom: '8px', color: '#b9bbbe', textTransform: 'uppercase', fontSize: '12px', fontWeight: 'bold' }}>Password</label>
          <input type="password" value={password} onChange={e => setPassword(e.target.value)} required style={{
            width: '100%', padding: '10px', backgroundColor: '#202225', border: 'none', color: 'white', borderRadius: '3px'
          }} />
        </div>

        <button type="submit" style={{
          width: '100%', padding: '12px', backgroundColor: '#5865f2', color: 'white', border: 'none',
          borderRadius: '3px', fontWeight: 'bold', fontSize: '16px', marginBottom: '8px'
        }}>
          {isLogin ? 'Log In' : 'Continue'}
        </button>

        <div style={{ fontSize: '14px', color: '#b9bbbe' }}>
          {isLogin ? "Need an account?" : "Already have an account?"}
          <span onClick={() => setIsLogin(!isLogin)} style={{ color: '#00aff4', cursor: 'pointer', marginLeft: '5px' }}>
            {isLogin ? 'Register' : 'Login'}
          </span>
        </div>
      </form>
    </div>
  );
};

export default AuthPage;
