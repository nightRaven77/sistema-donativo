USE [master]
GO
/****** Object:  Database [DonativosDB]    Script Date: 18/10/2026 03:54:06 p. m. ******/
CREATE DATABASE [DonativosDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'DonativosDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\DonativosDB.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'DonativosDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\DonativosDB_log.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
ALTER DATABASE [DonativosDB] SET COMPATIBILITY_LEVEL = 140
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [DonativosDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [DonativosDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DonativosDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DonativosDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DonativosDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DonativosDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [DonativosDB] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [DonativosDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DonativosDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DonativosDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DonativosDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DonativosDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DonativosDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DonativosDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DonativosDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DonativosDB] SET  DISABLE_BROKER 
GO
ALTER DATABASE [DonativosDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DonativosDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DonativosDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DonativosDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DonativosDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DonativosDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [DonativosDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [DonativosDB] SET RECOVERY FULL 
GO
ALTER DATABASE [DonativosDB] SET  MULTI_USER 
GO
ALTER DATABASE [DonativosDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DonativosDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [DonativosDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [DonativosDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [DonativosDB] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [DonativosDB] SET QUERY_STORE = OFF
GO
USE [DonativosDB]
GO
/****** Object:  User [userDonativos]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE USER [userDonativos] WITHOUT LOGIN WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [userDonativos]
GO
/****** Object:  Table [dbo].[donadores]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[donadores](
	[id_donador] [int] IDENTITY(1,1) NOT NULL,
	[codigo] [nvarchar](50) NOT NULL,
	[tipo] [nvarchar](20) NOT NULL,
	[nombre] [nvarchar](300) NOT NULL,
	[alias] [nvarchar](200) NULL,
	[rfc] [nvarchar](13) NULL,
	[contacto_principal] [nvarchar](200) NULL,
	[telefono] [nvarchar](20) NULL,
	[email] [nvarchar](100) NULL,
	[sitio_web] [nvarchar](200) NULL,
	[direccion] [nvarchar](500) NULL,
	[colonia] [nvarchar](100) NULL,
	[ciudad] [nvarchar](100) NULL,
	[estado] [nvarchar](50) NULL,
	[codigo_postal] [nvarchar](10) NULL,
	[pais] [nvarchar](50) NULL,
	[fecha_creacion] [date] NULL,
	[fecha_ultima_donacion] [date] NULL,
	[total_donativos] [int] NULL,
	[monto_total_donado] [decimal](15, 2) NULL,
	[notas] [nvarchar](2000) NULL,
	[fecha_actualizacion] [datetime2](3) NULL,
	[activo] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_donador] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[donativos]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[donativos](
	[id_donativo] [int] IDENTITY(1,1) NOT NULL,
	[folio] [nvarchar](50) NOT NULL,
	[id_donador] [int] NOT NULL,
	[fecha_recepcion] [date] NOT NULL,
	[tipo] [nvarchar](20) NOT NULL,
	[estatus] [nvarchar](20) NOT NULL,
	[observaciones] [nvarchar](2000) NULL,
	[id_usuario_registro] [int] NULL,
	[fecha_registro] [datetime2](3) NULL,
	[fecha_actualizacion] [datetime2](3) NULL,
	[id_almacen] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id_donativo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UQ__donativo__E8F12C9F99B04818] UNIQUE NONCLUSTERED 
(
	[folio] ASC,
	[estatus] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[contactos_donadores]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[contactos_donadores](
	[id_contacto] [int] IDENTITY(1,1) NOT NULL,
	[id_donador] [int] NOT NULL,
	[nombre] [nvarchar](200) NOT NULL,
	[puesto] [nvarchar](100) NULL,
	[telefono] [nvarchar](20) NULL,
	[extension] [nvarchar](10) NULL,
	[celular] [nvarchar](20) NULL,
	[email] [nvarchar](100) NULL,
	[horario_contacto] [nvarchar](100) NULL,
	[medio_preferido] [nvarchar](50) NULL,
	[es_principal] [bit] NULL,
	[notas] [nvarchar](500) NULL,
	[activo] [bit] NULL,
	[fecha_creacion] [datetime2](3) NULL,
	[fecha_actualizacion] [datetime2](3) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_contacto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_donadores_estadisticas]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================
-- VISTAS �TILES (ACTUALIZADAS)
-- ============================================

-- Vista: Donadores con estad�sticas
CREATE VIEW [dbo].[vw_donadores_estadisticas] AS
SELECT 
    d.id_donador,
    d.codigo,
    d.tipo,
    d.nombre,
    d.rfc,
    d.contacto_principal,
    d.telefono,
    d.email,
    d.fecha_registro,
    d.fecha_ultima_donacion,
    d.total_donativos,
    d.monto_total_donado,
    
    -- Estad�sticas recientes
    (SELECT COUNT(*) 
     FROM donativos dn 
     WHERE dn.id_donador = d.id_donador 
     AND YEAR(dn.fecha) = YEAR(GETDATE())) as donativos_este_anio,
    
    (SELECT SUM(dn.monto_efectivo)
     FROM donativos dn
     WHERE dn.id_donador = d.id_donador
     AND dn.estatus = 'recibido'
     AND YEAR(dn.fecha) = YEAR(GETDATE())) as monto_este_anio,
    
    -- Contactos principales
    (SELECT STRING_AGG(c.nombre, ', ') 
     FROM contactos_donadores c 
     WHERE c.id_donador = d.id_donador 
     AND c.es_principal = 1) as contactos_principales,
    
    -- �ltimo donativo
    (SELECT TOP 1 dn.folio 
     FROM donativos dn 
     WHERE dn.id_donador = d.id_donador 
     ORDER BY dn.fecha DESC) as ultimo_folio,
    
    (SELECT TOP 1 dn.fecha 
     FROM donativos dn 
     WHERE dn.id_donador = d.id_donador 
     ORDER BY dn.fecha DESC) as ultima_fecha_donativo
    
FROM donadores d;
GO
/****** Object:  Table [dbo].[donativos_detalle]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[donativos_detalle](
	[id_donativo_detalle] [int] IDENTITY(1,1) NOT NULL,
	[id_donativo] [int] NOT NULL,
	[id_articulo] [int] NOT NULL,
	[cantidad_recibida] [decimal](10, 2) NULL,
	[valor_unitario] [decimal](12, 2) NULL,
	[estado] [nvarchar](50) NULL,
	[descripcion] [nvarchar](1000) NULL,
	[unidad_medida] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK__donativo__4F1332DE4D4CF2FC] PRIMARY KEY CLUSTERED 
(
	[id_donativo_detalle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_donativos_completos]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Vista: Donativos con informaci�n de donador
CREATE VIEW [dbo].[vw_donativos_completos] AS
SELECT 
    d.id_donativo,
    d.folio,
    d.fecha,
    d.fecha_recepcion,
    d.tipo,
    d.monto_efectivo,
    d.moneda,
    d.estatus,
    d.observaciones,
    
    -- Informaci�n del donador
    dr.id_donador,
    dr.codigo as codigo_donador,
    dr.tipo as tipo_donador,
    dr.nombre as nombre_donador,
    dr.rfc as rfc_donador,
    dr.telefono as telefono_donador,
    dr.email as email_donador,
    
    -- Contacto espec�fico
    d.contacto_especifico,
    d.telefono_contacto,
    d.email_contacto,
    
    -- Estad�sticas del donativo
    (SELECT COUNT(*) FROM donativos_detalle dd WHERE dd.id_donativo = d.id_donativo) as total_articulos,
    (SELECT SUM(dd.cantidad_recibida) FROM donativos_detalle dd WHERE dd.id_donativo = d.id_donativo) as total_unidades,
    (SELECT SUM(dd.valor_total) FROM donativos_detalle dd WHERE dd.id_donativo = d.id_donativo) as valor_total_especie,
    
    -- Tiempo en sistema
    DATEDIFF(DAY, d.fecha, ISNULL(d.fecha_recepcion, GETDATE())) as dias_proceso
    
FROM donativos d
INNER JOIN donadores dr ON d.id_donador = dr.id_donador;
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[almacenes]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[almacenes](
	[id_almacen] [int] IDENTITY(1,1) NOT NULL,
	[codigo] [nvarchar](50) NOT NULL,
	[nombre] [nvarchar](200) NOT NULL,
	[tipo] [nvarchar](50) NULL,
	[activo] [bit] NULL,
	[fecha_creacion] [datetime2](3) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_almacen] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[articulos]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[articulos](
	[id_articulo] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](200) NOT NULL,
	[id_categoria] [int] NULL,
	[activo] [bit] NULL,
	[fecha_creacion] [datetime2](3) NULL,
	[fecha_actualizacion] [datetime2](3) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_articulo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[beneficiarios]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[beneficiarios](
	[id_beneficiario] [int] IDENTITY(1,1) NOT NULL,
	[codigo] [nvarchar](50) NULL,
	[tipo] [nvarchar](50) NULL,
	[nombre] [nvarchar](200) NOT NULL,
	[contacto_principal] [nvarchar](200) NULL,
	[telefono] [nvarchar](20) NULL,
	[email] [nvarchar](200) NULL,
	[direccion] [nvarchar](300) NULL,
	[colonia] [nvarchar](150) NULL,
	[ciudad] [nvarchar](100) NULL,
	[estado] [nvarchar](100) NULL,
	[codigo_postal] [nvarchar](10) NULL,
	[curp] [nvarchar](18) NULL,
	[total_ayudas] [int] NOT NULL,
	[monto_total_recibido] [decimal](18, 2) NOT NULL,
	[estatus] [nvarchar](50) NULL,
	[observaciones] [nvarchar](max) NULL,
	[fecha_registro] [datetime] NULL,
	[fecha_ultima_ayuda] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[bitacoras]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[bitacoras](
	[id_bitacora] [int] IDENTITY(1,1) NOT NULL,
	[fecha_hora] [datetime2](3) NULL,
	[id_usuario] [int] NULL,
	[modulo] [nvarchar](50) NOT NULL,
	[submodulo] [nvarchar](50) NULL,
	[accion] [nvarchar](100) NOT NULL,
	[tipo_accion] [nvarchar](20) NULL,
	[descripcion] [nvarchar](1000) NULL,
	[datos_anteriores] [nvarchar](max) NULL,
	[datos_nuevos] [nvarchar](max) NULL,
	[id_registro_afectado] [int] NULL,
	[ip_address] [nvarchar](45) NULL,
	[user_agent] [nvarchar](500) NULL,
	[navegador] [nvarchar](100) NULL,
	[sistema_operativo] [nvarchar](100) NULL,
	[nivel] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_bitacora] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[categorias]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[categorias](
	[id_categoria] [int] IDENTITY(1,1) NOT NULL,
	[nombre] [nvarchar](100) NOT NULL,
	[activo] [bit] NULL,
	[fecha_creacion] [datetime2](3) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_categoria] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[donativos_archivos]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[donativos_archivos](
	[id_archivo] [int] IDENTITY(1,1) NOT NULL,
	[id_donativo] [int] NOT NULL,
	[url] [nvarchar](max) NULL,
	[nombre_original] [nvarchar](max) NULL,
	[fecha_actualiza] [datetime2](7) NULL,
	[activo] [bit] NOT NULL,
 CONSTRAINT [PK_donativos_archivos] PRIMARY KEY CLUSTERED 
(
	[id_archivo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[inventario]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[inventario](
	[id_inventario] [int] IDENTITY(1,1) NOT NULL,
	[id_almacen] [int] NOT NULL,
	[id_articulo] [int] NOT NULL,
	[cantidad] [decimal](10, 2) NOT NULL,
	[fecha_ultima_entrada] [datetime2](3) NULL,
	[fecha_ultima_salida] [datetime2](3) NULL,
	[fecha_actualizacion] [datetime2](3) NULL,
PRIMARY KEY CLUSTERED 
(
	[id_inventario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [UQ_inventario] UNIQUE NONCLUSTERED 
(
	[id_almacen] ASC,
	[id_articulo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[movimientos_inventario]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[movimientos_inventario](
	[id_movimiento] [int] IDENTITY(1,1) NOT NULL,
	[id_almacen] [int] NOT NULL,
	[id_articulo] [int] NOT NULL,
	[cantidad] [decimal](18, 2) NOT NULL,
	[tipo_movimiento] [nvarchar](50) NOT NULL,
	[fecha_movimiento] [datetime] NOT NULL,
	[id_usuario] [int] NULL,
	[id_donativo] [int] NULL,
	[id_salida] [int] NULL,
	[id_traspaso] [int] NULL,
	[observaciones] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[salidas]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[salidas](
	[id_salida] [int] IDENTITY(1,1) NOT NULL,
	[folio] [nvarchar](50) NULL,
	[fecha_salida] [datetime] NOT NULL,
	[id_almacen_origen] [int] NOT NULL,
	[id_beneficiario] [int] NULL,
	[motivo] [nvarchar](500) NULL,
	[observaciones] [nvarchar](max) NULL,
	[id_usuario_entrega] [int] NULL,
	[estatus] [nvarchar](50) NULL,
	[fecha_entrega] [datetime] NULL,
	[recibio] [nvarchar](200) NULL,
	[fecha_registro] [datetime] NULL,
	[fecha_actualizacion] [datetime] NULL,
	[tipo_salida] [nvarchar](50) NULL,
 CONSTRAINT [PK_salidas] PRIMARY KEY CLUSTERED 
(
	[id_salida] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[salidas_archivos]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[salidas_archivos](
	[id_archivo] [int] IDENTITY(1,1) NOT NULL,
	[id_salida] [int] NOT NULL,
	[url] [nvarchar](max) NULL,
	[nombre_original] [nvarchar](300) NULL,
	[fecha_actualiza] [datetime] NULL,
	[activo] [bit] NOT NULL,
 CONSTRAINT [PK_salidas_archivos] PRIMARY KEY CLUSTERED 
(
	[id_archivo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[salidas_detalle]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[salidas_detalle](
	[id_detalle] [int] IDENTITY(1,1) NOT NULL,
	[id_salida] [int] NOT NULL,
	[id_articulo] [int] NOT NULL,
	[cantidad_entregada] [decimal](18, 2) NOT NULL,
	[valor_unitario] [decimal](18, 2) NULL,
	[observaciones] [nvarchar](max) NULL,
	[unidad_medida] [nvarchar](50) NULL,
 CONSTRAINT [PK_salidas_detalle] PRIMARY KEY CLUSTERED 
(
	[id_detalle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[traspasos]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[traspasos](
	[id_traspaso] [int] IDENTITY(1,1) NOT NULL,
	[folio] [nvarchar](50) NULL,
	[fecha_traspaso] [datetime] NOT NULL,
	[fecha_registro] [datetime] NULL,
	[fecha_actualiza] [datetime] NULL,
	[fecha_recibido] [datetime] NULL,
	[id_usuario_envia] [int] NULL,
	[id_usuario_recibe] [int] NULL,
	[almacen_origen] [int] NULL,
	[almacen_destino] [int] NULL,
	[motivo] [nvarchar](500) NULL,
	[estatus] [nvarchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[traspasos_archivos]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[traspasos_archivos](
	[id_archivo] [int] IDENTITY(1,1) NOT NULL,
	[id_traspaso] [int] NULL,
	[url] [nvarchar](max) NULL,
	[nombre_original] [nvarchar](300) NULL,
	[tipo_archivo] [nvarchar](50) NULL,
	[activo] [bit] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[traspasos_detalle]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[traspasos_detalle](
	[id_detalle] [int] IDENTITY(1,1) NOT NULL,
	[id_traspaso] [int] NOT NULL,
	[id_articulo] [int] NULL,
	[cantidad] [decimal](18, 2) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usuarios]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usuarios](
	[id_usuario] [int] IDENTITY(1,1) NOT NULL,
	[usuario] [varchar](50) NULL,
	[password] [varchar](100) NULL,
	[activo] [bit] NULL,
	[centroAcopio] [varchar](50) NULL,
	[id_almacen] [int] NULL,
	[PasswordHash] [varbinary](64) NULL,
	[PasswordSalt] [varbinary](32) NULL,
	[nombreCompleto] [varchar](255) NULL,
	[correoElectronico] [varchar](255) NULL,
	[telefono] [varchar](10) NULL,
	[numeroEmpleado] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[id_usuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usuarios_admin]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usuarios_admin](
	[id_usuario] [int] NOT NULL,
	[fecha_registro] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[id_usuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [idx_almacenes_activo]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_almacenes_activo] ON [dbo].[almacenes]
(
	[activo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_almacenes_tipo]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_almacenes_tipo] ON [dbo].[almacenes]
(
	[tipo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idx_articulos_categoria]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_articulos_categoria] ON [dbo].[articulos]
(
	[id_categoria] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_articulos_nombre]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_articulos_nombre] ON [dbo].[articulos]
(
	[nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idx_bitacora_fecha]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_bitacora_fecha] ON [dbo].[bitacoras]
(
	[fecha_hora] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_bitacora_modulo]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_bitacora_modulo] ON [dbo].[bitacoras]
(
	[modulo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_bitacora_nivel]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_bitacora_nivel] ON [dbo].[bitacoras]
(
	[nivel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idx_bitacora_usuario]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_bitacora_usuario] ON [dbo].[bitacoras]
(
	[id_usuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idx_contactos_donador]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_contactos_donador] ON [dbo].[contactos_donadores]
(
	[id_donador] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idx_contactos_principal]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_contactos_principal] ON [dbo].[contactos_donadores]
(
	[es_principal] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_donadores_nombre]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_donadores_nombre] ON [dbo].[donadores]
(
	[nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_donadores_rfc]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_donadores_rfc] ON [dbo].[donadores]
(
	[rfc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_donadores_tipo]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_donadores_tipo] ON [dbo].[donadores]
(
	[tipo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idc_donativos_fecha_recepcion]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idc_donativos_fecha_recepcion] ON [dbo].[donativos]
(
	[fecha_recepcion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idx_donativos_donador]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_donativos_donador] ON [dbo].[donativos]
(
	[id_donador] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_donativos_estatus]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_donativos_estatus] ON [dbo].[donativos]
(
	[estatus] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_donativos_folio]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_donativos_folio] ON [dbo].[donativos]
(
	[folio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [idx_donativos_tipo]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_donativos_tipo] ON [dbo].[donativos]
(
	[tipo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_donativos_almacen_fecha]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [IX_donativos_almacen_fecha] ON [dbo].[donativos]
(
	[id_almacen] ASC,
	[fecha_recepcion] ASC
)
INCLUDE([folio],[estatus],[id_donador],[observaciones]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_donativos_donador]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [IX_donativos_donador] ON [dbo].[donativos]
(
	[id_donador] ASC
)
INCLUDE([id_almacen],[fecha_recepcion],[estatus]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_donativos_estatus]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [IX_donativos_estatus] ON [dbo].[donativos]
(
	[estatus] ASC
)
WHERE ([estatus]<>'cancelado')
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_donativos_folio]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [IX_donativos_folio] ON [dbo].[donativos]
(
	[folio] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_donativos_archivos_id_donativo]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [IX_donativos_archivos_id_donativo] ON [dbo].[donativos_archivos]
(
	[id_donativo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idx_detalle_articulo]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_detalle_articulo] ON [dbo].[donativos_detalle]
(
	[id_articulo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idx_detalle_donativo]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_detalle_donativo] ON [dbo].[donativos_detalle]
(
	[id_donativo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idx_inventario_almacen]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_inventario_almacen] ON [dbo].[inventario]
(
	[id_almacen] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [idx_inventario_articulo]    Script Date: 18/10/2026 03:54:07 p. m. ******/
CREATE NONCLUSTERED INDEX [idx_inventario_articulo] ON [dbo].[inventario]
(
	[id_articulo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[almacenes] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [dbo].[almacenes] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[articulos] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [dbo].[articulos] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[articulos] ADD  DEFAULT (getdate()) FOR [fecha_actualizacion]
GO
ALTER TABLE [dbo].[beneficiarios] ADD  DEFAULT ((0)) FOR [total_ayudas]
GO
ALTER TABLE [dbo].[beneficiarios] ADD  DEFAULT ((0)) FOR [monto_total_recibido]
GO
ALTER TABLE [dbo].[beneficiarios] ADD  DEFAULT (getdate()) FOR [fecha_registro]
GO
ALTER TABLE [dbo].[bitacoras] ADD  DEFAULT (getdate()) FOR [fecha_hora]
GO
ALTER TABLE [dbo].[categorias] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [dbo].[categorias] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[contactos_donadores] ADD  DEFAULT ((0)) FOR [es_principal]
GO
ALTER TABLE [dbo].[contactos_donadores] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [dbo].[contactos_donadores] ADD  DEFAULT (getdate()) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[contactos_donadores] ADD  DEFAULT (getdate()) FOR [fecha_actualizacion]
GO
ALTER TABLE [dbo].[donadores] ADD  DEFAULT ('M?xico') FOR [pais]
GO
ALTER TABLE [dbo].[donadores] ADD  DEFAULT (CONVERT([date],getdate())) FOR [fecha_creacion]
GO
ALTER TABLE [dbo].[donadores] ADD  DEFAULT ((0)) FOR [total_donativos]
GO
ALTER TABLE [dbo].[donadores] ADD  DEFAULT ((0)) FOR [monto_total_donado]
GO
ALTER TABLE [dbo].[donadores] ADD  DEFAULT (getdate()) FOR [fecha_actualizacion]
GO
ALTER TABLE [dbo].[donadores] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [dbo].[donativos] ADD  DEFAULT ('pendiente') FOR [estatus]
GO
ALTER TABLE [dbo].[donativos] ADD  DEFAULT (getdate()) FOR [fecha_registro]
GO
ALTER TABLE [dbo].[donativos] ADD  DEFAULT (getdate()) FOR [fecha_actualizacion]
GO
ALTER TABLE [dbo].[donativos_detalle] ADD  DEFAULT ((0)) FOR [cantidad_recibida]
GO
ALTER TABLE [dbo].[inventario] ADD  DEFAULT ((0)) FOR [cantidad]
GO
ALTER TABLE [dbo].[inventario] ADD  DEFAULT (getdate()) FOR [fecha_actualizacion]
GO
ALTER TABLE [dbo].[movimientos_inventario] ADD  DEFAULT (getdate()) FOR [fecha_movimiento]
GO
ALTER TABLE [dbo].[salidas] ADD  DEFAULT (getdate()) FOR [fecha_registro]
GO
ALTER TABLE [dbo].[salidas_archivos] ADD  DEFAULT ((1)) FOR [activo]
GO
ALTER TABLE [dbo].[traspasos] ADD  DEFAULT (getdate()) FOR [fecha_registro]
GO
ALTER TABLE [dbo].[usuarios_admin] ADD  DEFAULT (getdate()) FOR [fecha_registro]
GO
ALTER TABLE [dbo].[articulos]  WITH CHECK ADD  CONSTRAINT [FK_articulos_categorias] FOREIGN KEY([id_categoria])
REFERENCES [dbo].[categorias] ([id_categoria])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[articulos] CHECK CONSTRAINT [FK_articulos_categorias]
GO
ALTER TABLE [dbo].[bitacoras]  WITH CHECK ADD  CONSTRAINT [FK_bitacora_usuario] FOREIGN KEY([id_usuario])
REFERENCES [dbo].[usuarios] ([id_usuario])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[bitacoras] CHECK CONSTRAINT [FK_bitacora_usuario]
GO
ALTER TABLE [dbo].[contactos_donadores]  WITH CHECK ADD  CONSTRAINT [FK_contactos_donador] FOREIGN KEY([id_donador])
REFERENCES [dbo].[donadores] ([id_donador])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[contactos_donadores] CHECK CONSTRAINT [FK_contactos_donador]
GO
ALTER TABLE [dbo].[donativos]  WITH CHECK ADD  CONSTRAINT [FK_donativos_donadores] FOREIGN KEY([id_donador])
REFERENCES [dbo].[donadores] ([id_donador])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[donativos] CHECK CONSTRAINT [FK_donativos_donadores]
GO
ALTER TABLE [dbo].[donativos_archivos]  WITH CHECK ADD  CONSTRAINT [FK_donativos_archivos_donativos_id_donativo] FOREIGN KEY([id_donativo])
REFERENCES [dbo].[donativos] ([id_donativo])
GO
ALTER TABLE [dbo].[donativos_archivos] CHECK CONSTRAINT [FK_donativos_archivos_donativos_id_donativo]
GO
ALTER TABLE [dbo].[donativos_detalle]  WITH CHECK ADD  CONSTRAINT [FK_detalle_articulo] FOREIGN KEY([id_articulo])
REFERENCES [dbo].[articulos] ([id_articulo])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[donativos_detalle] CHECK CONSTRAINT [FK_detalle_articulo]
GO
ALTER TABLE [dbo].[donativos_detalle]  WITH CHECK ADD  CONSTRAINT [FK_detalle_donativo] FOREIGN KEY([id_donativo])
REFERENCES [dbo].[donativos] ([id_donativo])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[donativos_detalle] CHECK CONSTRAINT [FK_detalle_donativo]
GO
ALTER TABLE [dbo].[inventario]  WITH CHECK ADD  CONSTRAINT [FK_inventario_almacen] FOREIGN KEY([id_almacen])
REFERENCES [dbo].[almacenes] ([id_almacen])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[inventario] CHECK CONSTRAINT [FK_inventario_almacen]
GO
ALTER TABLE [dbo].[inventario]  WITH CHECK ADD  CONSTRAINT [FK_inventario_articulo] FOREIGN KEY([id_articulo])
REFERENCES [dbo].[articulos] ([id_articulo])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[inventario] CHECK CONSTRAINT [FK_inventario_articulo]
GO
ALTER TABLE [dbo].[usuarios_admin]  WITH CHECK ADD  CONSTRAINT [FK_usuarios_admin_usuarios] FOREIGN KEY([id_usuario])
REFERENCES [dbo].[usuarios] ([id_usuario])
GO
ALTER TABLE [dbo].[usuarios_admin] CHECK CONSTRAINT [FK_usuarios_admin_usuarios]
GO
ALTER TABLE [dbo].[almacenes]  WITH CHECK ADD CHECK  (([tipo]='cuarentena' OR [tipo]='distribucion' OR [tipo]='temporal' OR [tipo]='regional' OR [tipo]='central'))
GO
ALTER TABLE [dbo].[bitacoras]  WITH CHECK ADD CHECK  (([nivel]='critico' OR [nivel]='error' OR [nivel]='advertencia' OR [nivel]='info'))
GO
ALTER TABLE [dbo].[bitacoras]  WITH CHECK ADD CHECK  (([tipo_accion]='logout' OR [tipo_accion]='login' OR [tipo_accion]='error' OR [tipo_accion]='sistema' OR [tipo_accion]='eliminar' OR [tipo_accion]='actualizar' OR [tipo_accion]='leer' OR [tipo_accion]='crear'))
GO
/****** Object:  StoredProcedure [dbo].[ActualizarPassword]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ActualizarPassword]
    @usuario NVARCHAR(50),
    @pwActual NVARCHAR(200),
    @pwNuevo NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @saltActual VARBINARY(32);
    DECLARE @hashActual VARBINARY(64);
    DECLARE @hashIngresado VARBINARY(64);

    -- Obtener salt y hash actuales
    SELECT 
        @saltActual = PasswordSalt,
        @hashActual = PasswordHash
    FROM Usuarios
    WHERE usuario = @usuario
      AND activo = 1;

    IF @saltActual IS NULL
    BEGIN
        SELECT -1 AS Resultado, 'Usuario no encontrado o inactivo' AS Mensaje;
        RETURN;
    END

    -- Generar hash del password actual ingresado
    SET @hashIngresado = HASHBYTES(
        'SHA2_256',
        @pwActual + CONVERT(VARCHAR(64), @saltActual, 1)
    );

    -- Validar password actual
    IF @hashIngresado <> @hashActual
    BEGIN
        SELECT -2 AS Resultado, 'El password actual es incorrecto' AS Mensaje;
        RETURN;
    END

    -- Generar nuevo salt
    DECLARE @nuevoSalt VARBINARY(32) = CRYPT_GEN_RANDOM(32);

    -- Generar nuevo hash
    DECLARE @nuevoHash VARBINARY(64) = HASHBYTES(
        'SHA2_256',
        @pwNuevo + CONVERT(VARCHAR(64), @nuevoSalt, 1)
    );

    -- Actualizar contraseña
    UPDATE Usuarios
    SET PasswordHash = @nuevoHash,
        PasswordSalt = @nuevoSalt
    WHERE usuario = @usuario;

    SELECT 1 AS Resultado, 'Password actualizado correctamente' AS Mensaje;
END;
GO
/****** Object:  StoredProcedure [dbo].[LoginUsuario]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[LoginUsuario] @usuario NVARCHAR(50),
@pass NVARCHAR(200) AS BEGIN
SET
    NOCOUNT ON;

DECLARE @salt VARBINARY(32);

DECLARE @hash VARBINARY(64);

-- Obtener el salt del usuario
SELECT
    @salt = PasswordSalt
FROM
    Usuarios
WHERE
    usuario = @usuario
    AND activo = 1;

IF @salt IS NULL BEGIN -- Usuario no existe o está inactivo
SELECT
    0 AS UsuarioID;

RETURN;

END -- Generar hash del password ingresado
SET
    @hash = HASHBYTES('SHA2_256', @pass + CONVERT(VARCHAR(64), @salt, 1));

-- Validar credenciales
SELECT
    usuarios.id_usuario,
    almacenes.nombre as centro_acopio,
    almacenes.id_almacen as centro_id,
    nombreCompleto as nombre_completo
FROM
    Usuarios
    join almacenes on usuarios.id_almacen = almacenes.id_almacen
WHERE
    usuario = @usuario
    AND PasswordHash = @hash
    AND usuarios.activo = 1;

END;
GO
/****** Object:  StoredProcedure [dbo].[RegistrarUsuario]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[RegistrarUsuario]
    @usuario NVARCHAR(50),
    @pw NVARCHAR(200),
    @centroAcopio NVARCHAR(100),
    @centroID INT,
    @nombreCompleto NVARCHAR(100),
    @correoElectronico NVARCHAR(100),
    @telefono NVARCHAR(15)

AS
BEGIN
    SET NOCOUNT ON;

    -- Validar si el usuario ya existe
    IF EXISTS (SELECT 1 FROM Usuarios WHERE usuario = @usuario)
    BEGIN
        SELECT -1 AS UsuarioID, 'El usuario ya existe' AS Mensaje;
        RETURN;
    END

    -- Generar salt aleatorio (32 bytes)
    DECLARE @salt VARBINARY(32) = CRYPT_GEN_RANDOM(32);

    -- Generar hash seguro
    DECLARE @hash VARBINARY(64) = HASHBYTES(
        'SHA2_256',
        @pw + CONVERT(VARCHAR(64), @salt, 1)
    );

    -- Insertar usuario
    INSERT INTO Usuarios (usuario, PasswordHash, PasswordSalt, centroAcopio, id_almacen, activo,nombreCompleto,correoElectronico,telefono)
    VALUES (@usuario, @hash, @salt, @centroAcopio, @centroID, 1,@nombreCompleto,@correoElectronico,@telefono);

    -- Devolver ID del usuario creado
    SELECT cast(SCOPE_IDENTITY() as int) AS UsuarioID, 'Usuario registrado correctamente' AS Mensaje;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_BuscarDonador]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- SP: Buscar donador por diferentes criterios
CREATE PROCEDURE [dbo].[sp_BuscarDonador]
    @criterio NVARCHAR(100) = NULL,
    @tipo NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        id_donador,
        codigo,
        tipo,
        nombre,
        rfc,
        contacto_principal,
        telefono,
        email,
        direccion,
        fecha_registro,
        fecha_ultima_donacion,
        total_donativos,
        monto_total_donado
    FROM donadores
    WHERE (@criterio IS NULL OR 
           nombre LIKE '%' + @criterio + '%' OR
           rfc LIKE '%' + @criterio + '%' OR
           codigo LIKE '%' + @criterio + '%' OR
           contacto_principal LIKE '%' + @criterio + '%' OR
           email LIKE '%' + @criterio + '%')
      AND (@tipo IS NULL OR tipo = @tipo)
    ORDER BY nombre;
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_Consulta_Movimientos]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_Consulta_Movimientos] @fecha_inicio DATETIME = NULL,
    @fecha_fin DATETIME = NULL,
    @tipo_movimiento NVARCHAR(50) = 'todos',
    @folio NVARCHAR(50) = NULL,
    @id_donador INT = NULL,
    @id_beneficiario INT = NULL,
    @id_articulo INT = NULL,
    @id_almacen INT = NULL,
    @estatus NVARCHAR(50) = NULL,
    @pagina INT = 1,
    @registros_por_pagina INT = 50 AS BEGIN
SET
    NOCOUNT ON;

-- CTE para Entradas (Donativos)
WITH Entradas AS (
    SELECT
        d.id_donativo AS id,
        'entrada' AS tipo,
        d.folio,
        d.fecha_recepcion AS fecha,
        CONCAT(
            'Donación de ',
            don.nombre,
            ' - ',
            COUNT(DISTINCT dd.id_articulo),
            ' artículo(s)'
        ) AS descripcion,
        don.nombre AS responsable,
        'donador' AS tipo_responsable,
        alm.nombre AS almacen_origen,
        NULL AS almacen_destino,
        COUNT(DISTINCT dd.id_articulo) AS total_articulos,
        ISNULL(SUM(dd.cantidad_recibida), 0) AS cantidad_total,
        d.estatus,
        d.observaciones,
        d.fecha_recepcion AS fecha_orden
    FROM
        donativos d
        INNER JOIN donadores don ON d.id_donador = don.id_donador
        INNER JOIN almacenes alm ON d.id_almacen = alm.id_almacen
        LEFT JOIN donativos_detalle dd ON d.id_donativo = dd.id_donativo
    WHERE
        (
            @tipo_movimiento = 'todos'
            OR @tipo_movimiento = 'entrada'
        )
        AND (
            @fecha_inicio IS NULL
            OR d.fecha_recepcion >= @fecha_inicio
        )
        AND (
            @fecha_fin IS NULL
            OR d.fecha_recepcion <= @fecha_fin
        )
        AND (
            @id_almacen IS NULL
            OR d.id_almacen = @id_almacen
        )
        AND (
            @id_donador IS NULL
            OR d.id_donador = @id_donador
        )
        AND (
            @id_articulo IS NULL
            OR dd.id_articulo = @id_articulo
        )
        AND (
            @estatus IS NULL
            OR d.estatus = case
                @estatus
                when 'completado' then 'Recibido'
                when 'pendiente' then 'Recibido'
                when 'cancelado' then 'cancelado'
                else 'Recibido'
            end
        )
        AND (
            @folio IS NULL
            OR d.folio LIKE '%' + @folio + '%'
        ) -- AND d.estatus != 'cancelado'
    GROUP BY
        d.id_donativo,
        d.folio,
        d.fecha_recepcion,
        don.nombre,
        alm.nombre,
        d.estatus,
        d.observaciones
),
-- CTE para Salidas
SalidasCTE AS (
    SELECT
        s.id_salida AS id,
        'salida' AS tipo,
        s.folio,
        s.fecha_salida AS fecha,
        CONCAT(
            CASE
                s.tipo_salida
                WHEN 'donacion' THEN 'Donación a '
                WHEN 'merma' THEN 'Merma - '
                WHEN 'baja' THEN 'Baja - '
                ELSE 'Salida a '
            END,
            ben.nombre,
            ' - ',
            COUNT(DISTINCT sd.id_articulo),
            ' artículo(s)'
        ) AS descripcion,
        ben.nombre AS responsable,
        'beneficiario' AS tipo_responsable,
        alm.nombre AS almacen_origen,
        NULL AS almacen_destino,
        COUNT(DISTINCT sd.id_articulo) AS total_articulos,
        ISNULL(SUM(sd.cantidad_entregada), 0) AS cantidad_total,
        s.estatus,
        s.observaciones,
        s.fecha_salida AS fecha_orden
    FROM
        salidas s
        LEFT JOIN beneficiarios ben ON s.id_beneficiario = ben.id_beneficiario
        INNER JOIN almacenes alm ON s.id_almacen_origen = alm.id_almacen
        LEFT JOIN salidas_detalle sd ON s.id_salida = sd.id_salida
    WHERE
        (
            @tipo_movimiento = 'todos'
            OR @tipo_movimiento = 'salida'
        )
        AND (
            @fecha_inicio IS NULL
            OR s.fecha_salida >= @fecha_inicio
        )
        AND (
            @fecha_fin IS NULL
            OR s.fecha_salida <= @fecha_fin
        )
        AND (
            @id_almacen IS NULL
            OR s.id_almacen_origen = @id_almacen
        )
        AND (
            @id_beneficiario IS NULL
            OR s.id_beneficiario = @id_beneficiario
        )
        AND (
            @id_articulo IS NULL
            OR sd.id_articulo = @id_articulo
        )
        AND (
            @estatus IS NULL
            OR s.estatus = case @estatus
                when 'completado' then 'Entregada'
                when 'pendiente' then 'Pendiente'
                when 'cancelado' then 'cancelada'
                else 'Entregada'
            end
        )
        AND (
            @folio IS NULL
            OR s.folio LIKE '%' + @folio + '%'
        ) -- AND s.estatus != 'cancelada'
    GROUP BY
        s.id_salida,
        s.folio,
        s.fecha_salida,
        ben.nombre,
        alm.nombre,
        s.tipo_salida,
        s.estatus,
        s.observaciones
),
-- CTE para Traspasos
TraspasosCTE AS (
    SELECT
        t.id_traspaso AS id,
        'traspaso' AS tipo,
        t.folio,
        t.fecha_traspaso AS fecha,
        CONCAT(
            'Traspaso ',
            alm_origen.nombre,
            ' → ',
            alm_destino.nombre,
            ' - ',
            COUNT(DISTINCT td.id_articulo),
            ' artículo(s)'
        ) AS descripcion,
        'Sistema' AS responsable,
        'almacen' AS tipo_responsable,
        alm_origen.nombre AS almacen_origen,
        alm_destino.nombre AS almacen_destino,
        COUNT(DISTINCT td.id_articulo) AS total_articulos,
        ISNULL(SUM(td.cantidad), 0) AS cantidad_total,
        t.estatus,
        t.motivo AS observaciones,
        t.fecha_traspaso AS fecha_orden
    FROM
        traspasos t
        INNER JOIN almacenes alm_origen ON t.almacen_origen = alm_origen.id_almacen
        INNER JOIN almacenes alm_destino ON t.almacen_destino = alm_destino.id_almacen
        LEFT JOIN traspasos_detalle td ON t.id_traspaso = td.id_traspaso
    WHERE
        (
            @tipo_movimiento = 'todos'
            OR @tipo_movimiento = 'traspaso'
        )
        AND (
            @fecha_inicio IS NULL
            OR t.fecha_traspaso >= @fecha_inicio
        )
        AND (
            @fecha_fin IS NULL
            OR t.fecha_traspaso <= @fecha_fin
        )
        AND (
            @id_almacen IS NULL
            OR (
                t.almacen_origen = @id_almacen
                OR t.almacen_destino = @id_almacen
            )
        )
        AND (
            @id_articulo IS NULL
            OR td.id_articulo = @id_articulo
        )
        AND (
            @estatus IS NULL
            OR t.estatus = case @estatus
                when 'completado' then 'Aprobado'
                when 'pendiente' then 'Pendiente'
                when 'cancelado' then 'Cancelado'
                else 'Aprobado'
            end
        )
        AND (
            @folio IS NULL
            OR t.folio LIKE '%' + @folio + '%'
        ) -- AND t.estatus != 'cancelado'
    GROUP BY
        t.id_traspaso,
        t.folio,
        t.fecha_traspaso,
        alm_origen.nombre,
        alm_destino.nombre,
        t.estatus,
        t.motivo
),
-- Unión de todos los movimientos
TodosMovimientos AS (
    SELECT
        *
    FROM
        Entradas
    UNION
    ALL
    SELECT
        *
    FROM
        SalidasCTE
    UNION
    ALL
    SELECT
        *
    FROM
        TraspasosCTE
)
Select
    * into #TodosMovimientos from TodosMovimientos
    -- RESULTADO 1: Total de registros (para paginación)
SELECT
    COUNT(*) AS total_registros
FROM
    #TodosMovimientos;
    -- RESULTADO 2: Movimientos paginados
SELECT
    *
FROM
    #TodosMovimientos
ORDER BY
    fecha_orden DESC OFFSET (@pagina - 1) * @registros_por_pagina ROWS FETCH NEXT @registros_por_pagina ROWS ONLY;

END
GO
/****** Object:  StoredProcedure [dbo].[sp_ConsultarMovimientosAgrupados]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_ConsultarMovimientosAgrupados]
    @fechaInicio DATETIME = NULL,
    @fechaFin DATETIME = NULL,
    @tipoMovimiento NVARCHAR(20) = 'todos',
    @folio NVARCHAR(50) = NULL,
    @idAlmacen INT = NULL,
    @idArticulo INT = NULL,
    @pagina INT = 1,
    @registrosPorPagina INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validaciones de entrada
    IF @pagina < 1 SET @pagina = 1;
    IF @registrosPorPagina < 1 OR @registrosPorPagina > 100 SET @registrosPorPagina = 50;
    
    -- Calcular offset
    DECLARE @offset INT = (@pagina - 1) * @registrosPorPagina;

    -- CTE: Movimientos base con filtros
    ;WITH MovimientosBase AS (
        SELECT 
            m.tipo_movimiento,
            m.id_donativo,
            m.id_salida,
            m.id_traspaso,
            m.fecha_movimiento,
            a.nombre AS almacen_nombre,
            m.observaciones,
            
            -- Determinar tipo simplificado
            CASE 
                WHEN m.tipo_movimiento = 'Donativo' THEN 'entrada'
                WHEN m.tipo_movimiento = 'Salida' THEN 'salida'
                WHEN m.tipo_movimiento LIKE 'Traspaso%' THEN 'traspaso'
                ELSE 'entrada'
            END AS tipo_simplificado
            
        FROM movimientos_inventario m WITH (NOLOCK)
        INNER JOIN Almacenes a WITH (NOLOCK) ON m.id_almacen = a.id_almacen
        INNER JOIN Articulos art WITH (NOLOCK) ON m.id_articulo = art.id_articulo
        
        WHERE 1=1
            AND art.activo = 1
            AND (@fechaInicio IS NULL OR m.fecha_movimiento >= @fechaInicio)
            AND (@fechaFin IS NULL OR m.fecha_movimiento <= @fechaFin)
            AND (@idAlmacen IS NULL OR m.id_almacen = @idAlmacen)
            AND (@idArticulo IS NULL OR m.id_articulo = @idArticulo)
            AND (
                @tipoMovimiento = 'todos'
                OR (@tipoMovimiento = 'entrada' AND m.tipo_movimiento IN ('Donativo', 'Traspaso-Entrada'))
                OR (@tipoMovimiento = 'salida' AND m.tipo_movimiento IN ('Salida', 'Traspaso-Salida'))
                OR (@tipoMovimiento = 'traspaso' AND m.tipo_movimiento LIKE 'Traspaso%')
            )
    ),
    
    -- CTE: Agrupación por movimiento original
    MovimientosAgrupados AS (
        SELECT 
            tipo_simplificado AS tipo,
            COALESCE(id_donativo, id_salida, id_traspaso, 0) AS id,
            MAX(fecha_movimiento) AS fecha,
            MAX(almacen_nombre) AS almacen_origen,
            MAX(observaciones) AS observaciones,
            COUNT(*) AS total_articulos_movimiento
        FROM MovimientosBase
        GROUP BY 
            tipo_simplificado,
            COALESCE(id_donativo, id_salida, id_traspaso, 0)
    ),
    
    -- CTE: Aplicar filtro de folio y calcular total
    MovimientosFiltrados AS (
        SELECT 
            *,
            -- Generar folio en SQL
            CONCAT(
                CASE tipo 
                    WHEN 'entrada' THEN 'E-' 
                    WHEN 'salida' THEN 'S-' 
                    WHEN 'traspaso' THEN 'T-' 
                    ELSE 'M-' 
                END,
                RIGHT('0000' + CAST(id AS VARCHAR), 4)
            ) AS folio,
            COUNT(*) OVER() AS total_registros
        FROM MovimientosAgrupados
        WHERE @folio IS NULL 
            OR CONCAT(
                CASE tipo 
                    WHEN 'entrada' THEN 'E-' 
                    WHEN 'salida' THEN 'S-' 
                    WHEN 'traspaso' THEN 'T-' 
                    ELSE 'M-' 
                END,
                RIGHT('0000' + CAST(id AS VARCHAR), 4)
            ) LIKE '%' + @folio + '%'
    ),
    
    -- CTE: Aplicar paginación
    MovimientosPaginados AS (
        SELECT *
        FROM MovimientosFiltrados
        ORDER BY fecha DESC
        OFFSET @offset ROWS
        FETCH NEXT @registrosPorPagina ROWS ONLY
    )
    
    -- Resultado final con información de responsables
    SELECT 
        mp.tipo,
        mp.id,
        mp.folio,
        mp.fecha,
        mp.almacen_origen,
        mp.observaciones,
        mp.total_articulos_movimiento,
        mp.total_registros,
        
        -- Responsable (LEFT JOIN para evitar nulls)
        COALESCE(don.nombre, ben.nombre, 'Sistema') AS responsable,
        
        CASE 
            WHEN don.nombre IS NOT NULL THEN 'donador'
            WHEN ben.nombre IS NOT NULL THEN 'beneficiario'
            ELSE 'almacen'
        END AS tipo_responsable,
        
        -- Almacén destino (solo para traspasos)
        ad.nombre AS almacen_destino,
        
        -- Metadatos adicionales
        @pagina AS pagina_actual,
        CEILING(CAST(mp.total_registros AS FLOAT) / @registrosPorPagina) AS total_paginas
        
    FROM MovimientosPaginados mp
    
    -- JOIN condicional para donativos
    LEFT JOIN donativos d WITH (NOLOCK) 
        ON mp.tipo = 'entrada' AND mp.id = d.id_donativo AND d.estatus <> 'cancelado'
    LEFT JOIN Donadores don WITH (NOLOCK) 
        ON d.id_donador = don.id_donador AND don.activo = 1
    
    -- JOIN condicional para salidas
    LEFT JOIN salidas s WITH (NOLOCK) 
        ON mp.tipo = 'salida' AND mp.id = s.id_salida AND s.estatus <> 'cancelada'
    LEFT JOIN Beneficiarios ben WITH (NOLOCK) 
        ON s.id_beneficiario = ben.id_beneficiario
    
    -- JOIN condicional para traspasos
    LEFT JOIN Traspasos t WITH (NOLOCK) 
        ON mp.tipo = 'traspaso' AND mp.id = t.id_traspaso AND t.estatus <> 'cancelado'
    LEFT JOIN Almacenes ad WITH (NOLOCK) 
        ON t.almacen_destino = ad.id_almacen
    
    ORDER BY mp.fecha DESC;
    
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_Detalle_Movimiento]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_Detalle_Movimiento] @tipo_movimiento NVARCHAR(20),
    @id_movimiento INT AS BEGIN
SET
    NOCOUNT ON;

IF @tipo_movimiento = 'entrada' BEGIN -- RESULTADO 1: Datos principales del donativo
SELECT
    d.folio,
    d.fecha_recepcion AS fecha,
    d.id_almacen AS almacen_origen_id,
    alm.nombre AS almacen_origen_nombre,
    NULL AS almacen_destino_id,
    NULL AS almacen_destino_nombre,
    NULL AS almacen_destino_ubicacion,
    d.id_donador AS responsable_id,
    'donador' AS responsable_tipo,
    don.codigo AS responsable_codigo,
    don.nombre AS responsable_nombre,
    don.telefono AS responsable_telefono,
    don.email AS responsable_email,
    CONCAT_WS(
        ', ',
        don.direccion,
        don.colonia,
        don.ciudad,
        don.estado
    ) AS responsable_direccion,
    d.estatus,
    d.observaciones,
    d.tipo AS tipo_movimiento,
    d.id_usuario_registro AS usuario_id,
    u.nombreCompleto AS usuario_nombre,
    d.fecha_registro
FROM
    donativos d
    INNER JOIN almacenes alm ON d.id_almacen = alm.id_almacen
    INNER JOIN donadores don ON d.id_donador = don.id_donador
    INNER JOIN usuarios u ON d.id_usuario_registro = u.id_usuario
WHERE
    d.id_donativo = @id_movimiento;

-- RESULTADO 2: Detalles de artículos
SELECT
    dd.id_donativo_detalle AS id_detalle,
    dd.id_articulo,
    art.nombre AS nombre_articulo,
    cat.nombre AS categoria,
    dd.cantidad_recibida AS cantidad,
    dd.unidad_medida,
    dd.valor_unitario,
    (dd.cantidad_recibida * dd.valor_unitario) AS valor_total,
    dd.descripcion
FROM
    donativos_detalle dd
    INNER JOIN articulos art ON dd.id_articulo = art.id_articulo
    LEFT JOIN categorias cat ON art.id_categoria = cat.id_categoria
WHERE
    dd.id_donativo = @id_movimiento
ORDER BY
    art.nombre;

-- RESULTADO 3: Archivos adjuntos
SELECT
    id_archivo,
    nombre_original,
    url,
    0 AS tamano
FROM
    donativos_archivos
WHERE
    id_donativo = @id_movimiento
    AND activo = 1;

END
ELSE IF @tipo_movimiento = 'salida' BEGIN -- RESULTADO 1: Datos principales de la salida
SELECT
    s.folio,
    s.fecha_salida AS fecha,
    s.id_almacen_origen AS almacen_origen_id,
    alm.nombre AS almacen_origen_nombre,
    NULL AS almacen_destino_id,
    NULL AS almacen_destino_nombre,
    NULL AS almacen_destino_ubicacion,
    ISNULL(s.id_beneficiario, 0) AS responsable_id,
    'beneficiario' AS responsable_tipo,
    ben.codigo AS responsable_codigo,
    ISNULL(ben.nombre, s.recibio) AS responsable_nombre,
    ben.telefono AS responsable_telefono,
    ben.email AS responsable_email,
    CONCAT_WS(
        ', ',
        ben.direccion,
        ben.colonia,
        ben.ciudad,
        ben.estado
    ) AS responsable_direccion,
    s.estatus,
    s.observaciones,
    s.tipo_salida AS tipo_movimiento,
    s.id_usuario_entrega AS usuario_id,
    u.nombreCompleto AS usuario_nombre,
    s.fecha_registro
FROM
    salidas s
    INNER JOIN almacenes alm ON s.id_almacen_origen = alm.id_almacen
    LEFT JOIN beneficiarios ben ON s.id_beneficiario = ben.id_beneficiario
    LEFT JOIN usuarios u ON s.id_usuario_entrega = u.id_usuario
WHERE
    s.id_salida = @id_movimiento;

-- RESULTADO 2: Detalles de artículos
SELECT
    sd.id_detalle,
    sd.id_articulo,
    art.nombre AS nombre_articulo,
    cat.nombre AS categoria,
    sd.cantidad_entregada AS cantidad,
    sd.unidad_medida,
    sd.valor_unitario,
    (
        sd.cantidad_entregada * ISNULL(sd.valor_unitario, 0)
    ) AS valor_total,
    sd.observaciones AS descripcion
FROM
    salidas_detalle sd
    INNER JOIN articulos art ON sd.id_articulo = art.id_articulo
    LEFT JOIN categorias cat ON art.id_categoria = cat.id_categoria
WHERE
    sd.id_salida = @id_movimiento
ORDER BY
    art.nombre;

-- RESULTADO 3: Archivos adjuntos
SELECT
    id_archivo,
    nombre_original,
    url,
    0 AS tamano
FROM
    salidas_archivos
WHERE
    id_salida = @id_movimiento
    AND activo = 1;

END
ELSE IF @tipo_movimiento = 'traspaso' BEGIN -- RESULTADO 1: Datos principales del traspaso
SELECT
    t.folio,
    t.fecha_traspaso AS fecha,
    t.almacen_origen AS almacen_origen_id,
    ao.nombre AS almacen_origen_nombre,
    t.almacen_destino AS almacen_destino_id,
    ad.nombre AS almacen_destino_nombre,
    0 AS responsable_id,
    'almacen' AS responsable_tipo,
    '' AS responsable_codigo,
    ad.nombre AS responsable_nombre,
    '' AS responsable_telefono,
    '' AS responsable_email,
    '' AS responsable_direccion,
    t.estatus,
    t.motivo AS observaciones,
    'traspaso' AS tipo_movimiento,
    t.id_usuario_envia AS usuario_id,
    '' AS usuario_nombre,
    t.fecha_registro
FROM
    traspasos t
    INNER JOIN almacenes ao ON t.almacen_origen = ao.id_almacen
    INNER JOIN almacenes ad ON t.almacen_destino = ad.id_almacen
WHERE
    t.id_traspaso = @id_movimiento;

-- RESULTADO 2: Detalles de artículos
SELECT
    td.id_detalle,
    td.id_articulo,
    art.nombre AS nombre_articulo,
    cat.nombre AS categoria,
    td.cantidad,
    0 AS valor_unitario,
    0 AS valor_total
FROM
    traspasos_detalle td
    INNER JOIN articulos art ON td.id_articulo = art.id_articulo
    LEFT JOIN categorias cat ON art.id_categoria = cat.id_categoria
WHERE
    td.id_traspaso = @id_movimiento
ORDER BY
    art.nombre;

-- RESULTADO 3: Archivos adjuntos
SELECT
    id_archivo,
    nombre_original,
    url,
    0 AS tamano
FROM
    traspasos_archivos
WHERE
    id_traspaso = @id_movimiento
    AND activo = 1;

END
END
GO
/****** Object:  StoredProcedure [dbo].[sp_Estadisticas_Almacen]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_Estadisticas_Almacen]
    @id_almacen INT
AS
BEGIN
    SET NOCOUNT ON;

    -- RESULTADO 1: Información del almacén
    SELECT 
        id_almacen,
        nombre
    FROM almacenes
    WHERE id_almacen = @id_almacen;

    -- RESULTADO 2: Resumen general
    SELECT 
        (SELECT COUNT(*) FROM articulos WHERE activo = 1) AS total_articulos_catalogo,
        (SELECT COUNT(DISTINCT id_articulo) FROM inventario WHERE id_almacen = @id_almacen AND cantidad > 0) AS articulos_con_stock,
        (SELECT COUNT(*) FROM articulos WHERE activo = 1) - 
            (SELECT COUNT(DISTINCT id_articulo) FROM inventario WHERE id_almacen = @id_almacen AND cantidad > 0) AS articulos_sin_stock,
        (SELECT ISNULL(SUM(cantidad), 0) FROM inventario WHERE id_almacen = @id_almacen) AS stock_total_general,
        (SELECT COUNT(DISTINCT id_articulo) FROM inventario WHERE id_almacen = @id_almacen AND cantidad > 0 AND cantidad < 10) AS alertas_stock_bajo;

    -- RESULTADO 3: Movimientos (hoy y este mes)
    SELECT 
        -- HOY
        (SELECT COUNT(*) FROM donativos 
         WHERE id_almacen = @id_almacen 
           AND CAST(fecha_recepcion AS DATE) = CAST(GETDATE() AS DATE)
           AND estatus != 'cancelado') AS entradas_hoy,
        
        (SELECT COUNT(*) FROM salidas 
         WHERE id_almacen_origen = @id_almacen 
           AND CAST(fecha_salida AS DATE) = CAST(GETDATE() AS DATE)
           AND estatus != 'cancelada') AS salidas_hoy,
        
        (SELECT COUNT(*) FROM traspasos 
         WHERE (almacen_origen = @id_almacen OR almacen_destino = @id_almacen)
           AND CAST(fecha_traspaso AS DATE) = CAST(GETDATE() AS DATE)
           AND estatus != 'cancelado') AS traspasos_hoy,
        
        -- ESTE MES
        (SELECT COUNT(*) FROM donativos 
         WHERE id_almacen = @id_almacen 
           AND MONTH(fecha_recepcion) = MONTH(GETDATE())
           AND YEAR(fecha_recepcion) = YEAR(GETDATE())
           AND estatus != 'cancelado') AS entradas_mes,
        
        (SELECT COUNT(*) FROM salidas 
         WHERE id_almacen_origen = @id_almacen 
           AND MONTH(fecha_salida) = MONTH(GETDATE())
           AND YEAR(fecha_salida) = YEAR(GETDATE())
           AND estatus != 'cancelada') AS salidas_mes,
        
        (SELECT COUNT(*) FROM traspasos 
         WHERE almacen_origen = @id_almacen
           AND MONTH(fecha_traspaso) = MONTH(GETDATE())
           AND YEAR(fecha_traspaso) = YEAR(GETDATE())
           AND estatus != 'cancelado') AS traspasos_mes,
        
        (SELECT COUNT(*) FROM traspasos 
         WHERE almacen_destino = @id_almacen
           AND MONTH(fecha_traspaso) = MONTH(GETDATE())
           AND YEAR(fecha_traspaso) = YEAR(GETDATE())
           AND estatus != 'cancelado') AS traspasos_recibidos_mes,
        
        (SELECT COUNT(*) FROM traspasos 
         WHERE (almacen_origen = @id_almacen OR almacen_destino = @id_almacen)
           AND estatus = 'Pendiente') AS traspasos_pendientes;

    -- RESULTADO 4: Top 5 artículos con más stock
    SELECT TOP 5
        i.id_articulo,
        a.nombre,
        i.cantidad AS stock,
        COALESCE(
            (SELECT TOP 1 unidad_medida 
             FROM donativos_detalle 
             WHERE id_articulo = i.id_articulo),
            'Unidad'
        ) AS unidad
    FROM inventario i
    INNER JOIN articulos a ON i.id_articulo = a.id_articulo
    WHERE i.id_almacen = @id_almacen
      AND i.cantidad > 0
      AND a.activo = 1
    ORDER BY i.cantidad DESC;

    -- RESULTADO 5: Top 5 artículos con menos stock (pero > 0)
    SELECT TOP 5
        i.id_articulo,
        a.nombre,
        i.cantidad AS stock,
        COALESCE(
            (SELECT TOP 1 unidad_medida 
             FROM donativos_detalle 
             WHERE id_articulo = i.id_articulo),
            'Unidad'
        ) AS unidad
    FROM inventario i
    INNER JOIN articulos a ON i.id_articulo = a.id_articulo
    WHERE i.id_almacen = @id_almacen
      AND i.cantidad > 0
      AND a.activo = 1
    ORDER BY i.cantidad ASC;

    -- RESULTADO 6: Categorías con más movimiento (este mes)
    SELECT TOP 5
        c.nombre AS categoria,
        (SELECT COUNT(DISTINCT d.id_donativo)
         FROM donativos d
         INNER JOIN donativos_detalle dd ON d.id_donativo = dd.id_donativo
         INNER JOIN articulos a ON dd.id_articulo = a.id_articulo
         WHERE a.id_categoria = c.id_categoria
           AND d.id_almacen = @id_almacen
           AND MONTH(d.fecha_recepcion) = MONTH(GETDATE())
           AND YEAR(d.fecha_recepcion) = YEAR(GETDATE())
           AND d.estatus != 'cancelado') AS entradas,
        
        (SELECT COUNT(DISTINCT s.id_salida)
         FROM salidas s
         INNER JOIN salidas_detalle sd ON s.id_salida = sd.id_salida
         INNER JOIN articulos a ON sd.id_articulo = a.id_articulo
         WHERE a.id_categoria = c.id_categoria
           AND s.id_almacen_origen = @id_almacen
           AND MONTH(s.fecha_salida) = MONTH(GETDATE())
           AND YEAR(s.fecha_salida) = YEAR(GETDATE())
           AND s.estatus != 'cancelada') AS salidas
    FROM categorias c
    -- ORDER BY (entradas + salidas) DESC;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_Inventario_PorAlmacen]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_Inventario_PorAlmacen] @id_almacen INT,
    @busqueda NVARCHAR(200) = NULL,
    @id_categoria INT = NULL,
    @solo_con_stock BIT = 0,
    @ordenar_por NVARCHAR(50) = 'nombre',
    @orden_direccion NVARCHAR(4) = 'asc' AS BEGIN
SET
    NOCOUNT ON;

-- RESULTADO 1: Información del almacén
SELECT
    id_almacen,
    nombre
FROM
    almacenes
WHERE
    id_almacen = @id_almacen;

-- RESULTADO 2: Artículos con stock calculado dinámicamente
WITH InventarioCalculado AS (
    SELECT
        a.id_articulo,
        a.nombre,
        '' AS descripcion,
        a.id_categoria,
        c.nombre AS nombre_categoria,
        -- Cálculo de stock actual
        ISNULL(i.cantidad, 0) AS stock_actual,
        -- Total de entradas
        (
            SELECT
                ISNULL(
                    SUM(
                        dd.cantidad_recibida
                    ),
                    0
                )
            FROM
                donativos_detalle dd
                INNER JOIN donativos d ON dd.id_donativo = d.id_donativo
            WHERE
                dd.id_articulo = a.id_articulo
                AND d.id_almacen = @id_almacen
                and estatus != 'cancelado'
        ) AS entradas_totales,
        -- Total de salidas
        (
            Select
                sum(cantidad_entregada)
            from
                (
                    SELECT
                        ISNULL(SUM(sd.cantidad_entregada), 0) as cantidad_entregada
                    FROM
                        salidas_detalle sd
                        INNER JOIN salidas s ON sd.id_salida = s.id_salida
                    WHERE
                        sd.id_articulo = a.id_articulo
                        AND s.id_almacen_origen = @id_almacen
                    union
                    all
                    Select
                        ISNULL(SUM(td.cantidad), 0)
                    from
                        traspasos_detalle td
                        inner join traspasos t on td.id_traspaso = t.id_traspaso
                    where
                        td.id_articulo = a.id_articulo
                        AND t.almacen_origen = @id_almacen
                        and t.estatus != 'cancelado'
                ) x
        ) AS salidas_totales,
        -- Fecha última entrada
        i.fecha_ultima_entrada,
        -- Fecha última salida
        i.fecha_ultima_salida,
        -- Movimientos recientes (últimos 30 días)
        (
            SELECT
                COUNT(*)
            FROM
                movimientos_inventario mi
            WHERE
                mi.id_articulo = a.id_articulo
                AND mi.id_almacen = @id_almacen
                AND mi.fecha_movimiento >= DATEADD(DAY, -30, GETDATE())
        ) AS movimientos_recientes,
        -- Alerta stock bajo (menos de 10 unidades)
        CASE
            WHEN ISNULL(i.cantidad, 0) < 10
            AND ISNULL(i.cantidad, 0) > 0 THEN 1
            ELSE 0
        END AS alerta_stock_bajo
    FROM
        articulos a
        JOIN categorias c ON a.id_categoria = c.id_categoria
        JOIN inventario i ON a.id_articulo = i.id_articulo
        AND i.id_almacen = @id_almacen
    WHERE
        a.activo = 1
        AND (
            @busqueda IS NULL
            OR a.nombre LIKE '%' + @busqueda + '%'
        )
        AND (
            @id_categoria IS NULL
            OR a.id_categoria = @id_categoria
        )
)
SELECT
    *
FROM
    InventarioCalculado
WHERE
    (
        @solo_con_stock = 0
        OR stock_actual > 0
    )
ORDER BY
    CASE
        WHEN @ordenar_por = 'nombre'
        AND @orden_direccion = 'asc' THEN nombre
    END ASC,
    CASE
        WHEN @ordenar_por = 'nombre'
        AND @orden_direccion = 'desc' THEN nombre
    END DESC,
    CASE
        WHEN @ordenar_por = 'stock'
        AND @orden_direccion = 'asc' THEN stock_actual
    END ASC,
    CASE
        WHEN @ordenar_por = 'stock'
        AND @orden_direccion = 'desc' THEN stock_actual
    END DESC,
    CASE
        WHEN @ordenar_por = 'categoria'
        AND @orden_direccion = 'asc' THEN nombre_categoria
    END ASC,
    CASE
        WHEN @ordenar_por = 'categoria'
        AND @orden_direccion = 'desc' THEN nombre_categoria
    END DESC;

-- RESULTADO 3: Resumen por categoría
SELECT
    c.id_categoria,
    c.nombre AS nombre_categoria,
    COUNT(DISTINCT a.id_articulo) AS total_articulos,
    SUM(
        CASE
            WHEN ISNULL(i.cantidad, 0) > 0 THEN 1
            ELSE 0
        END
    ) AS articulos_con_stock,
    ISNULL(SUM(i.cantidad), 0) AS stock_total
FROM
    categorias c
    LEFT JOIN articulos a ON c.id_categoria = a.id_categoria
    AND a.activo = 1
    LEFT JOIN inventario i ON a.id_articulo = i.id_articulo
    AND i.id_almacen = @id_almacen
GROUP BY
    c.id_categoria,
    c.nombre
HAVING
    COUNT(DISTINCT a.id_articulo) > 0
ORDER BY
    c.nombre;

END
GO
/****** Object:  StoredProcedure [dbo].[sp_Kardex_Inventario]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_Kardex_Inventario] @id_almacen INT,
@id_articulo INT,
@fecha_inicio DATETIME = NULL,
@fecha_fin DATETIME = NULL AS BEGIN
SET
    NOCOUNT ON;

IF @fecha_fin IS NULL
SET
    @fecha_fin = GETDATE();

-- ✅ SALDO INICIAL
DECLARE @saldo_inicial DECIMAL(18, 2) = 0;

IF @fecha_inicio IS NOT NULL BEGIN
SELECT
    @saldo_inicial = ISNULL(
        SUM(
            CASE
                WHEN tipo_movimiento in (
                    'Traspaso-Entrada',
                    'Traspaso-Cancelado-Entrada',
                    'Donativo'
                ) THEN cantidad
                WHEN tipo_movimiento like '%Salida' THEN - cantidad
                ELSE 0
            END
        ),
        0
    )
FROM
    movimientos_inventario
WHERE
    id_almacen = @id_almacen
    AND id_articulo = @id_articulo
    AND fecha_movimiento < @fecha_inicio;

END -- ✅ MOVIMIENTOS CON SALDO ACUMULADO
SELECT
    m.id_movimiento,
    m.fecha_movimiento,
    m.tipo_movimiento,
    m.cantidad,
    -- Referencia al documento origen
    CASE
        WHEN m.id_donativo IS NOT NULL THEN d.folio
        WHEN m.id_salida IS NOT NULL THEN s.folio
        WHEN m.id_traspaso IS NOT NULL THEN t.folio
        ELSE NULL
    END AS folio_documento,
    u.usuario AS usuario_registro,
    -- ✅ SALDO ACUMULADO
    @saldo_inicial + SUM(
        CASE
            WHEN m.tipo_movimiento in (
                'Salida-Eliminada',
                'Traspaso-Cancelado-Salida',
                'Traspaso-Entrada',
                'Donativo'
            ) THEN m.cantidad
            WHEN m.tipo_movimiento in (
                'Donativo-Eliminado',
                'Salida',
                'Traspaso-Cancelado-Entrada',
                'Traspaso-Salida'
            ) THEN - (m.cantidad * -1)
            ELSE 0
        END
    ) OVER (
        ORDER BY
            m.fecha_movimiento,
            m.id_movimiento
    ) AS saldo
FROM
    movimientos_inventario m
    LEFT JOIN donativos d ON m.id_donativo = d.id_donativo
    LEFT JOIN salidas s ON m.id_salida = s.id_salida
    LEFT JOIN traspasos t ON m.id_traspaso = t.id_traspaso
    LEFT JOIN usuarios u ON m.id_usuario = u.id_usuario
WHERE
    m.id_almacen = @id_almacen
    AND m.id_articulo = @id_articulo
    AND (
        @fecha_inicio IS NULL
        OR cast(m.fecha_movimiento as date) >= @fecha_inicio
    )
    AND cast(m.fecha_movimiento as date) <= @fecha_fin
ORDER BY
    m.fecha_movimiento,
    m.id_movimiento;

-- ✅ RESUMEN DEL KARDEX
SELECT
    @saldo_inicial AS saldo_inicial,
    SUM(
        CASE
            WHEN tipo_movimiento in (
                'Salida-Eliminada',
                'Traspaso-Cancelado-Salida',
                'Traspaso-Entrada',
                'Donativo'
            ) THEN cantidad
            ELSE 0
        END
    ) AS total_entradas,
    SUM(
        CASE
            WHEN tipo_movimiento in (
                'Donativo-Eliminado',
                'Salida',
                'Traspaso-Cancelado-Entrada',
                'Traspaso-Salida'
            ) THEN (cantidad * -1)
            ELSE 0
        END
    ) AS total_salidas,
    @saldo_inicial + SUM(
        CASE
            WHEN tipo_movimiento in (
                'Salida-Eliminada',
                'Traspaso-Cancelado-Salida',
                'Traspaso-Entrada',
                'Donativo'
            ) THEN cantidad
            WHEN tipo_movimiento in (
                'Donativo-Eliminado',
                'Salida',
                'Traspaso-Cancelado-Entrada',
                'Traspaso-Salida'
            ) THEN - (cantidad * -1)
            ELSE 0
        END
    ) AS saldo_final
FROM
    movimientos_inventario
WHERE
    id_almacen = @id_almacen
    AND id_articulo = @id_articulo
    AND (
        @fecha_inicio IS NULL
        OR cast(fecha_movimiento as date) >= @fecha_inicio
    )
    AND cast(fecha_movimiento as date) <= @fecha_fin;

END
GO
/****** Object:  StoredProcedure [dbo].[sp_RegistrarDonador]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- SP: Registrar nuevo donador
CREATE PROCEDURE [dbo].[sp_RegistrarDonador]
    @tipo NVARCHAR(20),
    @nombre NVARCHAR(300),
    @rfc NVARCHAR(13) = NULL,
    @telefono NVARCHAR(20) = NULL,
    @email NVARCHAR(100) = NULL,
    @direccion NVARCHAR(500) = NULL,
    @codigo_postal NVARCHAR(10) = NULL,
    @id_usuario INT = NULL,
    @id_donador INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Generar c�digo autom�tico
        DECLARE @codigo NVARCHAR(50);
        DECLARE @secuencia INT;
        
        SELECT @secuencia = ISNULL(MAX(CAST(SUBSTRING(codigo, 4, 6) AS INT)), 0) + 1
        FROM donadores
        WHERE codigo LIKE 'DON-%';
        
        SET @codigo = 'DON-' + RIGHT('000000' + CAST(@secuencia AS NVARCHAR(6)), 6);
        
        -- Insertar donador
        INSERT INTO donadores (
            codigo, tipo, nombre, rfc, telefono, email, direccion,codigo_postal, fecha_registro) VALUES (
            @codigo, @tipo, @nombre, @rfc, @telefono, @email, @direccion, @codigo_postal, GETDATE()
        );
        
        SET @id_donador = SCOPE_IDENTITY();
        
        -- Registrar en bit�cora
        INSERT INTO bitacora (id_usuario, modulo, submodulo, accion, descripcion)
        VALUES (@id_usuario, 'Donadores', 'Registro', 
                'Crear', 'Se registr� donador ' + @codigo + ' - ' + @nombre);
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_RegistrarDonativoConDonador]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- SP: Registrar donativo con donador existente
CREATE PROCEDURE [dbo].[sp_RegistrarDonativoConDonador]
    @id_donador INT,
    @tipo NVARCHAR(20),
    @monto_efectivo DECIMAL(15,2) = 0,
    @observaciones NVARCHAR(2000) = NULL,
    @id_usuario INT = NULL,
    @id_donativo INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificar que el donador existe y est� activo
        IF NOT EXISTS (SELECT 1 FROM donadores WHERE id_donador = @id_donador)
            THROW 50001, 'Donador no existe o no est� activo', 1;
        
        -- Generar folio autom�tico
        DECLARE @folio NVARCHAR(50);
        DECLARE @anio INT = YEAR(GETDATE());
        DECLARE @mes INT = MONTH(GETDATE());
        DECLARE @secuencia INT;
        
        SELECT @secuencia = ISNULL(MAX(CAST(SUBSTRING(folio, 11, 4) AS INT)), 0) + 1
        FROM donativos
        WHERE folio LIKE 'DON-' + CAST(@anio AS NVARCHAR(4)) + '-' + 
              RIGHT('0' + CAST(@mes AS NVARCHAR(2)), 2) + '-%';
        
        SET @folio = 'DON-' + CAST(@anio AS NVARCHAR(4)) + '-' + 
                     RIGHT('0' + CAST(@mes AS NVARCHAR(2)), 2) + '-' + 
                     RIGHT('0000' + CAST(@secuencia AS NVARCHAR(4)), 4);
        
        -- Insertar donativo
        INSERT INTO donativos (
            folio, id_donador, fecha, tipo, monto_efectivo, 
            observaciones, id_usuario_registro
        ) VALUES (
            @folio, @id_donador, GETDATE(), @tipo, @monto_efectivo,
            @observaciones, @id_usuario
        );
        
        SET @id_donativo = SCOPE_IDENTITY();
        
        -- Actualizar estad�sticas del donador
        UPDATE donadores 
        SET fecha_ultima_donacion = GETDATE(),
            total_donativos = total_donativos + 1,
            monto_total_donado = monto_total_donado + @monto_efectivo,
            fecha_actualizacion = GETDATE()
        WHERE id_donador = @id_donador;
        
        -- Registrar en bit�cora
        INSERT INTO bitacora (id_usuario, modulo, submodulo, accion, descripcion)
        VALUES (@id_usuario, 'Donativos', 'Registro', 
                'Crear', 'Se registr� donativo ' + @folio + ' para donador ID: ' + CAST(@id_donador AS NVARCHAR(10)));
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[sp_Reporte_Entradas]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_Reporte_Entradas]
    @fecha_inicio DATETIME = NULL,
    @fecha_fin DATETIME = NULL,
    @id_almacen INT = NULL,
    @id_donador INT = NULL,
    @id_articulo INT = NULL,
    @id_categoria INT = NULL,
    @tipo NVARCHAR(50) = NULL,
    @estatus NVARCHAR(50) = NULL,
    @agrupar_por NVARCHAR(50) = NULL,
    @pagina INT = 1,
    @registros_por_pagina INT = 50,
    @ordenar_por NVARCHAR(50) = 'fecha_recepcion',
    @orden NVARCHAR(4) = 'DESC'
AS
BEGIN
    SET NOCOUNT ON;

    -- Variables para paginación
    DECLARE @offset INT = (@pagina - 1) * @registros_por_pagina;

    -- ✅ TABLA TEMPORAL CON RESULTADOS BASE
    CREATE TABLE #ResultadosEntradas (
        id_donativo INT,
        folio NVARCHAR(50),
        fecha_recepcion DATETIME,
        tipo NVARCHAR(50),
        estatus NVARCHAR(50),
        id_donador INT,
        nombre_donador NVARCHAR(255),
        tipo_donador NVARCHAR(50),
        id_almacen INT,
        nombre_almacen NVARCHAR(255),
        id_articulo INT,
        nombre_articulo NVARCHAR(255),
        id_categoria INT,
        nombre_categoria NVARCHAR(100),
        cantidad_recibida DECIMAL(18,2),
        unidad_medida NVARCHAR(50),
        valor_unitario DECIMAL(18,2),
        valor_total DECIMAL(18,2),
        estado NVARCHAR(50),
        usuario_registro NVARCHAR(100),
        fecha_registro DATETIME
    );

    -- ✅ INSERTAR DATOS CON FILTROS DINÁMICOS
    INSERT INTO #ResultadosEntradas
    SELECT 
        d.id_donativo,
        d.folio,
        d.fecha_recepcion,
        d.tipo,
        d.estatus,
        don.id_donador,
        don.nombre AS nombre_donador,
        don.tipo AS tipo_donador,
        a.id_almacen,
        a.nombre AS nombre_almacen,
        art.id_articulo,
        art.nombre AS nombre_articulo,
        cat.id_categoria,
        cat.nombre AS nombre_categoria,
        dd.cantidad_recibida,
        dd.unidad_medida,
        dd.valor_unitario,
        dd.cantidad_recibida * dd.valor_unitario AS valor_total,
        dd.estado,
        u.usuario AS usuario_registro,
        d.fecha_recepcion AS fecha_registro
    FROM donativos d
    INNER JOIN donativos_detalle dd ON d.id_donativo = dd.id_donativo
    INNER JOIN donadores don ON d.id_donador = don.id_donador
    INNER JOIN almacenes a ON d.id_almacen = a.id_almacen
    INNER JOIN articulos art ON dd.id_articulo = art.id_articulo
    INNER JOIN categorias cat ON art.id_categoria = cat.id_categoria
    LEFT JOIN usuarios u ON d.id_usuario_registro = u.id_usuario
    WHERE 
        (@fecha_inicio IS NULL OR d.fecha_recepcion >= @fecha_inicio)
        AND (@fecha_fin IS NULL OR d.fecha_recepcion < DATEADD(DAY, 1, @fecha_fin))
        AND (@id_almacen IS NULL OR d.id_almacen = @id_almacen)
        AND (@id_donador IS NULL OR d.id_donador = @id_donador)
        AND (@id_articulo IS NULL OR dd.id_articulo = @id_articulo)
        AND (@id_categoria IS NULL OR art.id_categoria = @id_categoria)
        AND (@tipo IS NULL OR d.tipo = @tipo)
        AND (@estatus IS NULL OR d.estatus = @estatus)

    -- ✅ RESUMEN EJECUTIVO
    SELECT 
        COUNT(DISTINCT id_donativo) AS total_donativos,
        COUNT(DISTINCT id_articulo) AS total_articulos,
        SUM(cantidad_recibida) AS cantidad_total_recibida,
        SUM(valor_total) AS valor_total_estimado,
        COUNT(DISTINCT id_donador) AS total_donadores_activos,
        MIN(fecha_recepcion) AS periodo_inicio,
        MAX(fecha_recepcion) AS periodo_fin
    FROM #ResultadosEntradas;

    -- ✅ DETALLES CON PAGINACIÓN
    IF @agrupar_por IS NULL
    BEGIN
        -- Resultado sin agrupar
        SELECT *,
            COUNT(*) OVER() AS total_registros
        FROM #ResultadosEntradas
        ORDER BY 
            CASE WHEN @ordenar_por = 'fecha_recepcion' AND @orden = 'ASC' THEN fecha_recepcion END ASC,
            CASE WHEN @ordenar_por = 'fecha_recepcion' AND @orden = 'DESC' THEN fecha_recepcion END DESC,
            CASE WHEN @ordenar_por = 'folio' AND @orden = 'ASC' THEN folio END ASC,
            CASE WHEN @ordenar_por = 'folio' AND @orden = 'DESC' THEN folio END DESC,
            CASE WHEN @ordenar_por = 'donador' AND @orden = 'ASC' THEN nombre_donador END ASC,
            CASE WHEN @ordenar_por = 'donador' AND @orden = 'DESC' THEN nombre_donador END DESC
        OFFSET @offset ROWS
        FETCH NEXT @registros_por_pagina ROWS ONLY;
    END
    ELSE
    BEGIN
        -- ✅ RESULTADO AGRUPADO
        IF @agrupar_por = 'Donador'
        BEGIN
            SELECT 
                nombre_donador AS grupo,
                COUNT(DISTINCT id_donativo) AS total_donativos,
                COUNT(DISTINCT id_articulo) AS total_articulos,
                SUM(cantidad_recibida) AS cantidad_total,
                SUM(valor_total) AS valor_total
            FROM #ResultadosEntradas
            GROUP BY nombre_donador
            ORDER BY valor_total DESC;
        END
        ELSE IF @agrupar_por = 'Categoria'
        BEGIN
            SELECT 
                nombre_categoria AS grupo,
                COUNT(DISTINCT id_donativo) AS total_donativos,
                COUNT(DISTINCT id_articulo) AS total_articulos,
                SUM(cantidad_recibida) AS cantidad_total,
                SUM(valor_total) AS valor_total
            FROM #ResultadosEntradas
            GROUP BY nombre_categoria
            ORDER BY valor_total DESC;
        END
        ELSE IF @agrupar_por = 'Almacen'
        BEGIN
            SELECT 
                nombre_almacen AS grupo,
                COUNT(DISTINCT id_donativo) AS total_donativos,
                COUNT(DISTINCT id_articulo) AS total_articulos,
                SUM(cantidad_recibida) AS cantidad_total,
                SUM(valor_total) AS valor_total
            FROM #ResultadosEntradas
            GROUP BY nombre_almacen
            ORDER BY valor_total DESC;
        END
        ELSE IF @agrupar_por = 'Fecha'
        BEGIN
            SELECT 
                CONVERT(NVARCHAR(10), fecha_recepcion, 120) AS grupo,
                COUNT(DISTINCT id_donativo) AS total_donativos,
                COUNT(DISTINCT id_articulo) AS total_articulos,
                SUM(cantidad_recibida) AS cantidad_total,
                SUM(valor_total) AS valor_total
            FROM #ResultadosEntradas
            GROUP BY CONVERT(NVARCHAR(10), fecha_recepcion, 120)
            ORDER BY grupo DESC;
        END
    END

    -- ✅ LIMPIAR
    DROP TABLE #ResultadosEntradas;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_Reporte_Existencias]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_Reporte_Existencias] @id_almacen INT = NULL,
@id_articulo INT = NULL,
@id_categoria INT = NULL,
@cantidad_minima DECIMAL(18, 2) = NULL,
@cantidad_maxima DECIMAL(18, 2) = NULL,
@solo_sin_existencia BIT = 0,
@agrupar_por NVARCHAR(50) = NULL,
@ordenar_por NVARCHAR(50) = 'nombre_articulo',
@orden NVARCHAR(4) = 'ASC',
@fecha_corte DATETIME = NULL,
@pagina INT = 1,
@registros_por_pagina INT = 100 AS BEGIN
SET
    NOCOUNT ON;

-- ✅ FECHA DE CORTE (por defecto hoy)
IF @fecha_corte IS NULL
SET
    @fecha_corte = GETDATE();

DECLARE @offset INT = (@pagina - 1) * @registros_por_pagina;

-- ✅ TABLA TEMPORAL CON EXISTENCIAS
CREATE TABLE #ResultadosExistencias (
id_inventario INT,
id_almacen INT,
nombre_almacen NVARCHAR(255),
id_articulo INT,
nombre_articulo NVARCHAR(255),
id_categoria INT,
nombre_categoria NVARCHAR(100),
cantidad_actual DECIMAL(18, 2),
fecha_ultima_entrada DATETIME,
fecha_ultima_salida DATETIME,
dias_sin_movimiento INT,
valor_unitario_promedio DECIMAL(18, 2),
valor_total_existencia DECIMAL(18, 2),
fecha_actualizacion DATETIME
);

-- ✅ INSERTAR EXISTENCIAS ACTUALES
INSERT INTO
    #ResultadosExistencias
SELECT
    i.id_inventario,
    a.id_almacen,
    a.nombre AS nombre_almacen,
    art.id_articulo,
    art.nombre AS nombre_articulo,
    cat.id_categoria,
    cat.nombre AS nombre_categoria,
    i.cantidad AS cantidad_actual,
    i.fecha_ultima_entrada,
    i.fecha_ultima_salida,
    -- ✅ DÍAS SIN MOVIMIENTO
    DATEDIFF(
        DAY,
        CASE
            WHEN i.fecha_ultima_salida > i.fecha_ultima_entrada THEN i.fecha_ultima_salida
            WHEN i.fecha_ultima_entrada IS NOT NULL THEN i.fecha_ultima_entrada
            ELSE i.fecha_actualizacion
        END,
        @fecha_corte
    ) AS dias_sin_movimiento,
    -- ✅ VALOR UNITARIO PROMEDIO (últimos donativos)
    (
        SELECT
            TOP 1 AVG(dd.valor_unitario)
        FROM
            donativos_detalle dd
        WHERE
            dd.id_articulo = art.id_articulo
            AND dd.valor_unitario > 0
    ) AS valor_unitario_promedio,
    -- ✅ VALOR TOTAL EXISTENCIA
    i.cantidad * ISNULL(
        (
            SELECT
                TOP 1 AVG(dd.valor_unitario)
            FROM
                donativos_detalle dd
            WHERE
                dd.id_articulo = art.id_articulo
                AND dd.valor_unitario > 0
        ),
        0
    ) AS valor_total_existencia,
    i.fecha_actualizacion
FROM
    inventario i
    INNER JOIN almacenes a ON i.id_almacen = a.id_almacen
    INNER JOIN articulos art ON i.id_articulo = art.id_articulo
    INNER JOIN categorias cat ON art.id_categoria = cat.id_categoria
WHERE
    (
        @id_almacen IS NULL
        OR i.id_almacen = @id_almacen
    )
    AND (
        @id_articulo IS NULL
        OR i.id_articulo = @id_articulo
    )
    AND (
        @id_categoria IS NULL
        OR art.id_categoria = @id_categoria
    )
    AND (
        @cantidad_minima IS NULL
        OR i.cantidad >= @cantidad_minima
    )
    AND (
        @cantidad_maxima IS NULL
        OR i.cantidad <= @cantidad_maxima
    )
    AND (
        @solo_sin_existencia = 0
        OR i.cantidad = 0
    ) ;

SELECT
    COUNT(DISTINCT id_almacen) AS total_almacenes,
    COUNT(DISTINCT id_articulo) AS total_articulos_diferentes,
    SUM(
        CASE
            WHEN cantidad_actual > 0 THEN 1
            ELSE 0
        END
    ) AS total_articulos_con_existencia,
    SUM(
        CASE
            WHEN cantidad_actual = 0 THEN 1
            ELSE 0
        END
    ) AS total_articulos_sin_existencia,
    SUM(cantidad_actual) AS cantidad_total_existencia,
    SUM(valor_total_existencia) AS valor_total_inventario
FROM
    #ResultadosExistencias;
    -- ✅ DETALLES CON ORDENAMIENTO DINÁMICO
    IF @agrupar_por IS NULL BEGIN
SELECT
    *,
    COUNT(*) OVER() AS total_registros
FROM
    #ResultadosExistencias
ORDER BY
    CASE
        WHEN @ordenar_por = 'Nombre'
        AND @orden = 'ASC' THEN nombre_articulo
    END ASC,
    CASE
        WHEN @ordenar_por = 'Nombre'
        AND @orden = 'DESC' THEN nombre_articulo
    END DESC,
    CASE
        WHEN @ordenar_por = 'Cantidad'
        AND @orden = 'ASC' THEN cantidad_actual
    END ASC,
    CASE
        WHEN @ordenar_por = 'Cantidad'
        AND @orden = 'DESC' THEN cantidad_actual
    END DESC,
    CASE
        WHEN @ordenar_por = 'Categoria'
        AND @orden = 'ASC' THEN nombre_categoria
    END ASC,
    CASE
        WHEN @ordenar_por = 'Categoria'
        AND @orden = 'DESC' THEN nombre_categoria
    END DESC,
    CASE
        WHEN @ordenar_por = 'UltimaEntrada'
        AND @orden = 'ASC' THEN fecha_ultima_entrada
    END ASC,
    CASE
        WHEN @ordenar_por = 'UltimaEntrada'
        AND @orden = 'DESC' THEN fecha_ultima_entrada
    END DESC OFFSET @offset ROWS FETCH NEXT @registros_por_pagina ROWS ONLY;

END
ELSE BEGIN -- ✅ AGRUPADO
IF @agrupar_por = 'Almacen' BEGIN
SELECT
    nombre_almacen AS grupo,
    COUNT(DISTINCT id_articulo) AS total_articulos,
    SUM(cantidad_actual) AS cantidad_total,
    SUM(valor_total_existencia) AS valor_total,
    SUM(
        CASE
            WHEN cantidad_actual = 0 THEN 1
            ELSE 0
        END
    ) AS articulos_sin_existencia
FROM
    #ResultadosExistencias
GROUP BY
    nombre_almacen
ORDER BY
    valor_total DESC;

END
ELSE IF @agrupar_por = 'Categoria' BEGIN
SELECT
    nombre_categoria AS grupo,
    COUNT(DISTINCT id_articulo) AS total_articulos,
    SUM(cantidad_actual) AS cantidad_total,
    SUM(valor_total_existencia) AS valor_total,
    SUM(
        CASE
            WHEN cantidad_actual = 0 THEN 1
            ELSE 0
        END
    ) AS articulos_sin_existencia
FROM
    #ResultadosExistencias
GROUP BY
    nombre_categoria
ORDER BY
    valor_total DESC;

END
END DROP TABLE #ResultadosExistencias;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_Reporte_Salidas]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_Reporte_Salidas] @fecha_inicio DATETIME = NULL,
@fecha_fin DATETIME = NULL,
@id_almacen INT = NULL,
@id_beneficiario INT = NULL,
@id_articulo INT = NULL,
@id_categoria INT = NULL,
@tipo_salida NVARCHAR(50) = NULL,
@estatus NVARCHAR(50) = NULL,
@agrupar_por NVARCHAR(50) = NULL,
@pagina INT = 1,
@registros_por_pagina INT = 50 AS BEGIN
SET
    NOCOUNT ON;

DECLARE @offset INT = (@pagina - 1) * @registros_por_pagina;

-- ✅ TABLA TEMPORAL
CREATE TABLE #ResultadosSalidas (
id_salida INT,
folio NVARCHAR(50),
fecha_salida DATETIME,
tipo_salida NVARCHAR(50),
estatus NVARCHAR(50),
id_beneficiario INT,
nombre_beneficiario NVARCHAR(255),
tipo_beneficiario NVARCHAR(50),
id_almacen INT,
nombre_almacen NVARCHAR(255),
id_articulo INT,
nombre_articulo NVARCHAR(255),
id_categoria INT,
nombre_categoria NVARCHAR(100),
cantidad_entregada DECIMAL(18, 2),
unidad_medida NVARCHAR(50),
valor_unitario_estimado DECIMAL(18, 2),
valor_total_estimado DECIMAL(18, 2),
usuario_registro NVARCHAR(100),
fecha_registro DATETIME,
observaciones NVARCHAR(MAX)
);

-- ✅ INSERTAR DATOS
INSERT INTO
    #ResultadosSalidas
SELECT
    s.id_salida,
    s.folio,
    s.fecha_salida,
    s.tipo_salida,
    s.estatus,
    b.id_beneficiario,
    b.nombre AS nombre_beneficiario,
    b.tipo AS tipo_beneficiario,
    a.id_almacen,
    a.nombre AS nombre_almacen,
    art.id_articulo,
    art.nombre AS nombre_articulo,
     
    cat.id_categoria,
    cat.nombre AS nombre_categoria,
    sd.cantidad_entregada,
    sd.unidad_medida,
    sd.valor_unitario AS valor_unitario_estimado,
    sd.cantidad_entregada * sd.valor_unitario AS valor_total_estimado,
    u.usuario AS usuario_registro,
    s.fecha_registro AS fecha_registro,
    s.observaciones
FROM
    salidas s
    INNER JOIN salidas_detalle sd ON s.id_salida = sd.id_salida
    LEFT JOIN beneficiarios b ON s.id_beneficiario = b.id_beneficiario
    INNER JOIN almacenes a ON s.id_almacen_origen = a.id_almacen
    INNER JOIN articulos art ON sd.id_articulo = art.id_articulo
    INNER JOIN categorias cat ON art.id_categoria = cat.id_categoria
    LEFT JOIN usuarios u ON s.id_usuario_entrega = u.id_usuario
WHERE
    (
        @fecha_inicio IS NULL
        OR s.fecha_salida >= @fecha_inicio
    )
    AND (
        @fecha_fin IS NULL
        OR s.fecha_salida < DATEADD(DAY, 1, @fecha_fin)
    )
    AND (
        @id_almacen IS NULL
        OR s.id_almacen_origen = @id_almacen
    )
    AND (
        @id_beneficiario IS NULL
        OR s.id_beneficiario = @id_beneficiario
    )
    AND (
        @id_articulo IS NULL
        OR sd.id_articulo = @id_articulo
    )
    AND (
        @id_categoria IS NULL
        OR art.id_categoria = @id_categoria
    )
    AND (
        @tipo_salida IS NULL
        OR s.tipo_salida = @tipo_salida
    )
    AND (
        @estatus IS NULL
        OR s.estatus = @estatus
    )

-- ✅ RESUMEN EJECUTIVO
SELECT
    COUNT(DISTINCT id_salida) AS total_salidas,
    COUNT(DISTINCT id_articulo) AS total_articulos_entregados,
    SUM(cantidad_entregada) AS cantidad_total_entregada,
    SUM(valor_total_estimado) AS valor_total_entregado,
    COUNT(DISTINCT id_beneficiario) AS total_beneficiarios_atendidos,
    MIN(fecha_salida) AS periodo_inicio,
    MAX(fecha_salida) AS periodo_fin
FROM
    #ResultadosSalidas;
    -- ✅ DETALLES CON PAGINACIÓN
    IF @agrupar_por IS NULL BEGIN
SELECT
    *,
    COUNT(*) OVER() AS total_registros
FROM
    #ResultadosSalidas
ORDER BY
    fecha_salida DESC OFFSET @offset ROWS FETCH NEXT @registros_por_pagina ROWS ONLY;

END
ELSE BEGIN -- Agrupado por Beneficiario
IF @agrupar_por = 'Beneficiario' BEGIN
SELECT
    nombre_beneficiario AS grupo,
    COUNT(DISTINCT id_salida) AS total_salidas,
    COUNT(DISTINCT id_articulo) AS total_articulos,
    SUM(cantidad_entregada) AS cantidad_total,
    SUM(valor_total_estimado) AS valor_total
FROM
    #ResultadosSalidas
GROUP BY
    nombre_beneficiario
ORDER BY
    valor_total DESC;

END
ELSE IF @agrupar_por = 'Categoria' BEGIN
SELECT
    nombre_categoria AS grupo,
    COUNT(DISTINCT id_salida) AS total_salidas,
    COUNT(DISTINCT id_articulo) AS total_articulos,
    SUM(cantidad_entregada) AS cantidad_total,
    SUM(valor_total_estimado) AS valor_total
FROM
    #ResultadosSalidas
GROUP BY
    nombre_categoria
ORDER BY
    valor_total DESC;

END
END DROP TABLE #ResultadosSalidas;
END
GO
/****** Object:  StoredProcedure [dbo].[sp_Reporte_Traspasos]    Script Date: 18/10/2026 03:54:07 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_Reporte_Traspasos] @fecha_inicio DATETIME = NULL,
@fecha_fin DATETIME = NULL,
@id_almacen_origen INT = NULL,
@id_almacen_destino INT = NULL,
@id_articulo INT = NULL,
@id_categoria INT = NULL,
@estatus NVARCHAR(50) = NULL,
@agrupar_por NVARCHAR(50) = NULL,
@pagina INT = 1,
@registros_por_pagina INT = 50 AS BEGIN
SET
    NOCOUNT ON;

DECLARE @offset INT = (@pagina - 1) * @registros_por_pagina;

-- ✅ TABLA TEMPORAL
CREATE TABLE #ResultadosTraspasos (
id_traspaso INT,
folio NVARCHAR(50),
fecha_traspaso DATETIME,
fecha_recibido DATETIME,
estatus NVARCHAR(50),
id_almacen_origen INT,
nombre_almacen_origen NVARCHAR(255),
id_almacen_destino INT,
nombre_almacen_destino NVARCHAR(255),
id_articulo INT,
nombre_articulo NVARCHAR(255),
id_categoria INT,
nombre_categoria NVARCHAR(100),
cantidad DECIMAL(18, 2),
usuario_envio NVARCHAR(100),
usuario_recepcion NVARCHAR(100),
fecha_registro DATETIME,
motivo NVARCHAR(500)
);

-- ✅ INSERTAR DATOS
INSERT INTO
    #ResultadosTraspasos
SELECT
    t.id_traspaso,
    t.folio,
    t.fecha_traspaso,
    t.fecha_recibido,
    t.estatus,
    ao.id_almacen AS id_almacen_origen,
    ao.nombre AS nombre_almacen_origen,
    ad.id_almacen AS id_almacen_destino,
    ad.nombre AS nombre_almacen_destino,
    art.id_articulo,
    art.nombre AS nombre_articulo,
    cat.id_categoria,
    cat.nombre AS nombre_categoria,
    td.cantidad,
    ue.usuario AS usuario_envio,
    ur.usuario AS usuario_recepcion,
    t.fecha_registro AS fecha_registro,
    t.motivo
FROM
    traspasos t
    INNER JOIN traspasos_detalle td ON t.id_traspaso = td.id_traspaso
    INNER JOIN almacenes ao ON t.almacen_origen = ao.id_almacen
    INNER JOIN almacenes ad ON t.almacen_destino = ad.id_almacen
    INNER JOIN articulos art ON td.id_articulo = art.id_articulo
    INNER JOIN categorias cat ON art.id_categoria = cat.id_categoria
    LEFT JOIN usuarios ue ON t.id_usuario_envia = ue.id_usuario
    LEFT JOIN usuarios ur ON t.id_usuario_recibe = ur.id_usuario
WHERE
    (
        @fecha_inicio IS NULL
        OR t.fecha_traspaso >= @fecha_inicio
    )
    AND (
        @fecha_fin IS NULL
        OR t.fecha_traspaso < DATEADD(DAY, 1, @fecha_fin)
    )
    AND (
        @id_almacen_origen IS NULL
        OR t.almacen_origen = @id_almacen_origen
    )
    AND (
        @id_almacen_destino IS NULL
        OR t.almacen_destino = @id_almacen_destino
    )
    AND (
        @id_articulo IS NULL
        OR td.id_articulo = @id_articulo
    )
    AND (
        @id_categoria IS NULL
        OR art.id_categoria = @id_categoria
    )
    AND (
        @estatus IS NULL
        OR t.estatus = @estatus
    );

-- ✅ RESUMEN EJECUTIVO
SELECT
    COUNT(DISTINCT id_traspaso) AS total_traspasos,
    COUNT(DISTINCT id_articulo) AS total_articulos_traspasados,
    SUM(cantidad) AS cantidad_total,
    COUNT(DISTINCT id_almacen_origen) AS total_almacenes_origen,
    COUNT(DISTINCT id_almacen_destino) AS total_almacenes_destino,
    MIN(fecha_traspaso) AS periodo_inicio,
    MAX(fecha_traspaso) AS periodo_fin
FROM
    #ResultadosTraspasos;
    -- ✅ DETALLES CON PAGINACIÓN
    IF @agrupar_por IS NULL BEGIN
SELECT
    *,
    COUNT(*) OVER() AS total_registros
FROM
    #ResultadosTraspasos
ORDER BY
    fecha_traspaso DESC OFFSET @offset ROWS FETCH NEXT @registros_por_pagina ROWS ONLY;

END
ELSE BEGIN IF @agrupar_por = 'AlmacenOrigen' BEGIN
SELECT
    nombre_almacen_origen AS grupo,
    COUNT(DISTINCT id_traspaso) AS total_traspasos,
    COUNT(DISTINCT id_articulo) AS total_articulos,
    SUM(cantidad) AS cantidad_total
FROM
    #ResultadosTraspasos
GROUP BY
    nombre_almacen_origen
ORDER BY
    cantidad_total DESC;

END
ELSE IF @agrupar_por = 'AlmacenDestino' BEGIN
SELECT
    nombre_almacen_destino AS grupo,
    COUNT(DISTINCT id_traspaso) AS total_traspasos,
    COUNT(DISTINCT id_articulo) AS total_articulos,
    SUM(cantidad) AS cantidad_total
FROM
    #ResultadosTraspasos
GROUP BY
    nombre_almacen_destino
ORDER BY
    cantidad_total DESC;

END
END DROP TABLE #ResultadosTraspasos;
END
GO
USE [master]
GO
ALTER DATABASE [DonativosDB] SET  READ_WRITE 
GO
