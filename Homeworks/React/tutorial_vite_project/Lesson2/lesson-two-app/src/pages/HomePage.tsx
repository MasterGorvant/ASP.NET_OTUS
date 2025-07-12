import { useSelector } from 'react-redux';
import store from '../store/store';
import { type RootState } from '../store/store';

const HomePage: React.FC = () => {
  const { user } = useSelector((state: RootState) => state.auth);
  
  return (
    <div>
      <h1>Welcome, {user?.name || 'User'}!</h1>
      <p>This is your dashboard.</p>
    </div>
  );
};

export default HomePage;