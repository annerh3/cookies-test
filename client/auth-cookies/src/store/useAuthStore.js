import { create } from 'zustand';
// import { jwtDecode } from "jwt-decode";
import { loginAsync, validateTokenAsync } from '../actions/auth.actions';
import { setTokenAndState } from '../utilities/set_token_and_state.js';

export const useAuthStore = create((set, get) => ({ // set (para actualizar el estado) y get (para obtener el estado actual).
    // Propiedades de estado iniciales
    user: null,
    token: null,
    roles: [],
    refreshToken: null,
    isAuthenticated: false,
    message: '',
    error: false,
    // Se restablezca a su valor inicial cada vez que se recarga la página.



    // Método de login para autenticar al usuario
    login: async (form) => {
        // try {
        set({ message: '', error: false });
        const { status, data, message } = await loginAsync(form);
        // console.log(status)
        return setTokenAndState(status, data, message, set, get);
        // } catch (error) {
        //     console.error('Error during login:', error);
        //     set({ error: true });
        // }
    },

    // register: async (form) => {
    //     // try {
    //     set({ message: '', error: false });
    //     const { status, data, message } = await registerAsync(form);
    //     return setTokenAndState(status, data, message, set, get);
    //     // } catch (error) {
    //     //     console.error('Error during registration:', error);
    //     //     set({ error: true });
    //     // }
    // },

    setSession: (user, token, refreshToken) => {
        set(
            {
                user: user,
                token: token,
                refreshToken: refreshToken,
                isAuthenticated: true
            }
        );

        localStorage.setItem('user', JSON.stringify(get().user ?? {}))
    },

    // Método de logout para cerrar sesión
    logout: () => {

        set({
            user: null,
            token: null,
            roles: [],
            refreshToken: null,
            isAuthenticated: false,
            error: false,
            message: ''
        })

        const clearLocaleStorage = () => {
            const keys = [
               "user",
            ];
            keys.forEach(key => localStorage.removeItem(key));
    
        };
        clearLocaleStorage()

    },

    // validateAuthentication: () => {
    //     const token = localStorage.getItem('token');
    //     const user = localStorage.getItem('user');
    //     if (!token) {
    //         set({ isAuthenticated: false });
    //         return;
    //     }

    //     try {
    //         const decodeJwt = jwtDecode(token);
    //         const currentTime = Math.floor(Date.now() / 1000); // convertir fecha y hora actual en segundos
    //         if (decodeJwt.exp < currentTime) {
    //             console.log('Token Expirado');
    //             set({ isAuthenticated: false });
    //             return;
    //         }

    //         const roles = decodeJwt["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

    //         set(
    //             {
    //                 isAuthenticated: true,
    //                 roles: typeof (roles) === 'string' ? [roles] : roles,
    //                 user: typeof (user) === "string" ? JSON.parse(user) : user
    //             });

    //         console.log("(useAuthStore) isAuthenticated: ", get().isAuthenticated);

    //     } catch (error) {
    //         console.error(error);
    //         set({ isAuthenticated: false })
    //     }
    // },

    validateAuthentication: async () => {
        // Limpiar errores previos
        set({ error: false, message: '' });
      
        try {
          const result = await validateTokenAsync();
          console.log('Resultado de la validación del token:', result);
          
      
          if (result?.status) { // STATUS es un booleano
            // El token es válido
            set({ isAuthenticated: true });
          } else {
            // Token inválido o expirado
            set({ isAuthenticated: false });
          }
        } catch (error) {
          console.error('Error al validar token:', error);
          set({ isAuthenticated: false, error: true, message: 'Error al validar autenticación' });
        }
      }


}));


