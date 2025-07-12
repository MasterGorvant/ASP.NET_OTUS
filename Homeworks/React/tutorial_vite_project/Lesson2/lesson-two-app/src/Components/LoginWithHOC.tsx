import { useDispatch, useSelector } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import { loginStart, loginSuccess, loginFailure } from '../store/authSlice';
import { type RootState, type AppDispatch } from '../store/store';
import { type LoginFormData } from '../types/auth';
import withAuthForm from '../hocs/withAuthForm';

interface LoginProps {
  formData: LoginFormData;
  handleChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  handleSubmit: (e: React.FormEvent) => void;
}

const LoginForm: React.FC<LoginProps> = ({ formData, handleChange, handleSubmit }) => {
  const { loading, error } = useSelector((state: RootState) => state.auth);

  return (
    <div>
      <h1>Login</h1>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <form onSubmit={handleSubmit}>
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
          {loading ? 'Loading...' : 'Login'}
        </button>
      </form>
    </div>
  );
};

const loginAction = async (formData: LoginFormData, navigate: ReturnType<typeof useNavigate>, dispatch: AppDispatch) => {
  dispatch(loginStart());
    
  try {
    // Здесь должна быть логика авторизации
    const mockUser = { name: 'Test User', email: formData.email };
    dispatch(loginSuccess(mockUser));
    navigate('/');
  } catch (err) {
    dispatch(loginFailure(err instanceof Error ? err.message : 'An error occurred'));
  }
};

export default withAuthForm(
  LoginForm,
  (formData, navigate) => {
    const dispatch = useDispatch<AppDispatch>();
    return loginAction(formData, navigate, dispatch);
  }
);