import { Link, useLocation } from 'react-router-dom';

export function Header() {
  const location = useLocation();
  
  return (
    <header className="bg-white shadow-sm">
      <div className="max-w-1xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-stretch justify-between h-17">
          {/* Container for Logo and Navigation */}
          <div className="flex items-stretch">
            {/* NULLAM logo */}
            <div className="flex items-center">
              <Link to="/">
                <img src="/logo.svg" alt="NULLAM" className="h-10" />
              </Link>
            </div>

            {/* Navigation Links */}
            <nav className="flex items-stretch ml-7">
              <Link 
                to="/" 
                className={`flex items-center px-4 py-2 text-sm font-medium h-full ${location.pathname === '/' ? 'bg-blue-600 text-white' : 'text-gray-500 hover:text-gray-900'}`}
              >
                AVALEHT
              </Link>
              <Link 
                to="/uritus-lisamine" 
                className={`flex items-center px-4 py-2 text-sm font-medium h-full ${location.pathname === '/uritus-lisamine' ? 'bg-blue-600 text-white' : 'text-gray-500 hover:text-gray-900'}`}
              >
                ÜRITUSE LISAMINE
              </Link>
            </nav>
          </div>

          {/* Symbol */}
          <div className="flex items-center">
            <img src="/symbol.svg" alt="Symbol" className="h-[60px]" />
          </div>
        </div>
      </div>
    </header>
  );
} 