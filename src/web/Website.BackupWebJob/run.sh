#!/bin/bash
# Entry point used by Azure App Service (Linux) to run the triggered BackupExportJob WebJob.
cd "$(dirname "$0")" || exit 1
exec dotnet DadABase.BackupWebJob.dll
