import { ContactCard } from "../components";
import { useContacts } from "../hooks/useContacts";
import { Pagination } from "../components/Pagination";
import { TiContacts } from "react-icons/ti";
import { Spinner } from "../components/Spinner";

export default function ContactsPage() {
  const {
    contacts,
    paginationData,
    isLoading,
    handleNextPage,
    handlePreviousPage,
    handleCurrentPage,
    currentPage,
  } = useContacts();

  if (isLoading) return <Spinner />;    


  return (
    <div className="w-full">
      <div className="max-w-4xl mx-auto">
        <header className="text-2xl text-white py-3 font-bold flex gap-1 items-center">
          <TiContacts size={30} />
          Contactos
        </header>
        
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {contacts.map((contacto) => (
              <ContactCard key={contacto.id} contact={contacto} />
            ))}
          </div>
        

        {contacts.length > 0 && paginationData.totalPages > 1 && (
          <div className="mt-8 flex justify-center">
            <Pagination
              totalPages={paginationData.totalPages}
              handlePreviousPage={handlePreviousPage}
              hasPreviousPage={paginationData.hasPreviousPage}
              handleNextPage={handleNextPage}
              hasNextPage={paginationData.hasNextPage}
              handleCurrentPage={handleCurrentPage}
              currentPage={currentPage}
            />
          </div>
        )}
      </div>
    </div>
  );
}
