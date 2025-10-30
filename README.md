# SpeedwayTyper

Kompleksowa aplikacja do prowadzenia typowania wyników spotkań żużlowych. Poniższy dokument prowadzi przez konfigurację środowiska, zasady naliczania punktów, przepływ zaproszeń oraz planowane integracje.

## Spis treści
- [Architektura i konfiguracja środowiska](#architektura-i-konfiguracja-środowiska)
  - [Wymagania wstępne](#wymagania-wstępne)
  - [Zmienne środowiskowe](#zmienne-środowiskowe)
  - [Uruchomienie przez Docker Compose](#uruchomienie-przez-docker-compose)
- [Baza danych i seedowanie](#baza-danych-i-seedowanie)
- [Punktacja](#punktacja)
- [Przepływ zaproszeń](#przepływ-zaproszeń)
- [Eksport danych](#eksport-danych)
- [Planowane integracje](#planowane-integracje)
  - [E-mail](#e-mail)
  - [Discord](#discord)
  - [Webhooki HTTP](#webhooki-http)

## Architektura i konfiguracja środowiska

Aplikacja składa się z trzech projektów `.NET 7`:

- **Server** – aplikacja ASP.NET Core hostująca API, logikę biznesową i integrację z bazą danych PostgreSQL.
- **Client** – klient Blazor WebAssembly.
- **Shared** – współdzielone modele danych.

> ℹ️ Endpointy obsługujące typowania zostały scalone w `api/picks`. Poprzednie ścieżki `api/predictions/...` nie są już dostępne.

### Wymagania wstępne

- Docker 24+ oraz Docker Compose 2.20+.
- Opcjonalnie .NET 7 SDK jeżeli chcesz uruchamiać rozwiązanie spoza kontenerów.

### Zmienne środowiskowe

Serwer oczekuje następujących ustawień (w `docker-compose.yml` są one już zdefiniowane):

| Zmienna | Opis |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | Łańcuch połączenia do PostgreSQL (format `Host=...;Port=...;Database=...;Username=...;Password=...`). |
| `Jwt__Key` | Klucz symetryczny używany do podpisywania tokenów JWT – minimum 32 znaki. |
| `Jwt__Issuer` | Issuer wpisywany do tokenów JWT. |
| `ASPNETCORE_ENVIRONMENT` | Najczęściej `Development` dla środowiska lokalnego. |

### Uruchomienie przez Docker Compose

1. Zbuduj i uruchom stos:

   ```bash
   docker compose up --build
   ```

2. API zostanie wystawione pod `http://localhost:8080`, PostgreSQL pod `localhost:5432` (użytkownik `speedway`, hasło `speedway`).

3. Pierwsze uruchomienie zainicjalizuje bazę danymi z pliku `Server/Database/seed.sql`. W razie potrzeby ponownego załadowania danych usuń wolumen `postgres-data` lub wykonaj polecenia z sekcji [Baza danych i seedowanie](#baza-danych-i-seedowanie).

## Baza danych i seedowanie

Baza danych korzysta z Entity Framework Core z providerem PostgreSQL. Struktura jest definiowana przez migrację `InitialPostgres` w katalogu `Server/Migrations`. Zestaw startowych danych (8 drużyn oraz harmonogram dwóch pierwszych rund sezonów 2024 i 2025) dostarcza skrypt `Server/Database/seed.sql` kopiowany do katalogu inicjalizacyjnego Postgresa przez Docker Compose.

Aby ponownie wgrać dane w istniejącej bazie (np. po zmianie harmonogramu), użyj poniższych poleceń:

```bash
docker compose exec postgres psql \
  -U speedway \
  -d speedwaytyper \
  -f /docker-entrypoint-initdb.d/seed.sql
```

Polecenie usuwa dane z tabel `Predictions`, `Matches` i `Teams`, po czym ładuje drużyny oraz mecze dla kolejnych rund.

## Punktacja

Algorytm naliczania punktów znajduje się w `Server/Services/PredictionService.cs`.

- Za idealne trafienie remisu (`0:0`) użytkownik otrzymuje 50 punktów.
- Kod zawiera również ścieżkę przyznania 35 punktów za perfekcyjne trafienie wyniku, lecz z powodu obecnego warunku (`typicalResult` sprawdza remis) jest ona osiągalna tylko dla meczów zakończonych remisem – to znane ograniczenie, które wymaga poprawki w przyszłych iteracjach.
- W pozostałych przypadkach punktacja zależy od różnicy pomiędzy przewidywaną a faktyczną różnicą bramek. Przedziały różnicy 0–2, 3–4, …, 17–18 dają odpowiednio 20, 18, 16, 14, 12, 10, 8, 6 oraz 4 punkty. Większe odchylenie skutkuje jedynie 2 punktami.
- Jeśli użytkownik poprawnie oszacował różnicę, ale pomylił się co do zwycięzcy (lub wytypował zwycięzcę, a padł remis), zdobywa 0 punktów.
- Każda aktualizacja typowania automatycznie przelicza sumę punktów i liczbę perfekcyjnych trafień dla użytkownika.

## Przepływ zaproszeń

Choć backend nie posiada jeszcze dedykowanych endpointów do zarządzania zaproszeniami, przyjęto następujący proces organizacyjny:

1. **Administrator ligi** tworzy konto użytkownika (lub rejestruje je ręcznie w panelu Identity) i przypisuje mu rolę.
2. **Generowanie tokenu** – planowany endpoint `POST /api/invitations` wygeneruje jednorazowy token (ważny określoną liczbę dni) powiązany z adresem e-mail.
3. **Dystrybucja linku** – token trafi do zainteresowanych kanałem komunikacji (e-mail / Discord / webhook – opis w [Planowane integracje](#planowane-integracje)).
4. **Aktywacja konta** – użytkownik przejdzie na stronę `/accept-invite?token=...`, ustawi hasło i zaakceptuje regulamin.
5. **Śledzenie statusu** – planowany widok administratora wyświetli listę aktywnych zaproszeń, daty wygaśnięcia i status wykorzystania.

Na etapie MVP tokeny mogą być generowane ręcznie (np. GUID zapisany w tabeli pomocniczej), a dystrybucja odbywać się manualnie. Dokument opisuje docelowy proces, aby ułatwić przyszłe wdrożenie.

## Eksport danych

Eksport wyników i typowań planowany jest w postaci endpointów REST zwracających pliki CSV/JSON:

- `GET /api/export/matches` – harmonogram wraz z wynikami i oznaczeniem sezonu/okrągłej rundy.
- `GET /api/export/predictions?round=...` – typowania użytkowników dla zadanej rundy (wraz z punktami).
- `GET /api/export/leaderboard` – aktualna tabela punktowa.

Wersja startowa może zwracać statyczne dane (stub), dzięki czemu integracje zewnętrzne mogą zostać przygotowane równolegle. Po wdrożeniu właściwej logiki eksportowej endpointy zostaną podłączone do repozytoriów danych.

## Planowane integracje

### E-mail

- Wysyłka zaproszeń i przypomnień o zbliżających się rundach.
- Docelowa implementacja: moduł w tle (Hosted Service) korzystający z SMTP lub dostawcy typu SendGrid.
- Na razie przewidziany jest stub `IEmailNotificationService`, który loguje zdarzenia do konsoli (pozwala testować przepływ bez realnej wysyłki).

### Discord

- Powiadomienia o publikacji nowych rund oraz wynikach końcowych.
- Webhook Discorda będzie wywoływany przez osobny serwis, który na wejściu otrzyma strukturę JSON z informacjami o meczu.
- Aktualnie planowany jest stub, który wykonuje żądanie `POST` do zdefiniowanego URL-a i ignoruje ewentualne błędy, zapisując je w logu.

### Webhooki HTTP

- Ogólne powiadomienia dla systemów zewnętrznych (np. CMS klubu).
- Każde ważne zdarzenie domenowe (opublikowanie rundy, zamknięcie typowania, aktualizacja punktacji) będzie emitowane przez `IIntegrationEventPublisher`.
- Pierwsza wersja może jedynie kolejować zdarzenia w pamięci i eksponować je przez endpoint diagnostyczny, co ułatwi testy bez konieczności konfiguracji docelowych adresów.

---

Masz pytania lub chcesz rozszerzyć dokumentację? Dodaj nową sekcję w tym pliku, aby pozostawić wskazówki dla kolejnych osób rozwijających projekt.
