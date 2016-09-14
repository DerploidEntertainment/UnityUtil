::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
::
:: Created by Dan Vicarel
::
:: void copyToUnityProjects(string buildDir, string rootDir)
::     'buildDir' is the full path of the build output directory, containing files to be copied.
::     'rootDir' is the full path to directory containing user-created text files,
::          each describing a set of files to be copied from buildDir to target directories
::
::     The text files in rootDir will contain "directory blocks" formatted like so:
::
::      # A full-line comment
::      target C:\path\to\some\direcotry
::          rp something.dll
::          rp something.pdb
::          rp something.else.dll
::          rp something.else.pdb
::          cp another.dll
::          cp another.pdb    # an in-line comment
::
::     Each file mentioned below a directory path will be copied to that target directory
::          "rp" will copy and replace any file by the same name already there
::          "cp" will copy but not replace
::          indenting not necessary, but improves readability
::     If a mentioned file path does not exist, then it will be skipped with a warning.
::     If a target directory does not exist, then it will be skipped with a warning.
::
::     Multiple directory blocks may be present in a single file
::     Mutliple such files may be present in rootDir
::
:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::

@ECHO OFF
SETLOCAL EnableDelayedExpansion
CLS

SET err=0

:: Make sure that the provided build output directory actually exists
SET buildDir=%1
IF NOT EXIST %buildDir% (
    SET err=1
    ECHO The directory containing build outputs could not be found:
    ECHO     %buildDir%
)

:: Make sure that the provided text-file root directory actually exists
SET rootDir=%2
IF NOT EXIST %rootDir% (
    SET err=2
    ECHO The directory containinig copy config files could not be found:
    ECHO     %rootDir%
)

:: Exit the script if there were any errors
IF NOT %err%==0 EXIT /B %err%

:: For each targets file in rootDir
FOR %%T IN (%rootDir%\*.targets) DO (
    ECHO.
    ECHO Parsing %%~fT...
    
    SET lineNum=0
    SET targetSet=0
    SET targetDir=
    SET noTarget=0
    FOR /F "eol=# tokens=1,2 delims= " %%L IN (%%T) DO (
        SET /A lineNum+=1        
        SET done=0
        
        :: Set next target directory
        IF !done!==0 (
            IF %%L==target (
                IF EXIST %%M (SET noTarget=0) ELSE SET noTarget=1
                IF !noTarget!==1 (
                    ECHO     Could not find %%M
                    ECHO         Nothing copied.
                ) ELSE (
                    ECHO     Copying to %%M...
                )
                SET targetSet=1
                SET targetDir=%%M
                SET done=1
            )
        )
        
        :: Copy with overwrite
        IF !done!==0 (
            IF %%L==rp (
                IF NOT !targetSet!==1 (
                    ECHO Parsing error on line !lineNum!: A target directory must be specified before an "rp" line
                    EXIT /B 3
                )
                IF !noTarget!==0 (
                    IF EXIST %buildDir%\%%M (
                        IF EXIST !targetDir!\%%M (SET msg=Replaced %%M^^!) ELSE SET msg=Copied %%M^^!
                        COPY %buildDir%\%%M !targetDir! 1> NUL
                    ) ELSE (
                        SET msg=Could not find %buildDir%%%M^^!
                    )
                    ECHO         !msg!
                )
                SET done=1
            )
        )
        
        :: Copy without overwrite
        IF !done!==0 (
            IF %%L==cp (
                IF NOT !targetSet!==1 (
                    ECHO Parsing error on line !lineNum!: A target directory must be specified before a "cp" line
                    EXIT /B 3
                )
                IF !noTarget!==0 (
                    IF EXIST %buildDir%\%%M (
                        IF EXIST !targetDir!\%%M (
                            SET msg=Target already has %%M...
                        ) ELSE (
                            COPY %buildDir%\%%M !targetDir! 1> NUL
                            SET msg=Copied %%M^^!
                        )
                    ) ELSE (
                        SET msg=Could not find %buildDir%%%M^^!
                    )
                    ECHO         !msg!
                )
                SET done=1
            )
        )
        
        :: Show an error for unrecognized tokens
        IF !done!==0 (
            ECHO Parsing error on line !lineNum!: Unrecognized token "%%L"
            EXIT /B 4
        )
    )
    
)

ENDLOCAL
EXIT /B 0
