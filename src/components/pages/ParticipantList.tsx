import { useNavigate, useSearchParams } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { PageHeader } from '../layout/PageHeader';

interface EventDetails {
  id: number;
  name: string;
  eventDate: string;
  location: string;
  additionalInfo: string;
  participantCount: number;
  isPastEvent: boolean;
}

interface Participant {
  id: number;
  type: number;
  firstName: string;
  lastName: string;
  personalCode: string;
  companyName: string;
  registrationCode: string;
  numberOfParticipants?: number;
  paymentMethod: string;
  additionalInfo?: string;
  displayName: string;
}

interface ParticipantFormData {
  firstName: string;
  lastName: string;
  personalCode: string;
  paymentMethod: string;
  additionalInfo: string;
  type: 'individual' | 'company';
  numberOfParticipants?: number;
  companyName?: string;
  registrationCode?: string;
}

export function ParticipantList() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const eventId = searchParams.get('id');
  const [event, setEvent] = useState<EventDetails | null>(null);
  const [participants, setParticipants] = useState<Participant[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [formData, setFormData] = useState<ParticipantFormData>({
    firstName: '',
    lastName: '',
    personalCode: '',
    paymentMethod: 'bank',
    additionalInfo: '',
    type: 'individual',
    companyName: '',
    registrationCode: ''
  });

  useEffect(() => {
    const fetchEventAndParticipants = async () => {
      if (!eventId) return;
      
      try {
        setLoading(true);
        setError(null);

        const eventResponse = await fetch(`http://localhost:5000/api/Events/${eventId}`);
        if (!eventResponse.ok) {
          throw new Error('Üritust ei leitud');
        }
        const eventData = await eventResponse.json();
        setEvent(eventData);

        const participantsResponse = await fetch(`http://localhost:5000/api/Participants/event/${eventId}`);
        if (!participantsResponse.ok) {
          throw new Error('Osavõtjate andmete laadimine ebaõnnestus');
        }
        const participantsData = await participantsResponse.json();

        const participantsArray = participantsData.$values || participantsData;
        if (!Array.isArray(participantsArray)) {
          console.error('Expected array but got:', participantsData);
          throw new Error('Invalid participants data format from server');
        }
        setParticipants(participantsArray);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Andmete laadimine ebaõnnestus');
      } finally {
        setLoading(false);
      }
    };

    fetchEventAndParticipants();
  }, [eventId]);

  const handleViewParticipant = (participantId: number) => {
    navigate(`/osavotja-info/${participantId}`);
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleTypeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData(prev => ({
      ...prev,
      type: e.target.value as 'individual' | 'company'
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const participantData = {
        eventId: parseInt(eventId || '0'),
        type: formData.type === 'individual' ? 0 : 1,

        firstName: formData.type === 'individual' ? formData.firstName : '',
        lastName: formData.type === 'individual' ? formData.lastName : '',
        personalCode: formData.type === 'individual' ? formData.personalCode : '',

        name: formData.type === 'company' ? formData.companyName : '',
        reigstrationCode: formData.type === 'company' ? formData.registrationCode : '',
        participantsnR: formData.type === 'company' ? formData.numberOfParticipants?.toString() : '',

        paymentMethod: formData.paymentMethod === 'cash' ? 1 : 0,
        lisainfo: formData.additionalInfo || ''
      };

      console.log('Form Data:', formData);
      console.log('Participant Data being sent:', participantData);

      if (formData.type === 'individual') {
        if (!formData.firstName || !formData.lastName || !formData.personalCode) {
          throw new Error('Eesnimi, perekonnanimi ja isikukood on kohustuslikud');
        }
        if (formData.personalCode.length !== 11 || !/^\d+$/.test(formData.personalCode)) {
          throw new Error('Isikukood peab olema 11-kohaline number');
        }
      } else {
        console.log('Checking company validation fields:', {
          companyName: formData.companyName,
          registrationCode: formData.registrationCode,
          numberOfParticipants: formData.numberOfParticipants
        });
        if (!formData.companyName || !formData.registrationCode || !formData.numberOfParticipants) {
          console.log('Validation failed:', {
            companyName: formData.companyName,
            registrationCode: formData.registrationCode,
            numberOfParticipants: formData.numberOfParticipants
          });
          throw new Error('Ettevõtte nimi, registrikood ja osavõtjate arv on kohustuslikud');
        }
        if (formData.registrationCode.length !== 8 || !/^\d+$/.test(formData.registrationCode)) {
          throw new Error('Registrikood peab olema 8-kohaline number');
        }
      }

      const response = await fetch(`http://localhost:5000/api/Events/${eventId}/participants`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
        body: JSON.stringify(participantData),
      });

      if (!response.ok) {
        const errorData = await response.json();
        console.error('Server error:', errorData);
        const validationErrors = errorData.errors ? 
          Object.entries(errorData.errors)
            .map(([key, value]) => {
              if (Array.isArray(value)) {
                return `${key}: ${value.join(', ')}`;
              }
              return `${key}: ${value}`;
            })
            .join('\n') 
          : errorData.message || errorData.title;
        throw new Error(validationErrors || 'Osavõtja lisamine ebaõnnestus');
      }

      const participantsResponse = await fetch(`http://localhost:5000/api/Participants/event/${eventId}`);
      if (!participantsResponse.ok) {
        throw new Error('Osavõtjate andmete laadimine ebaõnnestus');
      }
      const participantsData = await participantsResponse.json();

      const participantsArray = participantsData.$values || participantsData;
      if (!Array.isArray(participantsArray)) {
        console.error('Expected array but got:', participantsData);
        throw new Error('Invalid participants data format from server');
      }
      setParticipants(participantsArray);

      setFormData({
        firstName: '',
        lastName: '',
        personalCode: '',
        paymentMethod: 'bank',
        additionalInfo: '',
        type: 'individual',
        numberOfParticipants: undefined,
        companyName: '',
        registrationCode: ''
      });

      
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Osavõtja lisamine ebaõnnestus');
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteParticipant = async (participantId: number) => {
    if (window.confirm('Oled kindel, et soovid osavõtjat kustutada?')) {
      try {
        setLoading(true);
        setError(null);
        const response = await fetch(`http://localhost:5000/api/Participants/${participantId}`, {
          method: 'DELETE',
        });

        if (!response.ok) {
          throw new Error('Osavõtja kustutamine ebaõnnestus.');
        }

        const participantsResponse = await fetch(`http://localhost:5000/api/Participants/event/${eventId}`);
        if (!participantsResponse.ok) {
          throw new Error('Osavõtjate andmete laadimine ebaõnnestus pärast kustutamist.');
        }
        const participantsData = await participantsResponse.json();
        const participantsArray = participantsData.$values || participantsData;
        if (!Array.isArray(participantsArray)) {
          console.error('Expected array but got:', participantsData);
          throw new Error('Invalid participants data format from server after deletion.');
        }
        setParticipants(participantsArray);

      } catch (err) {
        setError(err instanceof Error ? err.message : 'Osavõtja kustutamine ebaõnnestus.');
      } finally {
        setLoading(false);
      }
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-900 mx-auto"></div>
          <p className="mt-4 text-gray-600">Laadimine...</p>
        </div>
      </div>
    );
  }

  if (!event) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-900 mb-4">Üritust ei leitud</h1>
          <button 
            onClick={() => navigate('/')}
            className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50"
          >
            Tagasi avalehele
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <PageHeader title="Osavõtjad" />
      
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="bg-white shadow-md rounded-lg overflow-hidden">
          {/* Event Details */}
          <div className="px-6 py-5 border-b border-gray-200">
            <h2 className="text-xl font-semibold text-blue-600 mb-4">Ürituse info</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm text-gray-700">
              <div>
                <p><span className="font-medium">Ürituse nimi:</span> {event.name}</p>
                <p className="mt-2"><span className="font-medium">Toimumisaeg:</span> {new Date(event.eventDate).toLocaleDateString('et-EE')}</p>
                <p className="mt-2"><span className="font-medium">Koht:</span> {event.location}</p>
                <p className="mt-2"><span className="font-medium">Osavõtjate arv:</span> {event.participantCount}</p>
                <p className="mt-2"><span className="font-medium">Lisainfo:</span> {event.additionalInfo}</p>
              </div>
            </div>
          </div>

          {/* Participant List */}
          <div className="px-6 py-5">
            <h2 className="text-xl font-semibold text-blue-600 mb-1">Osavõtjad</h2>
            {participants.length === 0 ? (
              <p className="text-gray-500">Osavõtjaid pole veel lisatud</p>
            ) : (
              <ul className="divide-y divide-gray-200">
                {participants.map(participant => (
                  <li key={participant.id} className="py-3 flex items-center justify-between">
                    <div>
                      <span className="font-medium">{participant.displayName}</span>
                      <span className="text-gray-500 ml-2">
                        {participant.type === 0 ? participant.personalCode : participant.registrationCode}
                      </span>
                    </div>
                    <div className="flex space-x-4">
                      <button 
                        onClick={() => handleViewParticipant(participant.id)}
                        className="text-blue-600 hover:text-blue-800 font-medium"
                      >
                        VAATA
                      </button>
                      <button 
                        onClick={() => handleDeleteParticipant(participant.id)}
                        className="text-red-600 hover:text-red-800 font-medium"
                      >
                        KUSTUTA
                      </button>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
        
        {!event.isPastEvent && (
          <div className="mt-8 bg-white shadow-md rounded-lg overflow-hidden">
            <div className="px-6 py-5">
              <h3 className="text-xl font-semibold text-blue-600 mb-6 text-center">Osavõtjate lisamine</h3>
              <form className="space-y-6 max-w-2xl mx-auto" onSubmit={handleSubmit}>
                <div className="flex items-center justify-center space-x-8">
                  <label className="inline-flex items-center">
                    <input 
                      type="radio" 
                      className="form-radio text-blue-600" 
                      name="type" 
                      value="individual" 
                      checked={formData.type === 'individual'}
                      onChange={handleTypeChange}
                    />
                    <span className="ml-2 text-gray-700">Eraisik</span>
                  </label>
                  <label className="inline-flex items-center">
                    <input 
                      type="radio" 
                      className="form-radio text-blue-600" 
                      name="type" 
                      value="company"
                      checked={formData.type === 'company'}
                      onChange={handleTypeChange}
                    />
                    <span className="ml-2 text-gray-700">Ettevõte</span>
                  </label>
                </div>
                
                {error && (
                  <div className="p-4 bg-red-50 text-red-700 rounded-md">
                    {error}
                  </div>
                )}
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  {formData.type === 'individual' ? (
                    <>
                      <div>
                        <label htmlFor="firstName" className="block text-sm font-medium text-gray-700">Eesnimi</label>
                        <input
                          type="text"
                          id="firstName"
                          name="firstName"
                          value={formData.firstName}
                          onChange={handleChange}
                          className="mt-1 block w-full rounded-md border-black border-2 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                          required
                        />
                      </div>
                      <div>
                        <label htmlFor="lastName" className="block text-sm font-medium text-gray-700">Perekonnanimi</label>
                        <input
                          type="text"
                          id="lastName"
                          name="lastName"
                          value={formData.lastName}
                          onChange={handleChange}
                          className="mt-1 block w-full rounded-md border-black border-2 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                          required
                        />
                      </div>
                      <div>
                        <label htmlFor="personalCode" className="block text-sm font-medium text-gray-700">Isikukood</label>
                        <input
                          type="text"
                          id="personalCode"
                          name="personalCode"
                          value={formData.personalCode}
                          onChange={handleChange}
                          className="mt-1 block w-full rounded-md border-black border-2 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                          required
                        />
                      </div>
                    </>
                  ) : (
                    <>
                      <div>
                        <label htmlFor="companyName" className="block text-sm font-medium text-gray-700">Ettevõtte nimi</label>
                        <input
                          type="text"
                          id="companyName"
                          name="companyName"
                          value={formData.companyName || ''}
                          onChange={handleChange}
                          className="mt-1 block w-full rounded-md border-black border-2 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                          required
                        />
                      </div>
                      <div>
                        <label htmlFor="registrationCode" className="block text-sm font-medium text-gray-700">Registrikood</label>
                        <input
                          type="text"
                          id="registrationCode"
                          name="registrationCode"
                          value={formData.registrationCode || ''}
                          onChange={handleChange}
                          className="mt-1 block w-full rounded-md border-black border-2 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                          required
                        />
                      </div>
                      <div>
                        <label htmlFor="numberOfParticipants" className="block text-sm font-medium text-gray-700">Osavõtjate arv</label>
                        <input
                          type="number"
                          id="numberOfParticipants"
                          name="numberOfParticipants"
                          value={formData.numberOfParticipants || ''}
                          onChange={handleChange}
                          className="mt-1 block w-full rounded-md border-black border-2 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                          required
                          min="1"
                        />
                      </div>
                    </>
                  )}
                  
                  <div>
                    <label htmlFor="paymentMethod" className="block text-sm font-medium text-gray-700">Makseviis</label>
                    <select
                      id="paymentMethod"
                      name="paymentMethod"
                      value={formData.paymentMethod}
                      onChange={handleChange}
                      className="mt-1 block w-full rounded-md border-black border-2 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                      required
                    >
                      <option value="bank">Pangaülekanne</option>
                      <option value="cash">Sularaha</option>
                    </select>
                  </div>
                  
                  <div className="md:col-span-2">
                    <label htmlFor="additionalInfo" className="block text-sm font-medium text-gray-700">Lisainfo</label>
                    <textarea
                      id="additionalInfo"
                      name="additionalInfo"
                      value={formData.additionalInfo}
                      onChange={handleChange}
                      rows={3}
                      className="mt-1 block w-full rounded-md border-black border-2 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                    />
                  </div>
                </div>
                
                <div className="flex justify-center space-x-4">
                  <button 
                    type="button"
                    onClick={() => navigate(-1)}
                    className="px-6 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50"
                    disabled={loading}
                  >
                    Tagasi
                  </button>
                  <button
                    type="submit"
                    disabled={loading}
                    className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50"
                  >
                    {loading ? 'Salvestan...' : 'Salvesta'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </div>
  );
} 