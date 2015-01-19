:: import Minnesota data
@echo on
echo starting import.
:: dont forget to set up your PATH extensions for shp2pgsql and psql

:: pushd C:\Users\FloorMonster\Documents\GitHub\trout-dash\backend\TroutStreamMangler\TroutStreamMangler\bin\Debug\MN\Data\Streams
:: for %%f in (*.shp) do shp2pgsql -d -I -s 4326 %%f %%~nf > %%~nf.sql
:: for %%f in (*.sql) do psql -d TroutDashPrototype -f %%f --host=localhost --username=postgres
:: popd

pushd C:\Users\FloorMonster\Documents\GitHub\trout-dash\backend\TroutStreamMangler\TroutStreamMangler\MN\Data\Restrictions
for %%f in (*.shp) do shp2pgsql -d -I -s 4326 %%f %%~nf > %%~nf.sql
for %%f in (*.sql) do psql -d TroutDashPrototype -f %%f --host=localhost --username=postgres
popd

::pushd C:\Users\FloorMonster\Documents\GitHub\trout-dash\backend\TroutStreamMangler\TroutStreamMangler\MN\Data\PubliclyAccessibleLands\Easements
::for %%f in (*.shp) do shp2pgsql -d -I -s 4326 %%f %%~nf > %%~nf.sql
::for %%f in (*.sql) do psql -d TroutDashPrototype -f %%f --host=localhost --username=postgres
::popd

pushd C:\Users\FloorMonster\Documents\GitHub\trout-dash\backend\TroutStreamMangler\TroutStreamMangler\MN\Data\PubliclyAccessibleLands\StateParks
for %%f in (*.shp) do shp2pgsql -d -I -s 4326 %%f %%~nf > %%~nf.sql
for %%f in (*.sql) do psql -d TroutDashPrototype -f %%f --host=localhost --username=postgres
popd

pushd C:\Users\FloorMonster\Documents\GitHub\trout-dash\backend\TroutStreamMangler\TroutStreamMangler\MN\Data\PubliclyAccessibleLands\WildlifeManagementAreas
for %%f in (*.shp) do shp2pgsql -d -I -s 4326 %%f %%~nf > %%~nf.sql
for %%f in (*.sql) do psql -d TroutDashPrototype -f %%f --host=localhost --username=postgres
popd

::
pushd C:\Users\FloorMonster\Documents\GitHub\trout-dash\backend\TroutStreamMangler\TroutStreamMangler\MN\Data\TroutStreamSections
for %%f in (*.shp) do shp2pgsql -d -I -s 4326 %%f %%~nf > %%~nf.sql
for %%f in (*.sql) do psql -d TroutDashPrototype -f %%f --host=localhost --username=postgres
popd
::