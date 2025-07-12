import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

type AuthAction<T> = (formData: T, navigate: ReturnType<typeof useNavigate>) => Promise<void>;

const withAuthForm = <T extends {}>(
  WrappedComponent: React.ComponentType<{
    formData: T;
    handleChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
    handleSubmit: (e: React.FormEvent) => void;
  }>,
  authAction: AuthAction<T>
) => {
  return (props: Omit<React.ComponentProps<typeof WrappedComponent>, 'formData' | 'handleChange' | 'handleSubmit'>) => {
    const [formData, setFormData] = useState<T>({} as T);
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
      await authAction(formData, navigate);
    };

    return (
      <WrappedComponent 
        {...props}
        formData={formData}
        handleChange={handleChange}
        handleSubmit={handleSubmit}
      />
    );
  };
};

export default withAuthForm;