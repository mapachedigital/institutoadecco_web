#/bin/sh

# Copyright (c) 2021, Mapache Digital
# Version: 1.1
# Author: Samuel Kobelkowsky
# Email: samuel@mapachedigital.com
#
# Remove a migration from a .NET proyect using SQLite

PATH=/bin:/usr/bin:/c/Program\ Files/dotnet:/opt/homebrew/bin:/usr/local/share/dotnet

# Configure the following variables:
SOLUTION="InstitutoAdecco.slnx"
SQLPROJECT="SqliteMigrations"
MAINPROJECT="InstitutoAdecco"
DATABASEPROVIDER="Sqlite"
CONTEXT="ApplicationDbContext"

usage() { echo -e "\nUsage $0."; }

while getopts ":h:d" opt; do
	case $opt in
		h) usage; exit 0;;
		\?) usage; echo -e "\nError: invalid option: ${OPTARG}." >&2; exit 1;;
	esac	
done

shift $((OPTIND-1))
MIGRATIONNAME="$1"

dotnet tool install --global dotnet-ef && \
dotnet tool install dotnet-ef && \
dotnet build $SOLUTION && \
dotnet ef migrations remove --project $SQLPROJECT --startup-project $MAINPROJECT --context $CONTEXT -- --DatabaseProvider $DATABASEPROVIDER && \
dotnet build $SOLUTION