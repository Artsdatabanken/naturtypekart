
SET QUOTED_IDENTIFIER ON
GO

-- ======================================================================
-- Description: <Creates a new unique contact>
-- ======================================================================

IF OBJECT_ID('[createKontakt]', 'P') IS NOT NULL
	DROP PROCEDURE createKontakt;
GO

CREATE PROCEDURE createKontakt (
  @firmanavn NVARCHAR(MAX),
  @kontaktperson NVARCHAR(MAX),
  @epost NVARCHAR(MAX),
  @telefon NVARCHAR(MAX),
  @hjemmeside NVARCHAR(MAX)
)
AS
  BEGIN
    DECLARE @contact_id int;
    
    SELECT 
      @contact_id = id 
    FROM 
      Kontakt 
    WHERE 
      firmanavn = @firmanavn
    AND 
      kontaktperson = @kontaktperson OR (@kontaktperson IS NULL AND kontaktperson IS NULL)
    AND 
      epost = @epost OR (@epost IS NULL AND epost IS NULL)
    AND
      telefon = @telefon OR (@telefon IS NULL AND telefon IS NULL)
    AND
      hjemmeside = @hjemmeside OR (@hjemmeside IS NULL AND hjemmeside IS NULL)
    ;
    
    IF (@contact_id IS NULL) BEGIN
      DECLARE @id TABLE (id int);
      INSERT INTO Kontakt OUTPUT Inserted.id INTO @id VALUES(@firmanavn, @kontaktperson, @epost, @telefon, @hjemmeside);
      SELECT @contact_id = id FROM @id; 
    END
    
    RETURN(@contact_id);
  END;
GO

-- ======================================================================
-- Description: <Linking geometris to nature geometri>
-- ======================================================================


IF OBJECT_ID ('[linkOmrådes]', 'P') IS NOT NULL
  DROP PROCEDURE linkOmrådes;
GO

CREATE PROCEDURE linkOmrådes (
  @naturområdeId INT
) 
AS
  BEGIN
  
    DECLARE @geometri GEOMETRY;
    SELECT @geometri = geometri FROM Naturområde WHERE id = @naturområdeId; 
  
    DECLARE @geometriIds TABLE (
      geometriIndex INT IDENTITY(1,1),
      geometriId INT
    );
    
    INSERT INTO 
      @geometriIds
    SELECT 
      id 
    FROM 
      Område 
    WHERE
      geometri.STIntersects(@geometri) = 1;
      
    DECLARE @i INT;
    DECLARE @imax INT;
    DECLARE @geometriId INT;
    
    SELECT 
      @i = min(geometriIndex) - 1, 
      @imax = max(geometriIndex) 
    FROM 
      @geometriIds;
    
    WHILE @i < @imax
    BEGIN
      SELECT @i = @i + 1;
      
      SELECT 
        @geometriId = geometriId 
      FROM 
        @geometriIds 
      WHERE 
        geometriIndex = @i;
      
        INSERT INTO 
          OmrådeLink 
        VALUES (
          @geometriId, 
          @naturområdeId
        );
    END
  END
GO

-- ======================================================================
-- Description: <Relinking geometris to nature geometris>
-- ======================================================================

IF OBJECT_ID ('[relinkOmrådes]', 'P') IS NOT NULL
  DROP PROCEDURE relinkOmrådes;
GO

CREATE PROCEDURE relinkOmrådes (
  @geometritypeId INT
) 
AS
  BEGIN
    DECLARE @naturområdes TABLE (
      naturområdeIndex INT IDENTITY (1,1),
      naturområdeId INT,
      naturområdeOmråde GEOMETRY
    );
    
    DECLARE @geometriIds TABLE (
      geometriIndex INT IDENTITY(1,1),
      geometriId INT
    );
    
    INSERT INTO
      @naturområdes
    SELECT
      id,
      geometri
    FROM 
      Naturområde;
      
    DECLARE @i INT;
    DECLARE @imax INT;
    DECLARE @naturområdeId INT;
    DECLARE @naturområdeOmråde GEOMETRY;
    
    DECLARE @j INT;
    DECLARE @jmax INT;
    DECLARE @geometriId INT;
    
    SELECT 
      @i = min(naturområdeIndex) - 1, 
      @imax = max(naturområdeIndex) 
    FROM 
      @naturområdes;
    
    WHILE @i < @imax BEGIN
      SELECT @i = @i + 1;
      
      SELECT 
        @naturområdeId = naturområdeId,
        @naturområdeOmråde = naturområdeOmråde 
      FROM 
        @naturområdes 
      WHERE 
        naturområdeIndex = @i;
    
      DELETE FROM @geometriIds;
    
      INSERT INTO 
        @geometriIds
      SELECT 
        id 
      FROM 
        Område 
      WHERE
        geometriType_id = @geometritypeId
      AND
        geometri.STIntersects(@naturområdeOmråde) = 1;
            
      SELECT 
        @j = min(geometriIndex) - 1, 
        @jmax = max(geometriIndex) 
      FROM 
        @geometriIds;
      
      WHILE @j < @jmax BEGIN
        SELECT @j = @j + 1;
        
        SELECT 
          @geometriId = geometriId 
        FROM 
          @geometriIds 
        WHERE 
          geometriIndex = @i;
          
        INSERT INTO 
          OmrådeLink 
        VALUES (
          @geometriId, 
          @naturområdeId
        );
      END  
    END
  END
GO

-- ======================================================================
-- Description: <Merging parts of same geometris>
-- ======================================================================

IF OBJECT_ID ('[mergeOmrådes]', 'TR') IS NOT NULL
  DROP TRIGGER mergeOmrådes;
GO

CREATE TRIGGER mergeOmrådes ON Område INSTEAD OF INSERT AS
  BEGIN
    DECLARE @geometriTypeId INT;
    DECLARE @nummer INT;
    SELECT @geometriTypeId = geometriType_id, @nummer = nummer FROM Inserted;
    
    DECLARE @currentOmråde GEOMETRY;
    DECLARE @newOmråde GEOMETRY;
    SELECT 
      @currentOmråde = geometri 
    FROM 
      Område 
    WHERE 
      geometriType_id = @geometriTypeId
    AND 
      nummer = @nummer;
      
    IF (@currentOmråde IS NOT NULL) BEGIN
      SELECT
        @newOmråde = geometri
      FROM
        Inserted;
        
      UPDATE 
        Område 
      SET 
        geometri = @currentOmråde.STUnion(@newOmråde) 
      WHERE 
        geometriType_id = @geometriTypeId
      AND 
        nummer = @nummer;
    END
    ELSE BEGIN 
      INSERT INTO Område (geometriType_id, nummer, navn, kategori, geometri) SELECT geometriType_id, nummer, navn, kategori, geometri FROM Inserted;
    END
  END
GO

-- ======================================================================
-- Description: <Storing multiple geometris>
-- ======================================================================

IF OBJECT_ID('storeOmrådes', 'P') IS NOT NULL
  DROP PROCEDURE storeOmrådes;
GO

IF TYPE_ID('Område') IS NOT NULL
  DROP TYPE Område;
GO

CREATE TYPE Område AS TABLE(
  geometriType_id INT NOT NULL,
  nummer INT NOT NULL,
  navn NVARCHAR(MAX),
  kategori VARCHAR(16),
  geometri VARBINARY(MAX) NOT NULL,
  geometriEpsg INT NOT NULL
);
GO

CREATE PROCEDURE storeOmrådes (
  @geometris Område READONLY
)
AS
  BEGIN
  
    DECLARE @geometriType_id INT;
    DECLARE @nummer INT;
    DECLARE @navn NVARCHAR(MAX);
    DECLARE @kategori VARCHAR(16);
    DECLARE @geometri VARBINARY(MAX);
    DECLARE @geometriEpsg INT;
    
    DECLARE 
      geometriCursor 
    CURSOR LOCAL FAST_FORWARD FOR
      SELECT * FROM @geometris;
      
    OPEN geometriCursor;
    FETCH NEXT FROM geometriCursor INTO @geometriType_id, @nummer, @navn, @kategori, @geometri, @geometriEpsg;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
      INSERT INTO 
        Område (
          geometriType_id, 
          nummer, 
          navn,
          kategori, 
          geometri
        ) 
        VALUES (
          @geometriType_id, 
          @nummer, 
          @navn, 
          @kategori,
          geometry::STGeomFromWKB(
            @geometri,
            @geometriEpsg
          )
        );
      FETCH NEXT FROM geometriCursor INTO @geometriType_id, @nummer, @navn, @kategori, @geometri, @geometriEpsg;
    END
  END
GO

-- ======================================================================
-- Description: <Store geometri layer items>
-- ======================================================================

IF OBJECT_ID('storeOmrådekart', 'P') IS NOT NULL
  DROP PROCEDURE storeOmrådekart;
GO

IF TYPE_ID('Områdekart') IS NOT NULL
  DROP TYPE Områdekart;
GO

CREATE TYPE Områdekart AS TABLE(
  områdeKartType_id INT NOT NULL,
  geometri_id INT NOT NULL,
  trinn NVARCHAR(MAX) NOT NULL
);
GO

CREATE PROCEDURE storeOmrådekart (
  @områdeKartItems Områdekart READONLY
)
AS
  BEGIN   
    INSERT INTO 
      Områdekart (
        områdeKartType_id, 
        geometri_id, 
        trinn
      ) 
    SELECT 
      områdeKartType_id,
      geometri_id,
      trinn
    FROM 
      @områdeKartItems;
  END
GO

-- ======================================================================
-- Description: <Restore geometri layer items>
-- ======================================================================

IF OBJECT_ID('restoreOmrådekart', 'P') IS NOT NULL
  DROP PROCEDURE restoreOmrådekart;
GO

IF TYPE_ID('OmrådekartRestore') IS NOT NULL
  DROP TYPE OmrådekartRestore;
GO

CREATE TYPE OmrådekartRestore AS TABLE(
  områdeKartType_id INT NOT NULL,
  geometriType_id INT NOT NULL,
  nummer INT NOT NULL,
  trinn NVARCHAR(MAX) NOT NULL
);
GO

CREATE PROCEDURE restoreOmrådekart (
  @områdeKartItems OmrådekartRestore READONLY
)
AS
  BEGIN
  
    DECLARE @områdeKartType_id INT;
    DECLARE @geometriType_id INT;
    DECLARE @nummer INT;
    DECLARE @trinn NVARCHAR(MAX);
    
    DECLARE 
      områdeKartCursor 
    CURSOR LOCAL FAST_FORWARD FOR
      SELECT * FROM @områdeKartItems;
      
    OPEN områdeKartCursor;
    FETCH NEXT FROM områdeKartCursor INTO @områdeKartType_id, @geometriType_id, @nummer, @trinn;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
      DECLARE @geometri_id INT;
      
      SELECT 
        @geometri_id = id
      FROM 
        Område 
      WHERE 
        geometriType_id = @geometriType_id
      AND 
        nummer = @nummer;       
    
      IF (@geometri_id IS NOT NULL) BEGIN
        INSERT INTO 
          Områdekart (
            områdeKartType_id, 
            geometri_id, 
            trinn
          ) 
          VALUES (
            @områdeKartType_id, 
            @geometri_id, 
            @trinn
          );
      END;
      FETCH NEXT FROM områdeKartCursor INTO @områdeKartType_id, @geometriType_id, @nummer, @trinn;
    END
  END
GO

-- ======================================================================
-- Description: <Store rutenett geometris>
-- ======================================================================

IF OBJECT_ID('storeRutenett', 'P') IS NOT NULL
  DROP PROCEDURE storeRutenett;
GO

IF TYPE_ID('Rutenett') IS NOT NULL
  DROP TYPE Rutenett;
GO

CREATE TYPE Rutenett AS TABLE(
  rutenettype_id INT NOT NULL,
  geometrieId VARCHAR(32) NOT NULL,
  geometri VARBINARY(MAX) NOT NULL,
  geometriEpsg INT NOT NULL
);
GO

CREATE PROCEDURE storeRutenett (
  @rutenettCells Rutenett READONLY
)
AS
  BEGIN   
    INSERT INTO 
      Rutenett (
        rutenettype_id, 
        geometrieId, 
        geometri
      ) 
    SELECT 
      rutenettype_id,
      geometrieId,
      geometry::STGeomFromWKB(
        geometri,
        geometriEpsg
      )
    FROM 
      @rutenettCells;
  END
GO

-- ======================================================================
-- Description: <Store rutenett layer geometris>
-- ======================================================================

IF OBJECT_ID('storeRutenettkart', 'P') IS NOT NULL
  DROP PROCEDURE storeRutenettkart;
GO

IF TYPE_ID('Rutenettkart') IS NOT NULL
  DROP TYPE Rutenettkart;
GO

CREATE TYPE Rutenettkart AS TABLE(
  rutenettkartType_id INT NOT NULL,
  rutenett_id INT NOT NULL,
  trinn NVARCHAR(MAX) NOT NULL
);
GO

CREATE PROCEDURE storeRutenettkart (
  @rutenettkartCells Rutenettkart READONLY
)
AS
  BEGIN   
    INSERT INTO 
      Rutenettkart (
        rutenettkartType_id, 
        rutenett_id, 
        trinn
      ) 
    SELECT 
      rutenettkartType_id,
      rutenett_id,
      trinn
    FROM 
      @rutenettkartCells;
  END
GO

-- ======================================================================
-- Description: <Store custom rutenett layer geometris>
-- ======================================================================

IF OBJECT_ID('storeCustomRutenettkart', 'P') IS NOT NULL
  DROP PROCEDURE storeCustomRutenettkart;
GO

IF TYPE_ID('CustomRutenettkart') IS NOT NULL
  DROP TYPE CustomRutenettkart;
GO

CREATE TYPE CustomRutenettkart AS TABLE(
  geometrieId VARCHAR(32) NOT NULL,
  geometri VARBINARY(MAX) NOT NULL,
  geometriEpsg INT NOT NULL,
  trinn NVARCHAR(MAX) NOT NULL
);
GO

CREATE PROCEDURE storeCustomRutenettkart (
  @rutenettkartType_id INT,
  @customRutenettkartCells CustomRutenettkart READONLY
)
AS
  BEGIN   
    
    DECLARE @geometrieId VARCHAR(32);
    DECLARE @geometri VARBINARY(MAX);
    DECLARE @geometriEpsg INT;
    DECLARE @trinn NVARCHAR(MAX);
    
    DECLARE 
      customRutenettkartCellCursor 
    CURSOR LOCAL FAST_FORWARD FOR
      SELECT * FROM @customRutenettkartCells;
      
    OPEN customRutenettkartCellCursor;
    FETCH NEXT FROM customRutenettkartCellCursor INTO @geometrieId, @geometri, @geometriEpsg, @trinn;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
    
      DECLARE @rutenett_id TABLE (id INT);
    
      INSERT INTO 
        Rutenett (
          rutenettype_id, 
          geometrieId, 
          geometri
        )
        OUTPUT Inserted.id INTO @rutenett_id
        VALUES (
          0, 
          @geometrieId,  
          geometry::STGeomFromWKB(
            @geometri,
            @geometriEpsg
          )
        );
        
        INSERT INTO 
          Rutenettkart (
            rutenettkartType_id,
            rutenett_id,
            trinn
          )
          VALUES (
            @rutenettkartType_id,
            (SELECT id FROM @rutenett_id),
            @trinn
          );
        
      FETCH NEXT FROM customRutenettkartCellCursor INTO @geometrieId, @geometri, @geometriEpsg, @trinn;
    END
  END
GO

-- ======================================================================
-- Description: <Get nature geometri type kodes>
-- ======================================================================

IF OBJECT_ID('getNaturområdeTypeCodes', 'P') IS NOT NULL
  DROP PROCEDURE getNaturområdeTypeCodes;
GO

IF TYPE_ID('Ids') IS NOT NULL
  DROP TYPE Ids;
GO

CREATE TYPE Ids AS TABLE(
  Id INT NOT NULL
);
GO

CREATE PROCEDURE getNaturområdeTypeCodes (
  @ids Ids READONLY
)
AS
  BEGIN
      SELECT 
        naturområde_id, 
        kode 
      FROM 
        NaturområdeType 
      WHERE 
        naturområde_id 
      IN 
      (
        SELECT 
          id 
        FROM 
          @Ids
      )
      ORDER BY
        naturområde_id;
  END
GO