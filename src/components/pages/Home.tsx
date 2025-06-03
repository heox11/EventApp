import { useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';

interface Event {
  id: string;
  date: string;
  title: string;
  isPastEvent: boolean;
  location: string;
}

export function Home() {
  const navigate = useNavigate();
  const [upcomingEvents, setUpcomingEvents] = useState<Event[]>([]);
  const [pastEvents, setPastEvents] = useState<Event[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchEvents = async () => {
      try {
        const response = await fetch('http://localhost:5000/api/Events', {
          headers: {
            'Accept': 'application/json',
          },
        });
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        const data = await response.json();
        
        const fetchedUpcomingEvents: Event[] = [];
        const fetchedPastEvents: Event[] = [];

        // Handle the response format with $values property
        const eventsArray = data.$values || data;
        
        if (Array.isArray(eventsArray)) {
          eventsArray.forEach((event: any) => {
            const eventDate = new Date(event.eventDate);
            const now = new Date();
            const isPastEvent = eventDate.getTime() < now.getTime() - 60000; // 60000 ms = 1 minute

            const formattedEvent = {
              id: event.id.toString(),
              date: eventDate.toLocaleDateString('en-GB'),
              title: event.name,
              isPastEvent: isPastEvent,
              location: event.location,
            };
            if (formattedEvent.isPastEvent) {
              fetchedPastEvents.push(formattedEvent);
            } else {
              fetchedUpcomingEvents.push(formattedEvent);
            }
          });
        } else {
          console.error('Expected array but got:', data);
          throw new Error('Invalid response format from server');
        }

        fetchedUpcomingEvents.sort((a, b) => new Date(a.date.split('.').reverse().join('-')).getTime() - new Date(b.date.split('.').reverse().join('-')).getTime());
        fetchedPastEvents.sort((a, b) => new Date(b.date.split('.').reverse().join('-')).getTime() - new Date(a.date.split('.').reverse().join('-')).getTime());

        setUpcomingEvents(fetchedUpcomingEvents);
        setPastEvents(fetchedPastEvents);
        setLoading(false);
      } catch (error) {
        console.error("Error fetching events:", error);
        setError("Failed to load events.");
        setLoading(false);
      }
    };

    fetchEvents();
  }, []);

  const handleParticipantsClick = (eventId: string) => {
    navigate(`/osalejad?id=${eventId}`);
  };

  const handleDeleteEvent = async (eventId: string) => {
    if (window.confirm('Oled kindel, et soovid ürituse ja kõik selle osavõtjad kustutada?')) {
      try {
        setLoading(true);
        setError(null);
        const response = await fetch(`http://localhost:5000/api/Events/${eventId}`, {
          method: 'DELETE',
        });

        if (!response.ok) {
          throw new Error('Ürituse kustutamine ebaõnnestus.');
        }

        // Refetch events after deletion
        const fetchEvents = async () => {
          try {
            const response = await fetch('http://localhost:5000/api/Events', {
              headers: {
                'Accept': 'application/json',
              },
            });
            if (!response.ok) {
              throw new Error(`HTTP error! status: ${response.status}`);
            }
            const data = await response.json();
            
            const fetchedUpcomingEvents: Event[] = [];
            const fetchedPastEvents: Event[] = [];

            const eventsArray = data.$values || data;
            
            if (Array.isArray(eventsArray)) {
              eventsArray.forEach((event: any) => {
                const eventDate = new Date(event.eventDate);
                const now = new Date();
                const isPastEvent = eventDate.getTime() < now.getTime() - 60000; // 60000 ms = 1 minute

                const formattedEvent = {
                  id: event.id.toString(),
                  date: eventDate.toLocaleDateString('en-GB'),
                  title: event.name,
                  isPastEvent: isPastEvent,
                  location: event.location,
                };
                if (formattedEvent.isPastEvent) {
                  fetchedPastEvents.push(formattedEvent);
                } else {
                  fetchedUpcomingEvents.push(formattedEvent);
                }
              });
            } else {
              console.error('Expected array but got:', data);
              throw new Error('Invalid response format from server');
            }

            fetchedUpcomingEvents.sort((a, b) => new Date(a.date.split('.').reverse().join('-')).getTime() - new Date(b.date.split('.').reverse().join('-')).getTime());
            fetchedPastEvents.sort((a, b) => new Date(b.date.split('.').reverse().join('-')).getTime() - new Date(a.date.split('.').reverse().join('-')).getTime());

            setUpcomingEvents(fetchedUpcomingEvents);
            setPastEvents(fetchedPastEvents);
            setLoading(false);
          } catch (error) {
            console.error("Error fetching events:", error);
            setError("Failed to load events.");
            setLoading(false);
          }
        };
        fetchEvents();

      } catch (err) {
        setError(err instanceof Error ? err.message : 'Ürituse kustutamine ebaõnnestus.');
      } finally {
        setLoading(false);
      }
    }
  };

  const EventCard = ({ title, events }: { title: string; events: Event[] }) => (
    <div className="bg-white rounded-lg shadow-md overflow-hidden">
      <div className="bg-blue-900 text-white px-6 py-4 text-center">
        <h2 className="text-xl font-semibold">{title}</h2>
      </div>
      <div className="p-6 space-y-4 text-gray-800">
        {loading ? (
          <p>Loading events...</p>
        ) : error ? (
          <p className="text-red-500">{error}</p>
        ) : events.length === 0 ? (
          <p>No events found.</p>
        ) : (
          events.map((event) => (
            <div key={event.id} className="flex justify-between items-center">
              <span className="w-24">{event.date}</span>
              <span className="flex-1 mx-4">
                {event.title}
                <div className="text-sm text-gray-500">{event.location}</div>
              </span>
              <button 
                onClick={() => handleParticipantsClick(event.id)}
                className="text-blue-600 hover:text-blue-800 flex items-center whitespace-nowrap"
              >
                OSAVÕTJAD
              </button>
              <button
                onClick={() => handleDeleteEvent(event.id)}
                className="text-red-600 hover:text-red-800 flex items-center whitespace-nowrap"
                disabled={event.isPastEvent}
                title={event.isPastEvent ? 'Toimunud üritusi ei saa kustutada' : 'Kustuta üritus'}
              >
                <img src="/remove.svg" alt="Remove" className={`h-4 w-4 ml-2 ${event.isPastEvent ? 'opacity-50' : ''}`} />
              </button>
            </div>
          ))
        )}
      </div>
      {title === 'Tulevased üritused' && (
        <button 
          onClick={() => navigate('/uritus-lisamine')}
          className="mt-4 ml-6 mb-6 text-blue-600 hover:text-blue-800"
        >
          LISA ÜRITUS
        </button>
      )}
    </div>
  );

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Banner Section */}
      <div className="max-w-8xl mx-auto">
        <div className="grid grid-cols-1 md:grid-cols-2">
          <div className="bg-blue-900 h-60 flex items-center px-6 sm:px-12">
            <p className="text-xl text-white">
              Sed nec elit vestibulum, <span className="font-bold">tincidunt orci</span> et, sagittis ex. Vestibulum rutrum <span className="font-bold">neque suscipit</span> ante mattis maximus. Nulla non sapien <span className="font-bold">viverra, lobortis lorem non</span>, accumsan metus.
            </p>
          </div>
          <div className="relative h-60">
            <img src="/pilt.jpg" alt="Park bench" className="w-full h-full object-cover" />
          </div>
        </div>
      </div>
      
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          <EventCard title="Tulevased üritused" events={upcomingEvents} />
          <EventCard title="Toimunud üritused" events={pastEvents} />
        </div>
      </div>
    </div>
  );
} 