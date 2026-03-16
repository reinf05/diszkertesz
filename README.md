# Dísznövénykertész tanulás segítő app
# Cél
A projekt célja az, hogy a dísznövénykertész képzés keretein belül megtanulandó növények tanulását könnyebbé tegye, illetve egy platformot adjon saját növények nyomonkövetéséhez is.

# Motiváció
A dísznövényképzés során megtanulandó hatalmas -latin- névanyag, illetve növényismeret elősegítéséhez készült az app. További motiváció a mérnökinformatikai képzésen tanultak gyakorlati implementálása, illetve kiegészítése

A cél egy központosított tudástár létrehozása, amely elérhető bárhonnan és játékos módon segíti a tanulást. További cél a felhőinfrastruktúrák megtanulása, gyakorlat szerzés és helyes implementálás megvalósítása.


# Funkciók
Növénytár és kereső: Az adatbázisban rendelkezésre álló növények kilistázása, és az adatbázisban történő keresés megvalósítása latin és magyar név alapján.
    Hatékony betöltés: API oldalon megvalósított offset alapú lapozás (pagination). Ez biztosítja, hogy ne egyszerre töltse be az alkalmazás az összes rendelkezésre álló adatot, ami rengeteg időt igényelne, hanem részletekben, így nincs szükség az alkalmazásban hosszú várakozási időre.
    Kliens oldali megoldás: ObservableCollection segítségével folyamatosan megjeleníti az éppen hozzáadott növényeket is, így a felhasználó nem érzékeli a folyamatos betöltést. A kliens kéri az API-tól folyamatosan a következő lapot, attól függően, hogy a felhasználó mennyire görget le a listában.

Interaktív tanulást segítő kvíz: Az adatbázisból random kiválaszt az API egy növényt, átadja annak egyik képét és nevét, valamint három másik növény nevét a kliensnek.
    Azonnali visszajelzés: Az alkalmazás azonnal visszajelez a felhasználónak a válasza helyességéről.

AI alapú növényfelismerés: Valós idejű felismerés mesterséges intelligencia segítségével.
    PlantNET: Harmadik fél által kezelt API integrálása, mely egy AI modell segítségével felismeri a képről a növényt és JSON formátumban visszaküldi a felismert növény adatait.
    Képkezelés: MAUI keretrendszer kamera modulja segítségével kép készítése, hatékony kezelése és továbbküldése.

Authentikáció és profil kezelés: Felhasználói profil segítségével lehetőség van saját növénylista szerkesztésére, kép mentésével és saját leírás hozzáadásával.
    Regisztrálás és bejelentkezés: Microsoft Entra ID kezeli a felhasználói profilok létrehozását és a bejelentkezést.
    Saját lista: Felhasználók bejelentkezés után saját növény listát tudnak kezelni. Minden lista funkció előtt az alkalmazás hitelesíti a felhasználót Access Token segítségével.
    Kép felismerés és növény tippek: Ha a felhasználó feltölt egy képet a saját növény listájába, akkor az alkalamzás automatikusan elindítja a felismerést (PlantNET API), és ha sikeres akkor tovább lép a konkrét növény tippek lekérésére (Perenual API). Ha valahol elakad a folyamat, akkor azt jelzi a felhasználóval egy egyértelmű error üzenettel.
    Kihívások: Perenual API ingyenes használata szűk keretek között engedélyezett csak, így a rendelkezésre álló növények száma csekély.

Infrastruktúra és felhőalapú architektúra: A backend egésze felhőben fut (Azure), a kliens egy saját fejlesztésű API-on keresztül tud kommunikálni a felhőerőforrásokkal.
    Verziókezelés: GitHub segítségével megvalósítva, elkülönítve a fejlesztő környezet az éles környezettől.
    CI/CD pipeline: GitHub actions segítségével az API forráskódja átesik egy ellenőrzésen, majd Docker konténerbe csomagolódik és automatikusan telepítésre kerül Azure-ban.
    Kihívások: Mivel ez egy házi projekt, így a kiadásokat figyelembe véve a legtöbb felhő erőforrás az ingyenes kategóriában fut, így ezek nullára skálázódnak használaton kívül. Emiatt az első hívásoknak megnövekedett várakozási ideje van, de ezt kliens oldalon egy betöltő képernyővel kezelem.

# Technológiai stack
Frontend: .NET MAUI androidra optimalizálva
Backend: ASP.NET Core Web API (Azure App Service)
Adatbázis: Azure SQL és Azure Blob Storage
Authentikáció: Microsoft Entra ID
API integráció: Harmadik fél által biztosított képfelismerő API (PlantNET) és egy növénytartást segítő API (Perenual API)
Verzió kezelés: GitHub
CI/CD pipeline: GitHub Actions

# Rendszerarchitektúra

# App működés közben
[text](https://drive.google.com/file/d/1dkBDjFkgEttIHqCvCIjUcai3MMv1Y8YE/view?usp=drive_link)

# Mérnöki döntések, tanulságok
## Biztonság
- A projekt **Microsoft Entra ID**-t használ a felhasználók kezeléséhez.
- A kliens megszerzi a tokent, az API csak azt kapja meg, nem kezeli a jelszavakat vagy felhasználókat.
- Az API kulcsok (harmadik fél API-okhoz) környezeti változóként vannak kezelve.

## Skálázhatóság
Hamar rá kellett jönnöm, hogy nagy adathalmazokat kezelni egy lépésben nem optimális, ezért offset alapú lapozást implementáltam.

## CI/CD folyamat
A repetitív feladatok elkerülése érdekében egy egyszerű GitHub Actions workflowt hoztam létre, amely:
1. Teszteli a kódot (SonarQube)
2. Lefuttatja a Docker buildet, majd teszteli azt (Docker Scout) és feltölti Dockerhub-ra
3. Ha sikeresen lefutott az első két lépés, akkor telepíti Azure App Service-re

# Fejlesztési lehetőségek, ismert hibák
- Perenual API válaszai angol nyelven jelennek meg
- Lista elem szerkesztési oldala nem működik (szükséges implementálni oda is a tippeket)
- Súlyozott random implementálása a kvíz logikához
- Ha egyszer bejelntkezett egy felhasználó, nem enged rögtön a regisztrációra lépni
- Perenual API néha null értéket küld egy egy property értékének