import { useSelector } from 'react-redux';
import { Navigate } from 'react-router-dom';
import { type RootState } from '../store/store';

interface PrivateRouteProps {
  children: React.ReactElement;
}

const PrivateRoute: React.FC<PrivateRouteProps> = ({ children }) => {
  const { isAuthenticated } = useSelector((state: RootState) => state.auth);
  
  return isAuthenticated ? children : <Navigate to="/login" />;
};

export default PrivateRoute;