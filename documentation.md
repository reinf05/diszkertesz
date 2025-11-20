# Megvalósítás
## Szerver
**A fizikai szervert felváltotta egy felhőn alapuló megoldás. Lásd serverdocumentation.md**
**A szerver időnként leáll, így indításnál lehet, hogy várni kell mire az infrastruktúra felkel.**

A szervert egy itthoni számítógép fogja futtatni, amelyre telepítettem az UbuntuServer x.x verzióját.
A könnyebb fejlesztés, debugolás, mobilitás valamint scalelés miatt a szolgáltatások Docker konténerekben fognak futni, amelyeket egy Docker Compose fájl ofg összefogni. Ehhez telepítettem a szerverre a Docker Engine-t, ahogy a dokumentáció írja.

Mivel minden szolgáltatás Docker Compose által fut, ezért létre hoztam egy compose.yaml fájlt, amelyben deklaráltam a különböző szolgáltatásokat (postgres, pgadmin4, webapi) és azok szükséges konfigurációit.

### Adatbázis
Az adatbázis megvalósításához telepítettem a postgres Docker konténerét, hogy azt tudjam futtatni Docker Compose fájl segítségével.

Portok: 5432:5432

Környezeti változókat is meg kell adni: POSTGRES_USER, POSTGRES_PASSWORD.

Hogy az adatbázist könnyebb legyen konfigurálni és adatokkal feltölteni, telepítettem a pgadmin4 szolgáltatást is, amely egy egyszerű regisztráció után már enged is belépni a postgresql szerverünkbe, és egy GUI által konfigurálni azt.

Portok: 8080:80

Környezeti változókban meg kell adni: PGADMIN_DEFAULT_EMAIL, PGADMIN_DEFAULT_PASSWORD

Ezután beléptem a pgadmin-ba (192.168.1.151:8080), és létre hoztam az adatbázist, valamint a táblákat.
<img width="644" height="452" alt="image" src="https://github.com/user-attachments/assets/ba8a42c1-b250-4ca7-ad77-4460883f77bc" />

plants.id egy autómatikusan létrehozott ID, amely egy idegen kulcs a descriptions.plant_id oszlopban.
namel: Latin név
nameh: Magyar név

Beállítottam egy Trigger functiont is, amely annyit csinál, hogy ha létrehozunk egy sort a plants táblán, akkor ugyan azzal az ID-val létre jön egy sor a details táblában is. Ebben a sorban nincsenek adatok, csak plant_id van automatikus kitöltve, de így már csak fel kell töltenünk a sorokat, és nem kell foglalkozni azzal, hogy biztos minden növénynek legyen ugyan azzal az ID-val egy sora a deatils táblában is.

### API kezelés
A kommunikáció a szerver és a kliens között API hívásokkal lesz megoldva, ehhez futni fog a szerveren egy Docker konténerben egy ASP.NET Core WebAPI alkalmazás.

Először a kapcsolatot létesítjük az adatbázis szerverrel, ehhez NuGet csomagokat telepítünk (PostgreSQL és EF.Tools). Ezután létre hozunk egy diszkertDbContext fájlt, amely felelős a kapcsolatért az adatbázissal. Létre hozunk két modelt a két táblánkhoz, Plant.cs és Detail.cs. Ahhoz, hogy csatlakozni tudjon az alkalmazás az adatbázishoz, szükségünk lesz egy úgynevezett ConnectionStringre, amit az appsettings.json fáljban deklarálunk. Ezután konfiguráljuk a Program.cs fáljban. 

Létre hoztam még két modelt, hogy az API endpointok könnyebben tudjanak visszaadni különböző típusú adatot, a különböző igényekhez. Ez a Fullplant.cs, ahol egy növény összes adata (a két tábla: Plants és Details) megtalálható, valamint egy Quiz.cs fájlt, amely a növényfelismerésben fog segíteni.

Létre hozzuk az API endpointokat:
/plant/plants/{page}: Visszaad egy listányi (pageSize változóval változtatható a lista mérete) növényt a Plants táblából.
/plant/details/{id}: Visszaad egy növény leírását a Details táblából.
/plant/quiz: Visszaad egy Quiz adattípust, amely tartalmaz egy képet és négy nevet, amelyből az első (index 0) a képen látható növény neve.
/plant/fullpants/{id}: Visszaad egy növényt minden adatával együtt.
/plant/identify: Bekér egy MultipartFromDataContent típusú változót, amelyet majd átad a PlantNet API-nak, ami elvégzi a növény felismerést, majd visszaadja a legvalószínűbb növényt. Az API kulcsot a Docker compoose fájl segítségével adjuk át a webAPI-nak.

Miután ezek elkészültek, egy Docker image-be csomagolom a programot, és feltelepítem a szerverre, majd frissítem a compose.yaml fájlt, így a Docker Compose ezt a szolgáltatást is el tudja indítani a többivel együtt.
**GithubActions CI/CD pipeline automatikusan elkészíti a Docker image-t és deployolja Azureba. Lásd: serverdocumentation.md**

## Kliens
A kliens alapját egy .NET MAUI applikáció fogja adni, amelynek a felépítésében az MVVM struktúrát használom. Hogy a kódom ne legyen túl nagy, és segítsem a munkámat, telepítem a projektbe az MVVM Community Toolkit nevű NuGet packaget, amely segíteni fog nekem bizonyos kódrészek automatikus legenerálásban. Ezután előkészítettem a projekt mappa struktúráját.
<img width="388" height="392" alt="image" src="https://github.com/user-attachments/assets/0c48ee03-6f5c-4784-b1f8-5f21cc51e88e" />

### Models
A modellek fognak segíteni abban, hogy az adatokat helyesen tudjam tárolni és megjeleníteni.

- Plant.cs: Ez az alapja az összes növény objektumnak.
- FullPlant.cs: Ez egy adott növény részletes nézetéhez szükséges.
- Page.cs: Ez az API pagination miatt fontos, ez teszi lehetővé, hogy ne az összes növényt egyszerre kérjük le, ezzel kizárva a hosszú várakozási időt.
- Quiz.cs: Ez a quiz logikában használt adattípushoz fontos.
- IdentificationResult.cs: Több osztályt foglal magába, amik szükségesek a harmadik fél álltal biztosított API válasz feldolgozásában.

### Views
A view-k felelnek az applikáció kinézetéért.

- MainPage.xaml: A lista nézet. Magába foglal egy CollectionView-t, amely megjeleníti magát a listát, valamint gondoskodik a folyamatos betöltésről is (*RemainingItemsThreshold* segítségével). Amíg nincs betöltve egy növény sem, addig jelen van egy gomb, amely lehetőséget ad arra, hogy betöltsük az első page-t. Töltés közben megjelenik egy töltő képernyő is.
- DetailPage.xaml: Egy növény részletes nézete. Megjeleníti a rendelkezésre álló képeket egy *ScrollView*-ban, illetve a rendelkezésre álló adatokat a képek alatt.
- QuizPage.xaml: Felel a játék megjelenítéséért. 1 képet és 4 választ jelenít meg, valamint tartalmazza külön mezőben a helyes megoldást.
- IdentifyPage.xaml: Alap esetben megjeleníti a kamerát, egy gombot amellyel el lehet készíteni a képet. Ha van elkészített kép, akkor megjeleníti azt, valamint két gombot, az egyikkel új képet lehet készíteni, a másikkal pedig el lehet küldeni azonosításra. Ha sikeres volt az azonosítás, akkor megjelenik a kép, alatta a latin neve a növénynek, egy százalékos szint, amely jelöli a model pontosságát, valamint hétköznapi nevek.
- ProfilePage.xaml: Ha a felhasználó nincs bejelentkezve, akkor két gomb látszik, bejelentkezés és regisztráció. Ha bejelentkezett a felhasználó, akkor a profilját látja.

Minden view *code behind* fájljában a konstuktorban átadom *dependency injection*-el a megfelelő *viewmodel*-t, majd beállítom *BindingContext*-nek őket.

### Plant Service
Ez felel a szerverrel való kommunikációért. Itt vannak megadva azok a függvények, amelyek elhagyhatatlanok a WebAPI és a kliens közti kommunikáció létrejöttéhez.

### Authentication Service
Ez felel a kommunikációért a Microsoft Entra External ID-vel, amely az authentikációt intézi. *SignInAync* és *SignOutAsync* függvények felelnek a be és kijelentkezésért, a *GetAccessTokenAsync* felel a token megújításáért ha szükséges.

### Authentication for Android
Az Android operációs rendszerhez több módosítást is végre kell hajtani, hogy megfelelően működjön az authentikáció. A *Platforms/Android* mappában az új *MsalActivity.cs* szükséges ahhoz, hogy megfelelően vissza tudjon irányítani minket a belépés után az appra, ezt az activity-t regisztrálni kell a *MainActivity.cs*-ben is.

### Base ViewModel
Az összes többi *viewmodel* alapja. Olyan figyelhető tuladjonságokat tartalmaz, mint például az *isBusy*, *title*, *isLoaded*, *IsNotLoaded*, *IsNotBusy* amelyek az összes többi oldalon használva vannak.

A *RelayCommand* kulcsszót használom a függvények előtt, így az *MVVM Community Toolkit* NuGet package legenerálja a szükséges kódot, hogy tudjak rájuk hivatkozni a nézetekben *Command* kulcsszóval.

### Main ViewModel
Megadunk benne *RelayCommand*-okat, amelyek segítségével el tudjuk választani a *View* és a *ViewModel* szintjét egymástól, mivel egyszerűen a *Command* paraméterrel tudunk majd rájuk hivatkozni a nézeteknél. *LoadPageAsync* egy segítő függvény, amely feltölti a növény listát, amely egy figyelhető tulajdonság, így valós időben megjelenik az applikációban is. *GetFirstPageAsync*, *GetMorePageAsync* a növények betöltéséért felel. *GetFullPlantAsync* egy segítő függvény, amely lekéri a kiválasztott növény összes adatát, a *GoToDetailsAsync* függvény hívja meg, majd tovább is lép a megfelelő nézetre. Ez a viewmodel felelős azért is, hogy ha nincs több elérhető adat az adatbázisban, akkor meg se próbáljon lekérni több adatot.
A *GetMorePageAsync* függvényhez hozzá adunk egy *CanExecute* kulcsszót is, amely megakadájozza a felhasználót, hogy tovább görgessen amíg tölt be a következő page, így nem fogja fölöslegesen elkezdeni a függvényt.

### Detail ViewModel
Egyetlen szerepe annyi, hogy megváltoztassa a nézet címét. Jövőben lehetőséget ad a fejlesztéshez.

### Quiz ViewModel
A kvíz játéklogikát tartalmazza. Gombnyomásra lekéri az első kvízt, majd minden válaszadás után lekéri a következőt. Visszajelzést ad a felhasználónak a válasz helyességéről.

### Identify ViewModel
Implementálja a kép készítést (*CaptureAsync* relay command), az új kép készítése (*NewImage*) valamint a felismeréshez szükséges funkciókat. *IdentifyAsync* elküldi a képet *ByteStream* formátumban, megvárja a választ majd megjelenítetti a visszakapott adatokat.
A gyorsabb válasz miatt a képeket átméretezzük mielőtt elküldjük a harmadik fél (*PlantNet*) számára. Az *IdentifyAsync* függvényen belül betöltjük egy *IImage* formátumba, a beépített függvényekkel átméretezzük, majd elmentjük (kissé rontott minőségben) egy *MemoryStream*-be, amelyet átalakítunk egy *byte* tömbbe. Ezt a kisebb, rosszabb minőségű képet adjuk át a *PlantService*-nek, hogy minél kevesebb időt vegyen el a kép feltöltése először a saját API-nak, majd onnan a harmadik fél API-hoz.

### Profile ViewModel
Meghívja az *AuthenticationService* függvényeit, átalakítja a visszakapott adatokat a megfelelő formára, hogy a *ProfilePage* meg tudja jeleníteni. Kezeli a változókat amik alapján változik a hozzátartozó nézet.

### Navigation
Az AppShell.xaml fájlban létre hozok egy menüt, aminek segítségével lehet lépni a különböző lapok között. Ehhez TabBar-t használok.


