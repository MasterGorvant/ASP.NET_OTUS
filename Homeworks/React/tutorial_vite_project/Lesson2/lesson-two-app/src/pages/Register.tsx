import { useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { registerStart, registerSuccess, registerFailure } from '../store/authSlice';
import store from '../store/store';
import { type RootState, type AppDispatch } from '../store/store';
import { type RegisterFormData } from '../types/auth';

const Register: React.FC = () => {
  const [formData, setFormData] = useState<RegisterFormData>({
    name: '',
    email: '',
    password: ''
  });
  
  const dispatch: AppDispatch = useDispatch();
  const { loading, error } = useSelector((state: RootState) => state.auth);
  const navigate = useNavigate();

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    dispatch(registerStart());
    
    try {
      // Здесь должна быть логика регистрации
      const mockUser = { name: formData.name, email: formData.email };
      dispatch(registerSuccess(mockUser));
      navigate('/');
    } catch (err) {
      dispatch(registerFailure(err instanceof Error ? err.message : 'An error occurred'));
    }
  };

  return (
    <div>
      <h1>Register</h1>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <form onSubmit={handleSubmit}>
        <div>
          <label>Name:</label>
          <input 
            type="text" 
            name="name"
            value={formData.name} 
            onChange={handleChange} 
            required 
          />
        </div>
        <div>
          <label>Email:</label>
          <input 
            type="email" 
            name="email"
            value={formData.email} 
            onChange={handleChange} 
            required 
          />
        </div>
        <div>
          <label>Password:</label>
          <input 
            type="password" 
            name="password"
            value={formData.password} 
            onChange={handleChange} 
            required 
          />
        </div>
        <button type="submit" disabled={loading}>
          {loading ? 'Loading...' : 'Register'}
        </button>
      </form>
    </div>
  );
};

export default Register;