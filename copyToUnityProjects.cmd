REM void copyToUnityProjects(string pluginName, string copyFilePath)
REM     'pluginName' is the full path of the built DLL, minus the .dll extension.
REM     'copyFilePath' is the path to a user-created text file.
REM     That text will include the full path of every directory to which the DLL/PDB files are to be copied.
REM     If the provided paths do not exist, then they will be created.

@ECHO OFF

ECHO This file should contain names of Unity project directories: %2
IF EXIST %2 (
    ECHO File found!
) ELSE (
    ECHO File not found!
    EXIT /B
)

FOR /F "delims=" %%f IN (%2) DO (
    ECHO Copying dll/pdb to: %%f
    IF NOT EXIST "%%f" MKDIR "%%f"
    COPY "%1.dll" "%%f"
    COPY "%1.pdb" "%%f"
)
