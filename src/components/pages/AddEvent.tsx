import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { PageHeader } from '../layout/PageHeader';
import { FormCard } from '../layout/FormCard';
import { ErrorMessage } from '../layout/ErrorMessage';
import { FormButtons } from '../layout/FormButtons';
import { LoadingSpinner } from '../layout/LoadingSpinner';

interface EventData {
  name: string;
  eventDate: string;
  location: string;
  additionalInfo: string;
}

export function AddEvent() {
  const navigate = useNavigate();
  const [eventData, setEventData] = useState<EventData>({
    name: '',
    eventDate: '',
    location: '',
    additionalInfo: ''
  });
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setEventData(prevState => ({
      ...prevState,
      [name]: value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      // The datetime-local input already provides the date in ISO format
      const response = await fetch('http://localhost:5000/api/Events', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
        },
        body: JSON.stringify({
          ...eventData,
          eventDate: eventData.eventDate // Already in ISO format
        }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to create event');
      }

      navigate('/');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create event');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <LoadingSpinner fullScreen />;
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <PageHeader title="Lisa üritus" />
      
      <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <FormCard>
          <ErrorMessage message={error || ''} className="mb-4" />
          <form className="space-y-6" onSubmit={handleSubmit}>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="name">Ürituse nimi:</label>
              <input 
                type="text" 
                id="name"
                name="name"
                value={eventData.name}
                onChange={handleChange}
                required
                className="block w-full rounded-md border-2 border-black shadow-sm focus:border-blue-500 focus:ring-blue-500" 
              />
            </div>
            
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="eventDate">Toimumisaeg:</label>
              <input 
                type="datetime-local" 
                id="eventDate"
                name="eventDate"
                value={eventData.eventDate}
                onChange={handleChange}
                required
                min={new Date().toISOString().slice(0, 16)}
                className="block w-full rounded-md border-2 border-black shadow-sm focus:border-blue-500 focus:ring-blue-500" 
              />
            </div>
            
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="location">Koht:</label>
              <input 
                type="text" 
                id="location"
                name="location"
                value={eventData.location}
                onChange={handleChange}
                required
                className="block w-full rounded-md border-2 border-black shadow-sm focus:border-blue-500 focus:ring-blue-500" 
              />
            </div>
            
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="additionalInfo">Lisainfo (maksimaalselt 1000 tähemärki):</label>
              <textarea 
                id="additionalInfo"
                name="additionalInfo"
                rows={4} 
                value={eventData.additionalInfo}
                onChange={handleChange}
                maxLength={1000}
                className="block w-full rounded-md border-2 border-black shadow-sm focus:border-blue-500 focus:ring-blue-500"
              />
            </div>
            
            <FormButtons 
              onBack={() => navigate('/')}
              onSubmit={() => {}}
              submitText="Lisa"
              isLoading={loading}
            />
          </form>
        </FormCard>
      </div>
    </div>
  );
} 