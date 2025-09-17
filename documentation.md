# Megvalósítás
## Szerver
A szervert egy itthoni számítógép fogja futtatni, amelyre telepítettem az UbuntuServer x.x verzióját.
A könnyebb fejlesztés, debugolás, mobilitás valamint scalelés miatt a szolgáltatások Docker konténerekben fognak futni, amelyeket egy Docker Compose fájl ofg összefogni. Ehhez telepítettem a szerverre a Docker Engine-t, ahogy a dokumentáció írja.

Mivel minden szolgáltatás Docker Compose által fut, ezért létre hoztam egy compose.yaml fájlt, amelyben deklaráltam a különböző szolgáltatásokat (postgres, pgadmin4, webapi) és azok szükséges konfigurációit.
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
/plant/plants: Visszaad egy listát az összes növényről a Plants táblában.
/plant/plants/{id}: Visszaad egy növényt a Plants táblából.
/plant/details: Visszaad egy listát az összes növényről a Details táblában.
/plant/details/{id}: Visszaad egy növény leírását a Details táblából.
/plant/fullplants: Visszaad egy listát az összes növényről, amely a növények összes adatát tartalmazza.
/plant/quiz: Visszaad egy Quiz adattípust, amely tartalmaz egy képet és négy nevet, amelyből az első (index 0) a képen látható növény neve.
/plant/fullpants/{id}: Visszaad egy növényt minden adatával együtt.

Miután ezek elkészültek, egy Docker image-be csomagolom a programot, és feltelepítem a szerverre, majd frissítem a compose.yaml fájlt, így a Docker Compose ezt a szolgáltatást is el tudja indítani a többivel együtt.

## Kliens
A kliens alapját egy .NET MAUI applikáció fogja adni, amelynek a felépítésében az MVVM struktúrát használom. Hogy a kódom ne legyen túl nagy, és segítsem a munkámat, telepítem a projektbe az MVVM Community Toolkit nevű NuGet packaget, amely segíteni fog nekem bizonyos kódrészek automatikus legenerálásban. Ezután előkészítettem a projekt mappa struktúráját.
<img width="388" height="392" alt="image" src="https://github.com/user-attachments/assets/0c48ee03-6f5c-4784-b1f8-5f21cc51e88e" />

Ezután először a Main Page viewt fogom megcsinálni, ahol látható lesz egy lista a növényekről. Ehhez először csinálok egy modellt (Plant.cs). Ezután a MainPage.XAML fájlban megcsinálom a UI-t.
A Detail Page view fog felelni azért, hogy ha kiválasztunk egy adott növényt, akkor a részletes adatait megjelenítse. Ehhez szükségünk lesz egy FullPlant.cs modellre.

A viewmodelekhez csinálok először egy BaseViewModel.cs osztályt, ami az összes többi ViewModelnek az alapja lesz. Ebben olyan tulajdonságok vannak, amelyek közösek lesznek a ViewModellek között. 
A MainViewModel a MainPage viewmodelje. Ebben létrehozunk egy ObservableCollection, amely arra jó, hogy nem csak tárolja a növények adatait, hanem ha változás van bennük, akkor tudja értesíteni a MainPage viewt is, és tudja frissíteni azt. Dependency injection-nel átadjuk a konstruktoron keresztül a PlantService-t (amely felel a WebAPI kommunikációért), majd létre hozunk egy GetPlantsAsync() Task-ot, amelyet az MVVM Community Toolkit segítségével egy egyszerű tag-el ([RelayCommand]) Command-ra Generálunk, ezt fogja tudni meghívni a View.
Ahhoz hogy átirányítsunk egy Detail nézetre, konfigurálnunk kell egy új Task-ot (GoToDetailsCommand), majd ezt hozzá kell kötnünk a MainView.xaml fájlban a Grid-hez, amely egy egy növény kártyáját fogja egybe. Ehhez GestureRecogniser-t használok, és ide beállítok egy CommandParameter-t is, ami át fogja adni a Detail viewnak az adott növényt, amiről több részletet akarunk látni. Hogy működjön is ez a navigáció, regisztrálni kell egy Route-ot az AppShell.xaml.cs fájlban. Ahhoz, hogy a megfelelő növény adatait átadjuk a DetailPage-nek, a MainViewModel-ben készítettem egy privát Task-ot, ami lekéri az adott növény teljes verzióját (FullPlant) és azt adja át a view-nak. Egy QueryProperty segítségével Shell navigáción keresztül át is lehet adni paraméterként, így a view be tudja tölteni és meg tudja jeleníteni.

Hogy összekössük a view-t a viewmodel-el, az adott View mögötti C# kódban a konstruktorban Dependency injection-nel át kell adni a view modelt, majd egy BindingContext-et beállítani hozzá.

A Services mappában létrehozom a PlantService.cs fájlt, amely felelős lesz a kommunikációért a szerverrel az API hívásokon keresztül.
GetAllPlants visszaadja az összes növényt (szimpla Plant formátumban), GetPlantById visszaad egy adott növényt (FullPlant formátumban).

Az AppShell.xaml fájlban létre hozok egy menüt, aminek segítségével lehet lépni a különböző lapok között. Ehhez TabBar-t használok.
