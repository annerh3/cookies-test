import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { getContactsAsync } from '../actions/contacts.actions';

export const useContacts = ({
  initialFormData = {},
  initialSearchTerm = '',
  initialPage = 1
} = {}) => {
  // Estados para controlar la paginación y filtros
  const [page, setPage] = useState(initialPage);
  const [searchTerm, setSearchTerm] = useState(initialSearchTerm);
  const [formData, setFormData] = useState(initialFormData);

  // Query principal usando TanStack Query
  const {
    data,
    isLoading,
    isError,
    error,
    refetch,
    isFetching
  } = useQuery({
    queryKey: ['contacts', page, searchTerm, formData],
    queryFn: () => getContactsAsync(searchTerm, page),
    keepPreviousData: true, // Mantiene los datos anteriores mientras carga los nuevos
    staleTime: 5 * 60 * 1000, // 5 minutos de cache
    refetchOnWindowFocus: false,
  });

  // Datos para trabajar con el paginador
  const paginationData = {
    currentPage: data?.data?.currentPage || initialPage,
    totalPages: data?.data?.totalPages || 0,
    hasNextPage: data?.data?.hasNextPage || false,
    hasPreviousPage: data?.data?.hasPreviousPage || false,
    pageSize: data?.data?.pageSize || 0,
    totalItems: data?.data?.totalItems || 0,
  };

  // Funciones para controlar la paginación
  const handleNextPage = () => {
    if (paginationData.hasNextPage) {
      setPage(prev => prev + 1);
    }
  };

  const handlePreviousPage = () => {
    if (paginationData.hasPreviousPage) {
      setPage(prev => prev - 1);
    }
  };

  const handleCurrentPage = (pageNumber) => {
    setPage(pageNumber);
  };

  // Función para actualizar los filtros y reiniciar la paginación
  const updateFilters = (newFormData, newSearchTerm = searchTerm) => {
    setFormData(newFormData);
    setSearchTerm(newSearchTerm);
    setPage(1); // Reinicia a la primera página cuando cambian los filtros
  };

  // Función para actualizar solo el término de búsqueda
  const updateSearchTerm = (newSearchTerm) => {
    setSearchTerm(newSearchTerm);
    setPage(1); // Reinicia a la primera página cuando cambia la búsqueda
  };

  return {
    // Datos
    contacts: data?.data?.items || [],
    paginationData,
    isLoading,
    isFetching,
    isError,
    error,
    status: data?.status || false,
    message: data?.message || '',
    
    // Funciones para componente de paginación
    handleNextPage,
    handlePreviousPage,
    handleCurrentPage,
    
    // Funciones para filtros
    updateFilters,
    updateSearchTerm,
    
    // Estado actual
    currentPage: page,
    currentSearchTerm: searchTerm,
    currentFormData: formData,
    
    // Función para forzar recarga
    refetch
  };
};