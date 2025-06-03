import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Header } from './components/layout/Header';
import { Footer } from './components/layout/Footer';
import { Home } from './components/pages/Home';
import { AddEvent } from './components/pages/AddEvent';
import { ParticipantInfo } from './components/pages/ParticipantInfo';
import { ParticipantList } from './components/pages/ParticipantList';
import { EditParticipant } from './components/pages/EditParticipant';

function App() {
  return (
    <Router>
      <div className="min-h-screen bg-gray-100">
        <Header />
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/uritus-lisamine" element={<AddEvent />} />
          <Route path="/osavotja-info" element={<ParticipantInfo />} />
          <Route path="/osalejad" element={<ParticipantList />} />
          <Route path="/osavotja-info/:id" element={<EditParticipant />} />
        </Routes>
        <Footer />
      </div>
    </Router>
  );
}

export default App;