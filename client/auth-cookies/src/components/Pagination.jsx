import { generateid } from "../utilities/generateid";

export const Pagination = ({
  totalPages,
  handlePreviousPage = () => {},
  hasPreviousPage,
  handleNextPage = () => {},
  hasNextPage,
  handleCurrentPage,
  currentPage,
}) => {
  return (
    <div className="flex">
      <button
        onClick={handlePreviousPage}
        disabled={!hasPreviousPage}
        className={`px-3 py-2 mx-1 font-medium bg-coal text-white rounded-md ${
          !hasPreviousPage
            ? "cursor-not-allowed"
            : "hover:bg-purple-600 hover:text-white"
        }`}
      >
        Anterior
      </button>

      {[...Array(totalPages)].map((value, index) => (
        <button
          key={generateid()}
          onClick={() => handleCurrentPage(index + 1)}
          className={`px-3 py-2 mx-1 font-medium rounded-md text-white ${
            currentPage === index + 1
              ? "bg-purple-600 text-white"
              : "hover:bg-coal hover:text-white"
          }`}
        >
          {index + 1}
        </button>
      ))}

      <button
        onClick={handleNextPage}
        disabled={!hasNextPage}
        className={`px-3 py-2 mx-1 font-medium bg-coal text-white rounded-md ${
          !hasNextPage
            ? "cursor-not-allowed"
            : "hover:bg-purple-600 hover:text-white"
        }`}
      >
        Siguiente
      </button>
    </div>
  );
};
