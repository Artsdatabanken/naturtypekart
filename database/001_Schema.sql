CREATE TABLE Område ( 
	id int identity(1,1)  NOT NULL,
	geometriType_id int NOT NULL,
	nummer int,
	navn nvarchar(max),
	kategori varchar(16),
	geometri geometry NOT NULL
);

CREATE TABLE Områdekart ( 
	id int identity(1,1)  NOT NULL,
	områdeKartType_id int NOT NULL,
	geometri_id int NOT NULL,
	trinn nvarchar(max) NOT NULL
);

CREATE TABLE OmrådekartType ( 
	id int identity(1,1)  NOT NULL,
	doc_guid uniqueidentifier NOT NULL,
	navn nvarchar(max) NOT NULL,
	koderegister nvarchar(max) NOT NULL,
	kodeversjon nvarchar(max) NOT NULL,
	kode varchar(16) NOT NULL,
	minimumsverdi nvarchar(max),
	maksimumsverdi nvarchar(max),
	beskrivelse nvarchar(max),
	etablert datetime NOT NULL,
	eier_id int NOT NULL
);

CREATE TABLE OmrådeLink ( 
	geometri_id int NOT NULL,
	naturområde_id int NOT NULL
);

CREATE TABLE OmrådeType ( 
	id int NOT NULL,
	type nvarchar(max) NOT NULL
);

CREATE TABLE Kontakt ( 
	id int identity(1,1)  NOT NULL,
	firmanavn nvarchar(max) NOT NULL,
	kontaktperson nvarchar(max),
	epost nvarchar(max),
	telefon nvarchar(max),
	hjemmeside nvarchar(max)
);

CREATE TABLE EgendefinertVariabel ( 
	id int identity(1,1)  NOT NULL,
	naturområdetype_id int NOT NULL,
	spesifikasjon nvarchar(max) NOT NULL,
	trinn nvarchar(max) NOT NULL
);

CREATE TABLE EgendefinertVariabelDefinisjon ( 
	id int identity(1,1)  NOT NULL,
	kartlagtOmråde_id int NOT NULL,
	spesifikasjon nvarchar(max) NOT NULL,
	beskrivelse nvarchar(max) NOT NULL
);

CREATE TABLE Dataleveranse ( 
	id int identity(1,1)  NOT NULL,
	doc_id nvarchar(255) NOT NULL,
	navn nvarchar(max) NOT NULL,
	leveranseDato datetime NOT NULL,
	operatør_id int NOT NULL,
	begrunnelseForEndring nvarchar(max),
	beskrivelse nvarchar(max),
	parent_id nvarchar(255),
	opprettet datetime
);

CREATE TABLE Beskrivelsesvariabel ( 
	id int identity(1,1)  NOT NULL,
	naturområde_id int,
	naturområdetype_id int,
	tematiskId nvarchar(max),
	kode varchar(16) NOT NULL,
	kartlegger_id int,
	kartlagt datetime,
	trinn nvarchar(max) NOT NULL,
	beskrivelse nvarchar(max),
	opprinneligKode varchar(16) NOT NULL,
  totalklassifikasjonskvalitet INT NOT NULL	
);

CREATE TABLE Dokument ( 
	id int identity(1,1)  NOT NULL,
	doc_guid uniqueidentifier NOT NULL,
	kartlagtOmråde_id int,
	naturområde_id int,
	områdeKartType_id int,
	rutenettkartType_id int,
	tittel nvarchar(max),
	beskrivelse nvarchar(max),
	kartlegger_id int,
	filsti nvarchar(max) NOT NULL
);

CREATE TABLE Rutenett ( 
	id int identity(1,1)  NOT NULL,
	rutenettype_id int,
	geometrieId varchar(32) NOT NULL,
	geometri geometry NOT NULL
);

CREATE TABLE Rutenettkart ( 
	id int identity(1,1)  NOT NULL,
	rutenettkartType_id int NOT NULL,
	rutenett_id int NOT NULL,
	trinn nvarchar(max) NOT NULL
);

CREATE TABLE RutenettkartType ( 
	id int identity(1,1)  NOT NULL,
	doc_guid uniqueidentifier NOT NULL,
	navn nvarchar(max) NOT NULL,
	koderegister nvarchar(max) NOT NULL,
	kodeversjon nvarchar(max) NOT NULL,
	kode varchar(16) NOT NULL,
	minimumsverdi nvarchar(max),
	maksimumsverdi nvarchar(max),
	beskrivelse nvarchar(max),
	etablert datetime NOT NULL,
	eier_id int NOT NULL
);

CREATE TABLE Rutenettype ( 
	id int NOT NULL,
	type nvarchar(max) NOT NULL
);

CREATE TABLE KartlagtOmråde ( 
	id int identity(1,1)  NOT NULL,
	dataleveranse_id int NOT NULL,
	localId uniqueidentifier,
	navnerom nvarchar(max) NOT NULL,
	versjonId nvarchar(max),
	opprinneligReferansesystem nvarchar(max),
	program nvarchar(max) NOT NULL,
	prosjekt nvarchar(max) NOT NULL,
	prosjektbeskrivelse nvarchar(max),
	formål nvarchar(max),
	oppdragsgiver_id int NOT NULL,
	eier_id int NOT NULL,
	kartlagtFraDato datetime NOT NULL,
	kartlagtTilDato datetime NOT NULL,
	kartleggingsmålestokk nvarchar(max) NOT NULL,
	oppløsning nvarchar(max),
	geometri geometry NOT NULL,
	målemetode nvarchar(max) NOT NULL,
	nøyaktighet int,
	visibility nvarchar(max),
	målemetodeHøyde nvarchar(max),
	nøyaktighetHøyde int,
	maksimaltAvvik int,
	bruksbegrensning int NOT NULL,
  totalklassifikasjonskvalitet INT NOT NULL	
);

CREATE TABLE Naturområde ( 
	id int identity(1,1)  NOT NULL,
	kartlagtOmråde_id int NOT NULL,
	localId uniqueidentifier,
	navnerom nvarchar(max) NOT NULL,
	versjonId nvarchar(max),
	versjon nvarchar(max) NOT NULL,
	naturnivå_id int NOT NULL,
	geometri geometry NOT NULL,
	geometriSenterpunkt geometry NOT NULL,
	kartlegger_id int,
	kartlagt datetime,
	beskrivelse nvarchar(max) NOT NULL,
	institusjon nvarchar(max) NOT NULL,
	stedfestingskvalitet int NOT NULL
);

CREATE TABLE NaturområdeType ( 
	id int identity(1,1)  NOT NULL,
	naturområde_id int NOT NULL,
	tematiskId nvarchar(max),
	kode varchar(16) NOT NULL,
	kartlegger_id int,
	kartlagt datetime,
	andel float NOT NULL,
  totalklassifikasjonskvalitet INT NOT NULL	
);

CREATE TABLE Naturnivå ( 
	id int NOT NULL,
	type nvarchar(max) NOT NULL
);

CREATE TABLE Standardvariabel ( 
	id int identity(1,1)  NOT NULL,
	kartlagtOmråde_id int NOT NULL,
	koderegister nvarchar(max) NOT NULL,
	kodeversjon nvarchar(max) NOT NULL,
	kode varchar(16) NOT NULL
);


ALTER TABLE Område
	ADD CONSTRAINT UQ_Område_id UNIQUE (id);

ALTER TABLE OmrådeType
	ADD CONSTRAINT UQ_OmrådeType_id UNIQUE (id);

ALTER TABLE Kontakt
	ADD CONSTRAINT UQ_Kontakt_id UNIQUE (id);

ALTER TABLE EgendefinertVariabel
	ADD CONSTRAINT UQ_EgendefinertVariabel_id UNIQUE (id);

ALTER TABLE EgendefinertVariabelDefinisjon
	ADD CONSTRAINT UQ_EgendefinertVariabelDefinisjon_id UNIQUE (id);

ALTER TABLE Dataleveranse
	ADD CONSTRAINT UQ_Dataleveranse_id UNIQUE (id);

ALTER TABLE Beskrivelsesvariabel
	ADD CONSTRAINT UQ_Beskrivelsesvariabel_id UNIQUE (id);

ALTER TABLE Dokument
	ADD CONSTRAINT UQ_Dokument_id UNIQUE (id);

ALTER TABLE KartlagtOmråde
	ADD CONSTRAINT UQ_KartlagtOmråde_id UNIQUE (id);

ALTER TABLE NaturområdeType
	ADD CONSTRAINT UQ_NaturområdeType_id UNIQUE (id);

ALTER TABLE Naturnivå
	ADD CONSTRAINT UQ_Naturnivå_id UNIQUE (id);

ALTER TABLE Standardvariabel
	ADD CONSTRAINT UQ_Standardvariabel_id UNIQUE (id);

ALTER TABLE Område ADD CONSTRAINT PK_Område 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE Områdekart ADD CONSTRAINT PK_Områdekart 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE OmrådekartType ADD CONSTRAINT PK_OmrådekartType 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE OmrådeLink ADD CONSTRAINT PK_OmrådeLink 
	PRIMARY KEY CLUSTERED (geometri_id, naturområde_id);

ALTER TABLE OmrådeType ADD CONSTRAINT PK_OmrådeType 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE Kontakt ADD CONSTRAINT PK_Kontakt 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE EgendefinertVariabel ADD CONSTRAINT PK_EgendefinertVariabel 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE EgendefinertVariabelDefinisjon ADD CONSTRAINT PK_EgendefinertVariabelDefinisjon 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE Dataleveranse ADD CONSTRAINT PK_Dataleveranse 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE Beskrivelsesvariabel ADD CONSTRAINT PK_Beskrivelsesvariabel 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE Dokument ADD CONSTRAINT PK_Dokument 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE Rutenett ADD CONSTRAINT PK_Rutenett 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE Rutenettkart ADD CONSTRAINT PK_Rutenettkart 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE RutenettkartType ADD CONSTRAINT PK_RutenettkartType 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE Rutenettype ADD CONSTRAINT PK_Rutenettype 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE KartlagtOmråde ADD CONSTRAINT PK_KartlagtOmråde 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE Naturområde ADD CONSTRAINT PK_Naturområde 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE NaturområdeType ADD CONSTRAINT PK_NaturområdeType 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE Naturnivå ADD CONSTRAINT PK_Naturnivå 
	PRIMARY KEY CLUSTERED (id);

ALTER TABLE Standardvariabel ADD CONSTRAINT PK_Standardvariabel 
	PRIMARY KEY CLUSTERED (id);



ALTER TABLE Område ADD CONSTRAINT FK_Område_OmrådeType 
	FOREIGN KEY (geometriType_id) REFERENCES OmrådeType (id);

ALTER TABLE Områdekart ADD CONSTRAINT FK_Områdekart_Område 
	FOREIGN KEY (geometri_id) REFERENCES Område (id);

ALTER TABLE Områdekart ADD CONSTRAINT FK_Områdekart_OmrådekartType 
	FOREIGN KEY (områdeKartType_id) REFERENCES OmrådekartType (id);

ALTER TABLE OmrådekartType ADD CONSTRAINT FK_OmrådekartType_Kontakt 
	FOREIGN KEY (eier_id) REFERENCES Kontakt (id);

ALTER TABLE OmrådeLink ADD CONSTRAINT FK_OmrådeLink_Område 
	FOREIGN KEY (geometri_id) REFERENCES Område (id)
	ON DELETE CASCADE;

ALTER TABLE OmrådeLink ADD CONSTRAINT FK_OmrådeLink_Naturområde 
	FOREIGN KEY (naturområde_id) REFERENCES Naturområde (id)
	ON DELETE CASCADE;

ALTER TABLE EgendefinertVariabel ADD CONSTRAINT FK_EgendefinertVariabel_NaturområdeType 
	FOREIGN KEY (naturområdetype_id) REFERENCES NaturområdeType (id)
	ON DELETE CASCADE;

ALTER TABLE EgendefinertVariabelDefinisjon ADD CONSTRAINT FK_EgendefinertVariabelDefinisjon_KartlagtOmråde 
	FOREIGN KEY (kartlagtOmråde_id) REFERENCES KartlagtOmråde (id)
	ON DELETE CASCADE;

ALTER TABLE Dataleveranse ADD CONSTRAINT FK_Dataleveranse_Kontakt 
	FOREIGN KEY (operatør_id) REFERENCES Kontakt (id);

ALTER TABLE Beskrivelsesvariabel ADD CONSTRAINT FK_Beskrivelsesvariabel_Kontakt 
	FOREIGN KEY (kartlegger_id) REFERENCES Kontakt (id);

ALTER TABLE Beskrivelsesvariabel ADD CONSTRAINT FK_Beskrivelsesvariabel_Naturområde 
	FOREIGN KEY (naturområde_id) REFERENCES Naturområde (id)
	ON DELETE CASCADE;

ALTER TABLE Beskrivelsesvariabel ADD CONSTRAINT FK_Beskrivelsesvariabel_NaturområdeType 
	FOREIGN KEY (naturområdetype_id) REFERENCES NaturområdeType (id)
	ON DELETE CASCADE;

ALTER TABLE Dokument ADD CONSTRAINT FK_Dokument_OmrådekartType 
	FOREIGN KEY (områdeKartType_id) REFERENCES OmrådekartType (id)
	ON DELETE CASCADE;

ALTER TABLE Dokument ADD CONSTRAINT FK_Dokument_RutenettkartType 
	FOREIGN KEY (rutenettkartType_id) REFERENCES RutenettkartType (id)
	ON DELETE CASCADE;

ALTER TABLE Dokument ADD CONSTRAINT FK_Dokument_Kontakt 
	FOREIGN KEY (kartlegger_id) REFERENCES Kontakt (id);

ALTER TABLE Dokument ADD CONSTRAINT FK_Dokument_KartlagtOmråde 
	FOREIGN KEY (kartlagtOmråde_id) REFERENCES KartlagtOmråde (id)
	ON DELETE CASCADE;

ALTER TABLE Dokument ADD CONSTRAINT FK_Dokument_Naturområde 
	FOREIGN KEY (naturområde_id) REFERENCES Naturområde (id)
	ON DELETE CASCADE;

ALTER TABLE Rutenett ADD CONSTRAINT FK_Rutenett_Rutenettype 
	FOREIGN KEY (rutenettype_id) REFERENCES Rutenettype (id);

ALTER TABLE Rutenettkart ADD CONSTRAINT FK_Rutenettkart_Rutenett 
	FOREIGN KEY (rutenett_id) REFERENCES Rutenett (id);

ALTER TABLE Rutenettkart ADD CONSTRAINT FK_Rutenettkart_RutenettkartType 
	FOREIGN KEY (rutenettkartType_id) REFERENCES RutenettkartType (id);

ALTER TABLE RutenettkartType ADD CONSTRAINT FK_RutenettkartType_Kontakt 
	FOREIGN KEY (eier_id) REFERENCES Kontakt (id);

ALTER TABLE KartlagtOmråde ADD CONSTRAINT FK_KartlagtOmrådeOwner_Kontakt 
	FOREIGN KEY (eier_id) REFERENCES Kontakt (id);

ALTER TABLE KartlagtOmråde ADD CONSTRAINT FK_KartlagtOmråde_Dataleveranse 
	FOREIGN KEY (dataleveranse_id) REFERENCES Dataleveranse (id)
	ON DELETE CASCADE;

ALTER TABLE Naturområde ADD CONSTRAINT FK_Naturområde_KartlagtOmråde 
	FOREIGN KEY (kartlagtOmråde_id) REFERENCES KartlagtOmråde (id);

ALTER TABLE Naturområde ADD CONSTRAINT FK_Naturområde_Naturnivå 
	FOREIGN KEY (naturnivå_id) REFERENCES Naturnivå (id);

ALTER TABLE Naturområde ADD CONSTRAINT FK_Naturområde_Kontakt 
	FOREIGN KEY (kartlegger_id) REFERENCES Kontakt (id);

ALTER TABLE NaturområdeType ADD CONSTRAINT FK_NaturområdeType_Kontakt 
	FOREIGN KEY (kartlegger_id) REFERENCES Kontakt (id);

ALTER TABLE NaturområdeType ADD CONSTRAINT FK_NaturområdeType_Naturområdea 
	FOREIGN KEY (naturområde_id) REFERENCES Naturområde (id);

ALTER TABLE Standardvariabel ADD CONSTRAINT FK_Standardvariabel_KartlagtOmråde 
	FOREIGN KEY (kartlagtOmråde_id) REFERENCES KartlagtOmråde (id)
	ON DELETE CASCADE;
