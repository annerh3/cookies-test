/**
 * Gestiona el estado de autenticación del usuario y almacena los tokens
 * necesarios en el almacenamiento local
 * Establece el token y el estado del usuario basado en el estado y 
 * los datos proporcionados.
 * 
 * @param {boolean} status - El estado que indica éxito o fracaso.
 * @param {Object} data - Los datos que contienen la información del usuario y los tokens.
 * 
 * 
 * @param {string} data.name - El nombre del usuario.
 * @param {string} data.email - El correo electrónico del usuario.
 * @param {string} data.tokenExpiration - El tiempo de expiración del token.
 * @param {string} data.token - El token de autenticación.
 * @param {string} data.refreshToken - El token de actualización.
 * 
 * @param {string} message - El mensaje que se establecerá en el estado.
 * @param {Function} set - La función para actualizar el estado.
 * @param {Function} get - La función para recuperar el estado actual.
 * @returns {Object} Un objeto que contiene el estado de error y el mensaje.
 */
export function setTokenAndState(status, data, message, set, get ) {
    if (status) {
        // Si todo sale bien devuelve un objeto con error en falso y el mensaje
        // Actualiza el registro del usuario en el estado y en el almacenamiento local
        set(
            {
                error: false,
                user: {
                    name: data.fullName,
                    email: data.email,
                    tokenExpiration: data.tokenExpiration,
                },
                token: data.token,
                refreshToken: data.refreshToken,
                isAuthenticated: true,
                message: message
            }
        );
        localStorage.setItem('user', JSON.stringify(get().user ?? {}))
        return ({ error: false, message });
    }
    // Si algo sale mal devuelve un objeto con error en verdadero y el mensaje
    set({ message: message, error: true });
    return ({ error: true, message });
  }