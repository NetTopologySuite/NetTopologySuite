'******************************************************************************
'*
'* Name:     GdalConfiguration.vb.pp
'* Project:  GDAL VB.NET Interface
'* Purpose:  A static configuration utility class to enable GDAL/OGR.
'* Author:   Felix Obermaier
'*
'******************************************************************************
'* Copyright (c) 2012-2018, Felix Obermaier
'*
'* Permission is hereby granted, free of charge, to any person obtaining a
'* copy of this software and associated documentation files (the "Software"),
'* to deal in the Software without restriction, including without limitation
'* the rights to use, copy, modify, merge, publish, distribute, sublicense,
'* and/or sell copies of the Software, and to permit persons to whom the
'* Software is furnished to do so, subject to the following conditions:
'*
'* The above copyright notice and this permission notice shall be included
'* in all copies or substantial portions of the Software.
'*
'* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
'* OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
'* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
'* THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
'* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
'* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
'* DEALINGS IN THE SOFTWARE.
'*****************************************************************************/

Option Infer On

Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports Gdal = OSGeo.GDAL.Gdal
Imports Ogr = OSGeo.OGR.Ogr

Namespace NetTopologySuite.Reprojection.GDAL
    ''' <summary>
    ''' Configuration class for GDAL/OGR
    ''' </summary>
    Public Partial Class GdalConfiguration
        Private Shared _configuredOgr As Boolean
        Private Shared _configuredGdal As Boolean
        Private Shared _usable As Boolean

        <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Public Shared Function SetDefaultDllDirectories(ByVal directoryFlags As UInteger) As Boolean

        End Function

        '    LOAD_LIBRARY_SEARCH_USER_DIRS | LOAD_LIBRARY_SEARCH_SYSTEM32
        Private Const DllSearchFlags As UInteger = &H400 Or &H800

        <DllImport("kernel32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
        Public Shared Function AddDllDirectory(ByVal lpPathName As String) As <MarshalAs(UnmanagedType.Bool)> Boolean

        End Function

        ''' <summary>
        ''' Construction of Gdal/Ogr
        ''' </summary>
        Shared Sub New()
            Dim nativePath As String = Nothing
            Dim executingDirectory As String = Nothing
            Dim gdalPath As String = Nothing
            Try
                If Not IsWindows Then
                    Const notSet As String = "_Not_set_"
                    Dim tmp As String = Gdal.GetConfigOption("GDAL_DATA", notSet)
                    _usable = (tmp <> notSet)
                    Return
                End If

                Dim executingAssemblyFile As String = New Uri(Assembly.GetExecutingAssembly.GetName.CodeBase).LocalPath
                executingDirectory = Path.GetDirectoryName(executingAssemblyFile)
                If String.IsNullOrEmpty(executingDirectory) Then
                    Throw New InvalidOperationException("cannot get executing directory")
                End If

                SetDefaultDllDirectories(DllSearchFlags)
                gdalPath = Path.Combine(executingDirectory, "gdal")
                nativePath = Path.Combine(gdalPath, GetPlatform())
                If Not Directory.Exists(nativePath) Then
                    Throw New DirectoryNotFoundException($"GDAL native directory not found at '{nativePath}'")
                End If

                If Not File.Exists(Path.Combine(nativePath, "gdal_wrap.dll")) Then
                    Throw New FileNotFoundException($"GDAL native wrapper file not found at '{Path.Combine(nativePath, ", gdal_wrap.dll, ")}'")
                End If

                AddDllDirectory(nativePath)
                AddDllDirectory(Path.Combine(nativePath, "plugins"))
                ' Set the additional GDAL environment variables.
                Dim gdalData As String = Path.Combine(gdalPath, "data")
                Environment.SetEnvironmentVariable("GDAL_DATA", gdalData)
                Gdal.SetConfigOption("GDAL_DATA", gdalData)
                Dim driverPath As String = Path.Combine(nativePath, "plugins")
                Environment.SetEnvironmentVariable("GDAL_DRIVER_PATH", driverPath)
                Gdal.SetConfigOption("GDAL_DRIVER_PATH", driverPath)
                Environment.SetEnvironmentVariable("GEOTIFF_CSV", gdalData)
                Gdal.SetConfigOption("GEOTIFF_CSV", gdalData)
                Dim projSharePath As String = Path.Combine(gdalPath, "share")
                Environment.SetEnvironmentVariable("PROJ_LIB", projSharePath)
                Gdal.SetConfigOption("PROJ_LIB", projSharePath)
                _usable = True
            Catch e As Exception
                _usable = False
                Trace.WriteLine(e, "error")
                Trace.WriteLine($"Executing directory: {executingDirectory}", "error")
                Trace.WriteLine($"gdal directory: {gdalPath}", "error")
                Trace.WriteLine($"native directory: {nativePath}", "error")
                'throw;
            End Try

        End Sub

        ''' <summary>
        ''' Gets a value indicating if the GDAL package is set up properly.
        ''' </summary>
        Public Shared ReadOnly Property Usable As Boolean
            Get
                Return _usable
            End Get
        End Property
        
        ''' <summary>
        ''' Method to ensure the static constructor is being called.
        ''' </summary>
        ''' <remarks>Be sure to call this function before using Gdal/Ogr/Osr</remarks>
        Public Shared Sub ConfigureOgr()
            If Not _usable Then
                Return
            End If
            
            If _configuredOgr Then
                Return
            End If
            
            ' Register drivers
            Ogr.RegisterAll
            _configuredOgr = True
            PrintDriversOgr()
        End Sub
        
        ''' <summary>
        ''' Method to ensure the static constructor is being called.
        ''' </summary>
        ''' <remarks>Be sure to call this function before using Gdal/Ogr/Osr</remarks>
        Public Shared Sub ConfigureGdal()
            If Not _usable Then
                Return
            End If
            
            If _configuredGdal Then
                Return
            End If
            
            ' Register drivers
            Gdal.AllRegister
            _configuredGdal = True
            PrintDriversGdal()
        End Sub
        
        ''' <summary>
        ''' Function to determine which platform we're on
        ''' </summary>
        Private Shared Function GetPlatform() As String
            If (Environment.Is64BitProcess) Then Return "x64"
            return "x86"
        End Function
        
        ''' <summary>
        ''' Gets a value indicating if we are on a windows platform
        ''' </summary>
        Private Shared ReadOnly Property IsWindows As Boolean
            Get
                Dim res = Not ((Environment.OSVersion.Platform = PlatformID.Unix)  _
                            OrElse (Environment.OSVersion.Platform = PlatformID.MacOSX))
                Return res
            End Get
        End Property
        
        Private Shared Sub PrintDriversOgr()
            #if (DEBUG)
            If _usable Then
                Dim num = Ogr.GetDriverCount
                Dim i = 0
                Do While (i < num)
                    Dim driver = Ogr.GetDriver(i)
                    Trace.WriteLine($"OGR {i}: {driver.GetName()}", "Debug")
                    i = (i + 1)
                Loop
                
            End If
            
            #End If 
        End Sub
        
        Private Shared Sub PrintDriversGdal()
            #if (DEBUG)
            If _usable Then
                Dim num = Gdal.GetDriverCount
                Dim i = 0
                Do While (i < num)
                    Dim driver = Gdal.GetDriver(i)
                    Trace.WriteLine($"GDAL {i}: {driver.ShortName}-{driver.LongName}")
                    i = (i + 1)
                Loop
                
            End If
            
            #End If 
        End Sub
    End Class
End Namespace