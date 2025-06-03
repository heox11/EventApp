import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { PageHeader } from '../layout/PageHeader';

interface ParticipantData {
  eesnimi: string;
  perenimi: string;
  isikukood: string;
  maksuviis: string;
  lisainfo: string;
}

export function ParticipantInfo() {
  const navigate = useNavigate();
  const [participantData, setParticipantData] = useState<ParticipantData>({
    eesnimi: 'Test',
    perenimi: 'Kasutaja',
    isikukood: '1234567890',
    maksuviis: 'sularaha',
    lisainfo: 'See on testinfo.'
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setParticipantData(prevState => ({
      ...prevState,
      [name]: value
    }));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    navigate('/osalejad');
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <PageHeader title="Osavõtjad" />
      
      <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="bg-white shadow-md rounded-lg overflow-hidden">
          <div className="px-6 py-5">
            <form className="space-y-6" onSubmit={handleSubmit}>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="eesnimi">Eesnimi:</label>
                  <input 
                    type="text" 
                    id="eesnimi"
                    name="eesnimi"
                    value={participantData.eesnimi}
                    onChange={handleChange}
                    className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" 
                  />
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="perenimi">Perenimi:</label>
                  <input 
                    type="text" 
                    id="perenimi"
                    name="perenimi"
                    value={participantData.perenimi}
                    onChange={handleChange}
                    className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" 
                  />
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="isikukood">Isikukood:</label>
                  <input 
                    type="text" 
                    id="isikukood"
                    name="isikukood"
                    value={participantData.isikukood}
                    onChange={handleChange}
                    className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" 
                  />
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="maksuviis">Maksuviis:</label>
                  <select 
                    id="maksuviis"
                    name="maksuviis"
                    value={participantData.maksuviis}
                    onChange={handleChange}
                    className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                  >
                    <option value="sularaha">Sularaha</option>
                    <option value="pangaülekanne">Pangaülekanne</option>
                  </select>
                </div>
              </div>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="lisainfo">Lisainfo:</label>
                <textarea 
                  id="lisainfo"
                  name="lisainfo"
                  rows={4} 
                  value={participantData.lisainfo}
                  onChange={handleChange}
                  className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                />
              </div>
              
              <div className="flex justify-center space-x-4 pt-4">
                <button 
                  type="button" 
                  onClick={() => navigate(-1)}
                  className="px-6 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50"
                >
                  Tagasi
                </button>
                <button 
                  type="submit" 
                  className="px-6 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700"
                >
                  Salvesta
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
} 