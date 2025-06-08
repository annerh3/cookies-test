import axios from "axios";
import { useAuthStore } from "../store/useAuthStore";

const API_URL = import.meta.env.VITE_API_URL;

const api = axios.create({
    baseURL: `${API_URL}/api`,
    headers: { "Content-Type": "application/json"},
    withCredentials: true, // Para enviar cookies con las solicitudes
});

let refreshingTokenPromise = null;

api.interceptors.response.use(
    (response) => {return response},
    async (error) => {
        const originalRequest = error.config;
        
        // Si es error 401 (no autorizado) y no hemos intentado renovar ya
        if (
            error.response &&
            error.response.status === 401 &&
            !originalRequest._retry
        ) {
            console.log("Token expirado, intentando renovar...");
            
            // Marcamos esta solicitud para evitar bucles infinitos
            originalRequest._retry = true;
            
            if (!refreshingTokenPromise) {
                refreshingTokenPromise = api.post("auth/refresh-token")
                    .then((response) => {
                        console.log('Token renovado');                      
                        const setSession = useAuthStore.getState().setSession;
                        const user = {
                            email: response.data.data.email,
                            tokenExpiration: response.data.data.tokenExpiration,
                        };
                        setSession(
                            user,
                            response.data.data.token,
                            response.data.data.refreshToken
                        );         
                        return response.data.data.token;
                    })
                    .catch((err) => {
                        console.error("Error renovando token");
                        const logout = useAuthStore.getState().logout;
                        logout();
                        
                        // Importante: redirigimos pero también rechazamos la promesa
                        console.log("Redirigiendo a la página de login");
                        window.location.href = "/";
                        return Promise.reject(err);
                    })
                    .finally(() => {
                        // Siempre limpiamos la promesa, sin importar resultado
                        refreshingTokenPromise = null;
                    });
            }
            
            try {
                // Esperamos a que termine el proceso de refresh
                await refreshingTokenPromise;
                // Si llegamos aquí, el refresh fue exitoso, reintentamos
                return api(originalRequest);
            } catch (refreshError) {
                // Si el refresh falló, propagamos el error claramente
                console.log("No se pudo renovar el token, propagando error");
                return Promise.reject(refreshError);
            }
        }
        
        // Para cualquier otro error, simplemente lo propagamos
        return Promise.reject(error);
    }
);


export { api };