export function Footer() {
  return (
    <footer className="bg-gray-800 text-gray-300 py-12">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 grid grid-cols-3 gap-8">
        <div>
          <h3 className="text-lg font-semibold mb-4">Curabitur</h3>
          <ul className="space-y-2">
            <li>Emauris</li>
            <li>Kfringilla</li>
            <li>Oin magna sem</li>
            <li>Kelementum</li>
          </ul>
        </div>
        <div>
          <h3 className="text-lg font-semibold mb-4">Fusce</h3>
          <ul className="space-y-2">
            <li>Econsectetur</li>
            <li>Ksollicitudin</li>
            <li>Omvulputate</li>
            <li>Nunc fringilla tellu</li>
          </ul>
        </div>
        <div>
          <h3 className="text-lg font-semibold mb-4">Kontakt</h3>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="font-semibold">Peakontor: Tallinnas</p>
              <p>Väike-Ameerika 1, 11415 Tallinn</p>
              <p>Telefon: 605 4450</p>
              <p>Faks: 605 3186</p>
            </div>
            <div>
              <p className="font-semibold">Harukontor: Võrus</p>
              <p>Oja tn 7 (külastusaadress)</p>
              <p>Telefon: 605 3330</p>
              <p>Faks: 605 3155</p>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
} 