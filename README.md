### Eeldused
- .NET 9.0 SDK
- Node.js (LTS versioon)
- Docker
- Visual Studio 2022 või Visual Studio Code

### Käivita docker
```bash
docker-compose up --build


4. Käivita backend:
```bash
cd EventManagement.API
dotnet run
```

5. Käivita frontend:
```bash
npm install
npm run dev
```

## Rakenduse Arhitektuur

Rakendus koosneb kahest peamisest osast:

### 1. Frontend (React + TypeScript)
- Asub `src` kaustas
- Kasutab React raamistikku
- Stiilid on realiseeritud Tailwind CSS-iga
- Tüübid on defineeritud TypeScript failides

### 2. Backend (.NET Web API)
- Asub `EventManagement.API` kaustas
- Järgib N-kihtide arhitektuuri:
  - **Controllers**: API endpointide haldamine
  - **Models**: Andmemudelid ja DTO-d
  - **Data**: Andmebaasi kontekst ja migratsioonid
  - **Utils**: Abifunktsioonid ja validaatorid

### Andmebaas
- MySQL andmebaas
- Tabelid:
  - Events (üritused)
  - Participants (osalejad)
- Andmebaasi skriptid asuvad `mysql-init` kaustas

## Koodi Struktuur

### Frontend
- `src/components/`: Taaskasutatavad komponendid
- `src/views/`: Lehevaated
- `src/types/`: TypeScript tüübid
- `src/utils/`: Abifunktsioonid

### Backend
- `Controllers/`: API endpointide loogika
- `Models/`: Andmemudelid
- `Data/`: Andmebaasi kontekst
- `Utils/`: Abifunktsioonid

## API Dokumentatsioon

API dokumentatsioon on kättesaadav Swagger UI kaudu: https://localhost:5001/swagger

Peamised endpointid:
- `/api/Events`: Ürituste haldamine
- `/api/Participants`: Osalejate haldamine
