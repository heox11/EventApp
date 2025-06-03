import { useNavigate, useParams } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { PageHeader } from '../layout/PageHeader';

interface ParticipantData {
  type: 'individual' | 'company';
  eesnimi: string;
  perenimi: string;
  isikukood: string;
  ettevotte: string;
  registrikood: string;
  osavotjateArv: string;
  maksuviis: string;
  lisainfo: string;
}

export function EditParticipant() {
  const navigate = useNavigate();
  const { id } = useParams();

  const [participantData, setParticipantData] = useState<ParticipantData>({
    type: 'individual',
    eesnimi: '',
    perenimi: '',
    isikukood: '',
    ettevotte: '',
    registrikood: '',
    osavotjateArv: '',
    maksuviis: '',
    lisainfo: '',
  });

  useEffect(() => {
    if (id) {
      fetch(`http://localhost:5000/api/Participants/${id}`)
        .then(response => response.json())
        .then(data => {
          setParticipantData({
            type: data.type === 0 ? 'individual' : 'company',
            eesnimi: data.firstName || '',
            perenimi: data.lastName || '',
            isikukood: data.personalCode || '',
            ettevotte: data.companyName || '',
            registrikood: data.registrationCode || '',
            osavotjateArv: data.numberOfParticipants?.toString() || '',
            maksuviis: data.paymentMethod === 1 ? 'sularaha' : 'pangaülekanne',
            lisainfo: data.additionalInfo || '',
          });
        })
        .catch(error => {
          console.error('Error loading participant:', error);
        });
    }
  }, [id]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setParticipantData(prevState => ({
      ...prevState,
      [name]: value
    }));
  };

  const handleTypeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setParticipantData(prev => ({
      ...prev,
      type: e.target.value as 'individual' | 'company'
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      // Validate required fields based on type
      if (participantData.type === 'individual') {
        if (!participantData.eesnimi || !participantData.perenimi || !participantData.isikukood) {
          throw new Error('Eesnimi, perekonnanimi ja isikukood on kohustuslikud');
        }
      } else {
        if (!participantData.ettevotte || !participantData.registrikood || !participantData.osavotjateArv) {
          throw new Error('Ettevõtte nimi, registrikood ja osavõtjate arv on kohustuslikud');
        }
        if (participantData.registrikood.length !== 8 || !/^\d+$/.test(participantData.registrikood)) {
          throw new Error('Registrikood peab olema 8-kohaline number');
        }
      }

      // First, get the existing participant data to get the EventId and Type
      const getResponse = await fetch(`http://localhost:5000/api/Participants/${id}`);
      if (!getResponse.ok) {
        throw new Error('Osavõtja andmete laadimine ebaõnnestus');
      }
      const existingParticipant = await getResponse.json();
      console.log('Existing participant data:', existingParticipant);

      const updateData = {
        firstName: participantData.type === 'individual' ? participantData.eesnimi : '',
        lastName: participantData.type === 'individual' ? participantData.perenimi : '',
        personalCode: participantData.type === 'individual' ? participantData.isikukood : '',
        companyName: participantData.type === 'company' ? participantData.ettevotte : '',
        registrationCode: participantData.type === 'company' ? participantData.registrikood : '',
        numberOfParticipants: participantData.type === 'company' ? parseInt(participantData.osavotjateArv) : null,
        paymentMethod: participantData.maksuviis === 'sularaha' ? PaymentMethod.Cash : PaymentMethod.BankTransfer,
        additionalInfo: participantData.lisainfo || ''
      };
      console.log('Sending update data:', updateData);

      const response = await fetch(`http://localhost:5000/api/Participants/${id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(updateData),
      });

      console.log('Response status:', response.status);
      const responseText = await response.text();
      console.log('Response text:', responseText);

      if (!response.ok) {
        let errorMessage = 'Osavõtja andmete salvestamine ebaõnnestus';
        try {
          const errorData = JSON.parse(responseText);
          console.error('Server error response:', errorData);
          
          if (errorData.errors) {
            const validationErrors = Object.entries(errorData.errors)
              .map(([key, value]) => {
                if (Array.isArray(value)) {
                  return `${key}: ${value.join(', ')}`;
                }
                return `${key}: ${value}`;
              })
              .join('\n');
            errorMessage = validationErrors;
          } else if (errorData.message || errorData.title) {
            errorMessage = errorData.message || errorData.title;
          }
        } catch (e) {
          console.error('Error parsing error response:', e);
        }
        throw new Error(errorMessage);
      }

      // If we get here, the update was successful
      navigate(`/osalejad?id=${existingParticipant.eventId}`);
    } catch (error) {
      console.error('Error saving participant:', error);
    }
  };

  // Add enums to match the API
  enum ParticipantType {
    Individual = 0,
    Company = 1
  }

  enum PaymentMethod {
    BankTransfer = 0,
    Cash = 1
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <PageHeader title="Osavõtjad" />
      
      <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="bg-white shadow-md rounded-lg overflow-hidden">
          <div className="px-6 py-5">
            <form className="space-y-6" onSubmit={handleSubmit}>
              <div className="space-y-4">
                {participantData.type === 'individual' ? (
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
                  </div>
                ) : (
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="ettevotte">Ettevõtte nimi:</label>
                      <input 
                        type="text" 
                        id="ettevotte" 
                        name="ettevotte" 
                        value={participantData.ettevotte} 
                        onChange={handleChange} 
                        className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" 
                      />
                    </div>
                    
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="registrikood">Registrikood:</label>
                      <input 
                        type="text" 
                        id="registrikood" 
                        name="registrikood" 
                        value={participantData.registrikood} 
                        onChange={handleChange} 
                        className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" 
                      />
                    </div>
                    
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="osavotjateArv">Osavõtjate arv:</label>
                      <input 
                        type="number" 
                        id="osavotjateArv" 
                        name="osavotjateArv" 
                        value={participantData.osavotjateArv} 
                        onChange={handleChange} 
                        className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500" 
                      />
                    </div>
                  </div>
                )}
                
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="maksuviis">Maksuviis:</label>
                  <select 
                    id="maksuviis" 
                    name="maksuviis" 
                    value={participantData.maksuviis} 
                    onChange={handleChange} 
                    className="block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                  >
                    <option value="">Vali maksuviis</option>
                    <option value="sularaha">Sularaha</option>
                    <option value="pangaülekanne">Pangaülekanne</option>
                  </select>
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