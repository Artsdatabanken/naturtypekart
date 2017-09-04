SET QUOTED_IDENTIFIER ON
GO

-- ======================================================================
-- Description: <Index for rutenett geometris>
-- ======================================================================

IF EXISTS (SELECT * FROM sys.indexes WHERE NAME='Rutenett_Cell' AND object_id = OBJECT_ID('Rutenett')) BEGIN
  DROP INDEX Rutenett_Cell ON Rutenett;  
END

CREATE SPATIAL INDEX 
  Rutenett_Cell 
ON 
  Rutenett(geometri)
  USING GEOMETRY_GRID
WITH (
  BOUNDING_BOX = (
    xmin = -5000000,
    ymin = 2000000,
    xmax = 7000000,
    ymax = 20000000
  ), 
  GRIDS = (
    LEVEL_1 = HIGH,
    LEVEL_2 = MEDIUM,
    LEVEL_3 = MEDIUM,
    LEVEL_4 = MEDIUM
  ), 
  CELLS_PER_OBJECT = 16
);

-- ======================================================================
-- Description: <Index for nature geometris>
-- ======================================================================

IF EXISTS (SELECT * FROM sys.indexes WHERE NAME='Naturområde_Område' AND object_id = OBJECT_ID('Naturområde')) BEGIN
  DROP INDEX Naturområde_Område ON Naturområde;  
END

CREATE SPATIAL INDEX 
  Naturområde_Område 
ON 
  Naturområde(geometri)
  USING GEOMETRY_GRID
WITH (
  BOUNDING_BOX = (
    xmin = -5000000,
    ymin = 2000000,
    xmax = 7000000,
    ymax = 20000000
  ), 
  GRIDS = (
    LEVEL_1 = HIGH,
    LEVEL_2 = MEDIUM,
    LEVEL_3 = MEDIUM,
    LEVEL_4 = MEDIUM
  ), 
  CELLS_PER_OBJECT = 16
);

-- ======================================================================
-- Description: <Index for geometris>
-- ======================================================================

IF EXISTS (SELECT * FROM sys.indexes WHERE NAME='Område_Område' AND object_id = OBJECT_ID('Område')) BEGIN
  DROP INDEX Område_Område ON Område;  
END

CREATE SPATIAL INDEX 
  Område_Område 
ON 
  Område(geometri)
  USING GEOMETRY_GRID
WITH (
  BOUNDING_BOX = (
    xmin = -5000000,
    ymin = 2000000,
    xmax = 7000000,
    ymax = 20000000
  ), 
  GRIDS = (
    LEVEL_1 = HIGH,
    LEVEL_2 = MEDIUM,
    LEVEL_3 = MEDIUM,
    LEVEL_4 = MEDIUM
  ), 
  CELLS_PER_OBJECT = 16
);

-- ======================================================================
-- Description: <Index for nature geometri levels>
-- ======================================================================

IF EXISTS (SELECT * FROM sys.indexes WHERE NAME='Naturområde_Naturnivå_Id' AND object_id = OBJECT_ID('Naturområde')) BEGIN
  DROP INDEX Naturområde_Naturnivå_Id ON Naturområde;  
END

CREATE INDEX 
  Naturområde_Naturnivå_Id 
ON 
  Naturområde (naturnivå_id);
GO

-- ======================================================================
-- Description: <Index for nature geometri kartlagtOmråde id>
-- ====================================================================== 

IF EXISTS (SELECT * FROM sys.indexes WHERE NAME='Naturområde_KartlagtOmråde_Id' AND object_id = OBJECT_ID('Naturområde')) BEGIN
  DROP INDEX Naturområde_KartlagtOmråde_Id ON Naturområde;  
END

CREATE NONCLUSTERED INDEX 
  Naturområde_KartlagtOmråde_Id
ON
  Naturområde (kartlagtOmråde_id)
INCLUDE
  (id, localId);
GO

-- ======================================================================
-- Description: <Index for nature geometri type kodes>
-- ======================================================================

IF EXISTS (SELECT * FROM sys.indexes WHERE NAME='NaturområdeType_Code' AND object_id = OBJECT_ID('NaturområdeType')) BEGIN
  DROP INDEX NaturområdeType_Code ON NaturområdeType;  
END

CREATE INDEX 
  NaturområdeType_Code 
ON 
  NaturområdeType (kode);
GO

-- ======================================================================
-- Description: <Index for beskrivelse variable kodes>
-- ======================================================================

IF EXISTS (SELECT * FROM sys.indexes WHERE NAME='Beskrivelsesvariabel_Code' AND object_id = OBJECT_ID('Beskrivelsesvariabel')) BEGIN
  DROP INDEX Beskrivelsesvariabel_Code ON Beskrivelsesvariabel;  
END

CREATE INDEX 
  Beskrivelsesvariabel_Code 
ON 
  Beskrivelsesvariabel (kode);
GO

-- ======================================================================
-- Description: <Index for geometris>
-- ======================================================================

IF EXISTS (SELECT * FROM sys.indexes WHERE NAME='Område_OmrådeType_Id_Number' AND object_id = OBJECT_ID('Område')) BEGIN
  DROP INDEX Område_OmrådeType_Id_Number ON Område;  
END

CREATE INDEX 
  Område_OmrådeType_Id_Number 
ON 
  Område (geometriType_id, nummer);
GO
