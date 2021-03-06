#!/bin/bash
if [ $# -eq 0 ]
 then
   echo Creating backup.
   echo No password given as argument.
   echo Please enter database password:
   read -s password
   else 
      password=$1
 fi


docker exec -it db /opt/mssql-tools/bin/sqlcmd \
   -S localhost -U SA -P $password \
   -Q "BACKUP DATABASE YgoProAnalytics TO DISK = N'/var/opt/mssql/backup/ygoproa.bak' WITH NOFORMAT, NOINIT, NAME = 'YGOProAnalytics-full', SKIP, NOREWIND, NOUNLOAD, STATS = 10"

#Copy backup file to current directory.
docker cp db:/var/opt/mssql/backup ./
#remove backup file from docker.
docker exec -it db rm -rf /var/opt/mssql/backup
#change backup permissions
chmod 777 backup/*