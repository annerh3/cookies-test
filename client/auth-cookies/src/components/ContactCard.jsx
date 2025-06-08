import { FaLocationCrosshairs, FaSquarePhone, FaTreeCity } from 'react-icons/fa6';
import { MdOutlineMailOutline } from 'react-icons/md';

export const ContactCard = ({ contact }) => {
  const handleClick = () => {
    window.open(`https://www.google.com/maps/search/?api=1&query=${contact.latitude},${contact.longitude}`, '_blank');
  };

  return (
    <div className="bg-charcoal rounded-lg shadow-md p-4 mb-4 ">
      <h2 className="text-xl font-semibold text-white">{contact.firstName} {contact.lastName}</h2>
      <div className="mt-2 space-y-1">
        <p className="text-white flex items-center">
            <span className='mr-2'><MdOutlineMailOutline /></span>
          <span className='text-sm md:text-lg truncate'>{contact.email}</span>
        </p>
        <p className="text-white flex items-center">
          <span className="mr-2"><FaSquarePhone /></span>
          <span>{contact.telephone}</span>
        </p>
        <p className="text-white flex items-center">
          <span className="mr-2"><FaTreeCity /></span>
          <span>{contact.city}, {contact.country}</span>
        </p>
        <p className="text-white/50 text-sm flex items-center hover:underline cursor-pointer" title='Ver en Maps' onClick={handleClick}>
        <FaLocationCrosshairs className='mr-2' />
          {contact.latitude}, {contact.longitude}
        </p>
      </div>
    </div>
  );
};