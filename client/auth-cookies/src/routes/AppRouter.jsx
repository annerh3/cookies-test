import { useEffect, useState } from "react";
import LoginPage from "../pages/LoginPage";
import { useAuthStore } from "../store/useAuthStore";
import { ProtectedLayout } from "../components/ProtectedLayout";
import { Navigate, Route, Routes } from "react-router";
import ContactsPage from "../pages/ContactsPage";
import { Spinner } from "../components/Spinner";

export const AppRouter = () => {
  const [isLoading, setLoading] = useState(true);
  const { validateAuthentication } = useAuthStore();
  
  useEffect(() => {
    const initApp = async () => {
      console.log('Setting global state');
      await validateAuthentication();
      setLoading(false);
    };
    setLoading(false);
    
    // initApp();
    console.log('ENV-V ', import.meta.env.VITE_API_URL);

    
  }, [validateAuthentication]);

  
  if (isLoading) return <Spinner />;

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-midnight">
      <div className="w-3/4 flex-grow py-5">
        <div className="flex justify-between">
          <Routes>
            {/* Rutas públicas */}
            <Route path="/" element={<LoginPage />} />

            {/* Rutas protegidas */}
            <Route element={<ProtectedLayout />}>
              <Route path="/contacts" element={<ContactsPage />} />
            </Route>

            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </div>
      </div>
    </div>
  );
};
