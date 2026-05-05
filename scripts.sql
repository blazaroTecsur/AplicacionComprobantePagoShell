
DELETE FROM rpoefectivo;
DELETE FROM rpopersonal;
DELETE FROM rposervicioprov;
DELETE FROM rposervicio;
DELETE FROM rposolicitud;


SELECT * FROM rpoefectivo;
SELECT * FROM rpopersonal;
SELECT * FROM rposervicioprov;
SELECT * FROM rposervicio;
SELECT * FROM rposolicitud;
SELECT * FROM rpoaprobador;
UPDATE rposolicitud SET CodDpto = '4001'

﻿
INSERT INTO rpogenerico (Tipo, Codigo, Descripcion, UsuarioReg, FechaReg)
VALUES ('PROVEEDOR', '20601088186', 'EULEN', 'DBO', NOW());
INSERT INTO rpogenerico (Tipo, Codigo, Descripcion, UsuarioReg, FechaReg)
VALUES ('PROVEEDOR', '20224088186', 'SEGUMAX', 'DBO', NOW());

INSERT INTO rpogenerico (Tipo, Codigo, Descripcion, UsuarioReg, FechaReg)
VALUES ('ESTADO', '01', 'INGRESADO', 'DBO', NOW());
INSERT INTO rpogenerico (Tipo, Codigo, Descripcion, UsuarioReg, FechaReg)
VALUES ('ESTADO', '02', 'APROBADO POR JEFATURA', 'DBO', NOW());
INSERT INTO rpogenerico (Tipo, Codigo, Descripcion, UsuarioReg, FechaReg)
VALUES ('ESTADO', '03', 'APROBADO POR CLIENTE', 'DBO', NOW());
INSERT INTO rpogenerico (Tipo, Codigo, Descripcion, UsuarioReg, FechaReg)
VALUES ('ESTADO', '05', 'ANULADA', 'DBO', NOW());
INSERT INTO rpogenerico (Tipo, Codigo, Descripcion, UsuarioReg, FechaReg)
VALUES ('FLUJO', '01', 'REGULAR', 'DBO', NOW());
INSERT INTO rpogenerico (Tipo, Codigo, Descripcion, UsuarioReg, FechaReg)
VALUES ('FLUJO', '02', 'URGENCIA', 'DBO', NOW());
INSERT INTO rpogenerico (Tipo, Codigo, Descripcion, UsuarioReg, FechaReg)
VALUES ('TIPO', '01', 'SOLICITUD', 'DBO', NOW());
INSERT INTO rpogenerico (Tipo, Codigo, Descripcion, UsuarioReg, FechaReg)
VALUES ('TIPO', '02', 'AMPLIACION', 'DBO', NOW());
INSERT INTO rpogenerico (Tipo, Codigo, Descripcion, UsuarioReg, FechaReg)
VALUES ('TIPO', '02', 'TRASLADO', 'DBO', NOW());
INSERT INTO rpogenerico (Tipo, Codigo, Descripcion, UsuarioReg, FechaReg)
VALUES ('SERVICIO', '01', 'RESGUARDO', 'DBO', NOW());
INSERT INTO rpogenerico (Tipo, Codigo, Descripcion, UsuarioReg, FechaReg)
VALUES ('SERVICIO', '02', 'PREVENCION VIAL', 'DBO', NOW());
INSERT INTO rpogenerico (Tipo, Codigo, Descripcion, UsuarioReg, FechaReg)
VALUES ('FOLIO', '00', '0', 'DBO', NOW());

CREATE TABLE  `rpogenerico` (
	`IdGenerico` INT NOT NULL AUTO_INCREMENT UNIQUE,
	`Tipo` VARCHAR(10) NOT NULL,
	`Codigo` VARCHAR(15) NOT NULL,
	`Descripcion` VARCHAR(50) NOT NULL,
	UsuarioReg VARCHAR(50) NOT NULL,
	FechaReg DATETIME NOT NULL,
	UsuarioAct VARCHAR(50),
	FechaAct DATETIME,
	PRIMARY KEY(`IdGenerico`)
);

CREATE TABLE  `rposolicitud` (
	`IdSolicitud` INT NOT NULL AUTO_INCREMENT UNIQUE,
	`Folio` VARCHAR(9) NOT NULL,
	`IdTipo` INT NOT NULL,	
	`IdEstado` INT NOT NULL,	
	`IdFlujo` INT NOT NULL,
	`NumSro` VARCHAR(10) NOT NULL,
	`CodDpto` VARCHAR(15) NOT NULL,
	`NomDpto` VARCHAR(50) NOT NULL,
	`CodActv` VARCHAR(15) NOT NULL,
	`NomActv` VARCHAR(50) NOT NULL,
	`CodSupr` VARCHAR(15) NOT NULL,
	`NomSupr` VARCHAR(50) NOT NULL,
	`RucSctta` VARCHAR(15) NOT NULL,
	`NomSctta` VARCHAR(50) NOT NULL,
	`CodCapataz` VARCHAR(15) NOT NULL,
	`NomCapataz` VARCHAR(50) NOT NULL,	
	`FechaFoc` DATETIME NOT NULL,		
	`Coordenada` VARCHAR(30) NOT NULL,	
	`Direccion` VARCHAR(100) NOT NULL,
	`Celular` VARCHAR(15) NOT NULL,
	`TpoTrabajo` VARCHAR(200) NOT NULL,
	`UsuarioApro` VARCHAR(50),
	`FechaApro` DATETIME,
	`Comentario` VARCHAR(200),
	`FolioRef` VARCHAR(10),
	UsuarioReg VARCHAR(50) NOT NULL,
	FechaReg DATETIME NOT NULL,
	UsuarioAct VARCHAR(50),
	FechaAct DATETIME,
	PRIMARY KEY(`IdSolicitud`)
);

ALTER TABLE `rposolicitud` ADD FOREIGN KEY(`IdTipo`) REFERENCES `rpogenerico`(`IdGenerico`);
ALTER TABLE `rposolicitud` ADD FOREIGN KEY(`IdEstado`) REFERENCES `rpogenerico`(`IdGenerico`);
ALTER TABLE `rposolicitud` ADD FOREIGN KEY(`IdFlujo`) REFERENCES `rpogenerico`(`IdGenerico`);

ALTER TABLE rposervicio ADD HraAmplia CHAR(5) NULL;

CREATE TABLE  `rposervicio` (
	`IdServicio` INT NOT NULL AUTO_INCREMENT UNIQUE,
	`IdSolicitud` INT NOT NULL,
	`IdTpoServicio` INT NOT NULL,
	`Fecha` DATE NOT NULL,
	Turno CHAR(1) NOT NULL,
	`HraInicio` CHAR(5) NOT NULL,
	`HraFinal` CHAR(5) NOT NULL,
	`DiaSig` BOOLEAN NOT NULL,
	`Coordenada` VARCHAR(50) NOT NULL,
	`Direccion` VARCHAR(200) NOT NULL,
	`Cantidad` INT NOT NULL,
	CantidadBck INT NULL,
	Comentario VARCHAR(100) NULL,		
	UsuarioReg VARCHAR(50) NOT NULL,
	FechaReg DATETIME NOT NULL,
	UsuarioAct VARCHAR(50),
	FechaAct DATETIME,
	PRIMARY KEY(`IdServicio`)
);

ALTER TABLE `rposervicio` ADD FOREIGN KEY(`IdSolicitud`) REFERENCES `rposolicitud`(`IdSolicitud`);
ALTER TABLE `rposervicio` ADD FOREIGN KEY(`IdTpoServicio`) REFERENCES `rpogenerico`(`IdGenerico`);


CREATE TABLE rposervicioprov ( 
IdServicioProv INT NOT NULL AUTO_INCREMENT UNIQUE,
IdServicio INT NOT NULL,
IdProveedor INT NOT NULL,
Cantidad INT NOT NULL,
Estado CHAR(2) NOT NULL
PRIMARY KEY(`IdServicioProv`)
);
ALTER TABLE `rposervicioprov` ADD FOREIGN KEY(`IdServicio`) REFERENCES `rposervicio`(`IdServicio`);
ALTER TABLE `rposervicioprov` ADD FOREIGN KEY(`IdProveedor`) REFERENCES `rpogenerico`(`IdGenerico`);

CREATE TABLE  `rpopersonal` (
	`IdPersonal` INT NOT NULL AUTO_INCREMENT UNIQUE,
	`Dni` VARCHAR(15) NOT NULL,
	`Nombres` VARCHAR(50) NOT NULL,
	`Apellidos` VARCHAR(50) NOT NULL,
	`Telefono` VARCHAR(15) NULL,
	UsuarioReg VARCHAR(50) NOT NULL,
	FechaReg DATETIME NOT NULL,
	UsuarioAct VARCHAR(50),
	FechaAct DATETIME,
	PRIMARY KEY(`IdPersonal`)
);

SELECT * FROM rposervicio
SELECT * FROM rpoefectivo
ALTER TABLE rpoefectivo DROP COLUMN AmpliaApro

CREATE TABLE  `rpoefectivo` (
	`IdEfectivo` INT NOT NULL AUTO_INCREMENT UNIQUE,
	`IdServicioProv` INT NOT NULL,
	`IdPersonal` INT NOT NULL,	
	`Telefono` VARCHAR(15),
	HraAmplia CHAR(5) NULL,
	EstAmplia CHAR(1),
	`UsuarioApro` VARCHAR(50) NULL,
	`FechaApro` DATETIME NULL,
	`ComentApro` VARCHAR(200) NULL,		
	Asistio BOOL NULL,
	`HraInicio` CHAR(5),
	`HraFinal` CHAR(5),
	SroRefencia VARCHAR(10) NULL,
	Comentario VARCHAR(300) NULL,
	UsuarioReg VARCHAR(50) NOT NULL,
	FechaReg DATETIME NOT NULL,
	UsuarioAct VARCHAR(50),
	FechaAct DATETIME,	
	PRIMARY KEY(`IdEfectivo`)
);

ALTER TABLE `rpoefectivo` ADD FOREIGN KEY(`IdServicioProv`) REFERENCES `rposervicioprov`(`IdServicioProv`);
ALTER TABLE `rpoefectivo` ADD FOREIGN KEY(`IdPersonal`) REFERENCES `rpopersonal`(`IdPersonal`);

SELECT * FROM rpoconfig;
DELETE FROM rpoconfig;
ALTER TABLE rpoconfig DROP COLUMN NomDpto;
ALTER TABLE rpoconfig DROP COLUMN Turno;
ALTER TABLE rpoconfig DROP COLUMN Cantidad;
ALTER TABLE rpoconfig ADD COLUMN Diurno INT NOT NULL;
ALTER TABLE rpoconfig ADD COLUMN Nocturno INT NOT NULL;

CREATE TABLE  `rpoconfig` (
	`IdConfig` INT NOT NULL AUTO_INCREMENT UNIQUE,
	`Fecha` DATE NOT NULL,
	`CodDpto` VARCHAR(15) NOT NULL,
	`IdTpoServicio` INT NOT NULL,
	`Diurno` INT NOT NULL,
	`Nocturno` INT NOT NULL,
	UsuarioReg VARCHAR(50) NOT NULL,
	FechaReg DATETIME NOT NULL,
	UsuarioAct VARCHAR(50),
	FechaAct DATETIME,
	PRIMARY KEY(`IdConfig`)
);

ALTER TABLE `rpoconfig` ADD FOREIGN KEY(`IdTpoServicio`) REFERENCES `rpogenerico`(`IdGenerico`);

CREATE TABLE  `rpoaprobador` (
	`IdAprobador` INT NOT NULL AUTO_INCREMENT UNIQUE,	
	`CodSocio` VARCHAR(15) NOT NULL,
	`NomSocio` VARCHAR(50) NOT NULL,
	`CodUnidad` VARCHAR(15) NOT NULL,
	`DscUnidad` VARCHAR(50) NOT NULL,
	UsuarioReg VARCHAR(50) NOT NULL,
	FechaReg DATETIME NOT NULL,
	UsuarioAct VARCHAR(50),
	FechaAct DATETIME,
	PRIMARY KEY(`IdAprobador`)
);
-- ALTER TABLE `rpoaprobador` ADD FOREIGN KEY(`IdCapataz`) REFERENCES `rposolicitud`(`IdCapataz`);
-- ALTER TABLE `rpoaprobador` ADD FOREIGN KEY(`IdSolicitante`) REFERENCES `rposolicitud`(`IdSolicitante`);


