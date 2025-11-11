# Dísznövénykertész tanulás segítő app

# Cél
Ennek a projektnek a célja az, hogy a MAGYULA Dísznövénykertész képzésén a dísznövények tanulását könnyebbé tegye, valamint egyféle centralizált tudástárként szolgáljon.

Ennek az alkalmazásnak a segítségével lehetőség lesz a növények neveinek könnyebb megtanulására, egy játékos módon. Képekről kell felismerni a növény nevét, amelyet ki kell választani a 4 válasz közül, amelyből egy helyes, a többi három pedig más növény neve. Az alkalmazásban lehetőség lesz a növények között keresni, és kiválasztás után egy részletes leírást (igények, felhasználás, károkozók, szaporítás) láthat.

# Felépítés

## Szerver
A szerver egy házilag üzemeltetett szerver, amely teret ad a szükséges adatbázisnak, valamint a szerver oldali számításoknak is.

ASP.NET Core WebAPI fog eleget tenni a szerver és a kliens közötti kommunikációnak, valamint itt fog lefutni minden számítás is. 

PostgreSQL az adatbázis, ahol különböző táblák fogják tárolni az adatokat.

Minden szolgáltatás Docker Compose segítségével fog futni konténerekben.

**A fizikai szervert felváltotta egy felhőn alapuló megoldás. Lásd serverdocumentation.md**

## Kliens
A kliens oldal csak az UI-ért lesz felelős, valamint API hívásokat fog küldeni a szerver felé, majd a megkapott adatokat rendereli.

A kliens felépítése MAUI-val lesz megoldva, és REST API (HTTP + JSON) alapon fog kommunikálni a szerverrel.

## Docker image
A Docker image a szerveren futtatott ASP.NET Core WebAPI szolgáltatáshoz megtalálható a Dockerhubon. 
Letöltés: `docker pull rferko/diszkerteszapi:1.0`
**A fizikai szervert felváltotta egy felhőn alapuló megoldás. Lásd serverdocumentation.md**

# Alkalmazás működése
Az alkalmazás alap nézete egy lista, amely megjeleníti az adatbázisban rendelkezésre álló növényeket.
Egy kiválaszott növényre kattintva megjelenik az adott növény részletes leírása, több képpel.
Az alkalmazás navigációs csíkjában lehetőség van váltani nézetek között, a jaték nézetben lehet tanulni a növények latin nevét. Megjelenik egy random kép, valamint 3 helytelen és 1 helyes válasz. A válaszadást értékeli az alkalmazás, és rögtön visszajelzést ad a helyességről.
A harmadik nézetben lehetőség van kép készítésére egy növényről, amelyet egy harmadik fél álltal biztosított API-on keresztül egy AI model felismer, és visszaküldi az eredményt.

# További ötletek
- Authentikáció
- Mindenkinek saját magára szabott Quiz logika (súlyozott random)
- Saját növény lista (szerkeszthető, törölhető, hozzárendelhető képpel, locsolási információval)
- Kiegészíteni tippekkel, értesítésekkel (locsolás miatt)
- Dashboard
