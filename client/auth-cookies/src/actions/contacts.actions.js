import { api } from "../config/api";
export const getContactsAsync = async (searchTerm = "", page = 1) => {
    try {
        const { data } = await api.get(`/contacts?searchTerm=${searchTerm}&page=${page}`, {withCredentials: true});
        return data;
    }
    catch (error) {
        console.error(error);
        return error?.response?.data;
    }
}
