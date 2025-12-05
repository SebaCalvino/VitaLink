-- ============================================================================
-- SCRIPT PARA CORREGIR SP sp_InsertEstudioManual
-- ============================================================================
-- Este SP corrige el problema de la modalidad que no existe
-- ============================================================================

USE [BDVitalink]
GO

ALTER PROCEDURE [dbo].[sp_InsertEstudioManual]
    @IdUsuario INT,
    @NombreEstudio VARCHAR(50),
    @Observacion VARCHAR(100) = NULL,
    @Fecha DATE,
    @Capacidad VARCHAR(50) = NULL,
    @FechaCreacionArchivo DATE = NULL,
    @NombreArchivo VARCHAR(50) = NULL,
    @TipoArchivo VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE 
            @IdOrganizacion INT,
            @IdEncuentro INT,
            @IdArchivo INT = NULL,
            @IdTipoDocumento INT,
            @IdEstudio INT,
            @IdModalidad INT;

        -- 1. Usamos una ORGANIZACIÓN genérica si existe
        SELECT @IdOrganizacion = Id 
        FROM Organizaciones 
        WHERE Nombre = 'Carga Manual';

        IF @IdOrganizacion IS NULL
        BEGIN
            -- Los tipos de organización ya están cargados en la BD
            -- Usar el primer tipo de organización disponible
            DECLARE @IdTipoOrg INT = (SELECT TOP 1 Id FROM Tipo_Organizacion ORDER BY Id);
            DECLARE @IdDir INT = (SELECT TOP 1 Id FROM Direcciones ORDER BY Id);
            
            -- Si no hay direcciones, crear una genérica
            IF @IdDir IS NULL
            BEGIN
                INSERT INTO Direcciones (Calle, Altura, CallesParalelas)
                VALUES ('Sin dirección', '0', '');
                SET @IdDir = SCOPE_IDENTITY();
            END
            
            INSERT INTO Organizaciones (Nombre, Id_Tipo_Organizacion, IdDireccion)
            VALUES ('Carga Manual', @IdTipoOrg, @IdDir); 
            SET @IdOrganizacion = SCOPE_IDENTITY();
        END

        -- 2. Crear ENCUENTRO manual para este estudio
        INSERT INTO Encuentros 
        (
            IdUsuario, IdOrganizacion, 
            NombreMedico, ApellidoMedico,
            FechaInicio, FechaFin,
            EstadoMotivo
        )
        VALUES 
        (
            @IdUsuario, @IdOrganizacion,
            'Carga', 'Manual',
            @Fecha, @Fecha,
            'Registro manual de estudio'
        );
        SET @IdEncuentro = SCOPE_IDENTITY();

        -- 3. Si hay archivo, insertarlo
        IF @NombreArchivo IS NOT NULL AND LEN(LTRIM(RTRIM(@NombreArchivo))) > 0
        BEGIN
            INSERT INTO Archivos (Capacidad, FechaCreacion)
            VALUES (ISNULL(@Capacidad, '0 KB'), ISNULL(@FechaCreacionArchivo, GETDATE()));
            SET @IdArchivo = SCOPE_IDENTITY();

            -- 4. Buscar o crear el tipo de documento 'Estudio'
            SELECT @IdTipoDocumento = Id
            FROM Tipo_Documento
            WHERE Tipo = 'Estudio';

            IF @IdTipoDocumento IS NULL
            BEGIN
                INSERT INTO Tipo_Documento (Tipo)
                VALUES ('Estudio');
                SET @IdTipoDocumento = SCOPE_IDENTITY();
            END

            -- 5. Insertar DOCUMENTO CLÍNICO vinculado al archivo
            INSERT INTO Documentos_Clinicos
            (
                IdEncuentro, Id_TipoDocumento, 
                Titulo, Fecha,
                IdArchivo, NombreArchivo, TipoArchivo
            )
            VALUES
            (
                @IdEncuentro, @IdTipoDocumento,
                @NombreEstudio, @Fecha,
                @IdArchivo, @NombreArchivo, @TipoArchivo
            );
        END

        -- 6. Obtener o crear una modalidad por defecto
        -- Primero intentar obtener la primera modalidad disponible
        SELECT TOP 1 @IdModalidad = Id FROM Modalidad ORDER BY Id;
        
        -- Si no existe ninguna modalidad, crear una por defecto
        IF @IdModalidad IS NULL
        BEGIN
            INSERT INTO Modalidad (Tipo_ImagenEstudio)
            VALUES ('Estudio General');
            SET @IdModalidad = SCOPE_IDENTITY();
        END

        -- 7. Insertar ESTUDIO DE IMAGEN vinculado
        -- Si IdArchivo es NULL, la columna debe permitir NULL
        IF @IdArchivo IS NULL
        BEGIN
            -- Si no hay archivo, insertar sin IdArchivo (asumiendo que la columna permite NULL)
            INSERT INTO Imagenes_Estudios
            (
                IdEncuentro, Id_Modalidad,
                Region, Fecha, Informe_Texto
            )
            VALUES
            (
                @IdEncuentro, 
                @IdModalidad,  -- Usar la modalidad obtenida o creada
                @NombreEstudio, 
                @Fecha, 
                ISNULL(@Observacion, '')
            );
        END
        ELSE
        BEGIN
            -- Si hay archivo, insertar con IdArchivo
            INSERT INTO Imagenes_Estudios
            (
                IdEncuentro, Id_Modalidad,
                Region, Fecha, Informe_Texto, IdArchivo
            )
            VALUES
            (
                @IdEncuentro, 
                @IdModalidad,  -- Usar la modalidad obtenida o creada
                @NombreEstudio, 
                @Fecha, 
                ISNULL(@Observacion, ''),
                @IdArchivo
            );
        END
        
        SET @IdEstudio = SCOPE_IDENTITY();

        COMMIT TRANSACTION;

        SELECT @IdEstudio AS IdEstudio;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK;

        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END
GO

