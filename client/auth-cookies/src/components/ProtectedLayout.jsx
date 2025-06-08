import { Navigate, Outlet } from "react-router";
// import { useState, useEffect } from "react";
import { useAuthStore } from "../store/useAuthStore";
import { Spinner } from "./Spinner";

export const ProtectedLayout = () => {
  const { isAuthenticated } = useAuthStore();
  // const [isValidating, setIsValidating] = useState(true);

  // useEffect(() => {
  //   const validate = async () => {
  //     await validateAuthentication();
  //     setIsValidating(false);
  //   };
    
  //   validate();
  // }, [validateAuthentication]);

  // if (isValidating) return <Spinner />

  // Una vez completada la validación, decidir si mostrar el contenido o redirigir
  if (!isAuthenticated) {
    return <Navigate to="/" />;
  }

  return (
    <div className="size-full">
      <Outlet />
    </div>
  );
};