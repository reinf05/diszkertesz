# Megvalósítás
## Szerver
A szervert egy itthoni számítógép fogja futtatni, amelyre telepítettem az UbuntuServer x.x verzióját.
A könnyebb fejlesztés, debugolás, mobilitás valamint scalelés miatt a szolgáltatások Docker konténerekben fognak futni, amelyeket egy Docker Compose fájl ofg összefogni. Ehhez telepítettem a szerverre a Docker Engine-t, ahogy a dokumentáció írja.

Mivel minden szolgáltatás Docker Compose által fut, ezért létre hoztam egy compose.yaml fájlt, amelyben deklaráltam a különböző szolgáltatásokat (postgres, pgadmin4) és azok szükséges konfigurációit.
### Adatbázis
Az adatbázis megvalósításához telepítettem a postgres Docker konténerét, hogy azt tudjam futtatni Docker Compose fájl segítségével.

Portok: 5432:5432

ENVIRONMENTAL VARIABLES:
POSTGRES_USER: feri
POSTGRES_PASSWORD: feri

Hogy az adatbázist könnyebb legyen konfigurálni és adatokkal feltölteni, telepítettem a pgadmin4 szolgáltatást is, amely egy egyszerű regisztráció után már enged is belépni a postgresql szerverünkbe, és egy GUI által konfigurálni azt.

Portok: 8080:80

ENVIRONMENTAL VARIABLES:
PGADMIN_DEFAULT_EMAIL: rferko@gmail.com
PGADMIN_DEFAULT_PASSWORD: feri

Ezután beléptem a pgadmin-ba, és létre hoztam az adatbázist, valamint a táblákat.
<img width="644" height="452" alt="image" src="https://github.com/user-attachments/assets/ba8a42c1-b250-4ca7-ad77-4460883f77bc" />

plants.id egy autómatikusan létrehozott ID, amely egy idegen kulcs a descriptions.plant_id oszlopban.
namel: Latin név
nameh: Magyar név

Beállítottam egy Trigger functiont is, amely annyit csinál, hogy ha létrehozunk egy sort a plants táblán, akkor ugyan azzal az ID-val létre jön egy sor a details táblában is. Ebben a sorban nincsenek adatok, csak plant_id van automatikus kitöltve, de így már csak fel kell töltenünk a sorokat, és nem kell foglalkozni azzal, hogy biztos minden növénynek legyen ugyan azzal az ID-val egy sora a deatils táblában is.

### API kezelés
A kommunikáció a szerver és a kliens között API hívásokkal lesz megoldva, ehhez futni fog a szerveren egy Docker konténerben egy ASP.NET Core WebAPI alkalmazás.

Először a kapcsolatot létesítjük az adatbázis szerverrel, ehhez NuGet csomagokat telepítünk (PostgreSQL és EF.Tools). Ezután létre hozunk egy diszkertDbContext fájlt, amely felelős a kapcsolatért az adatbázissal. Létre hozunk egy modelt a sorainknak az adatbázison belül, ez lesz a Plant.cs. Ahhoz, hogy csatlakozni tudjon az alkalmazás az adatbázishoz, szükségünk lesz egy úgynevezett ConnectionStringre, amit az appsettings.json fáljban deklarálunk. Ezután konfiguráljuk a Program.cs fáljban. 
