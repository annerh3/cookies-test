import { api } from "../config/api";
export const loginAsync = async (form) => {
    try {
        const { data } = await api.post('/auth/login', form, {withCredentials: true});
        return data;
    }
    catch (error) {
        console.error(error);
        return error?.response?.data;
    }
}

export const validateTokenAsync = async () => {
    try {
        const { data } = await api.get('/auth/validate', {withCredentials: true});
        return data;
    }
    catch (error) {
        console.error(error);
        return error?.response?.data;
    }
}
