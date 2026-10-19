using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using apiDonativos.Models.Dtos.Consulta;
using apiDonativos.Services.Base;

namespace apiDonativos.Services.Consulta
{
    /// <summary>
    /// Servicio para consultas complejas de movimientos e inventario
    /// ✅ CONVENCIÓN: Hereda de InventarioServiceBase para logging y configuración
    /// ✅ ARQUITECTURA: Usa ADO.NET (SqlConnection + SqlDataAdapter) para múltiples ResultSets
    /// ✅ PERFORMANCE: Consultas optimizadas con índices y cálculos dinámicos
    /// </summary>
    public class ConsultaService : InventarioServiceBase<ConsultaService>, IConsultaService
    {
        private readonly string _connectionString;

        public ConsultaService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<ConsultaService> logger)
            : base(context, configuration, logger)
        {
            _connectionString = configuration.GetConnectionString("donativosCn")
                ?? throw new InvalidOperationException("Connection string 'donativosCn' no configurada");
        }

        #region Métodos Auxiliares

        /// <summary>
        /// Ejecuta un Stored Procedure y retorna un DataSet con múltiples resultsets
        /// </summary>
        private async Task<DataSet> EjecutarStoredProcedureAsync(string nombreSP, SqlParameter[] parametros)
        {
            var dataSet = new DataSet();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(nombreSP, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 120; // 2 minutos

                    if (parametros != null)
                    {
                        command.Parameters.AddRange(parametros);
                    }

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataSet);
                    }
                }
            }

            return dataSet;
        }

        #endregion

        #region Consulta Movimientos

        /// <summary>
        /// Consulta unificada de movimientos con filtros dinámicos y paginación
        /// ✅ EJECUTA: sp_Consulta_Movimientos
        /// </summary>
        public async Task<ConsultaMovimientosResponse> ConsultarMovimientosAsync(ConsultaMovimientosRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Los parámetros de consulta son requeridos");

            _logger.LogInformation(
                "Ejecutando consulta de movimientos. Tipo: {TipoMovimiento}, Fechas: {FechaInicio} - {FechaFin}",
                request.tipo_movimiento,
                request.fecha_inicio,
                request.fecha_fin
            );

            try
            {
                var dataSet = await EjecutarStoredProcedureAsync(
                    "sp_Consulta_Movimientos",
                    new SqlParameter[]
                    {
                        new SqlParameter("@fecha_inicio", (object)request.fecha_inicio ?? DBNull.Value),
                        new SqlParameter("@fecha_fin", (object)request.fecha_fin ?? DBNull.Value),
                        new SqlParameter("@tipo_movimiento", request.tipo_movimiento ?? "todos"),
                        new SqlParameter("@folio", (object)request.folio ?? DBNull.Value),
                        new SqlParameter("@id_donador", (object)request.id_donador ?? DBNull.Value),
                        new SqlParameter("@id_beneficiario", (object)request.id_beneficiario ?? DBNull.Value),
                        new SqlParameter("@id_articulo", (object)request.id_articulo ?? DBNull.Value),
                        new SqlParameter("@id_almacen", (object)request.id_almacen ?? DBNull.Value),
                        new SqlParameter("@estatus", (object)request.estatus ?? DBNull.Value),
                        new SqlParameter("@pagina", request.pagina),
                        new SqlParameter("@registros_por_pagina", Math.Min(request.registros_por_pagina, 100))
                    }
                );

                if (dataSet == null || dataSet.Tables.Count < 2)
                {
                    return new ConsultaMovimientosResponse
                    {
                        exito = false,
                        mensaje = "No se encontraron datos",
                        error = "El procedimiento almacenado no retornó datos"
                    };
                }

                var totalRegistros = dataSet.Tables[0].Rows.Count > 0 
                    ? Convert.ToInt32(dataSet.Tables[0].Rows[0]["total_registros"]) 
                    : 0;

                var movimientos = new List<MovimientoItem>();

                foreach (DataRow row in dataSet.Tables[1].Rows)
                {
                    movimientos.Add(new MovimientoItem
                    {
                        id = Convert.ToInt32(row["id"]),
                        tipo = row["tipo"].ToString(),
                        folio = row["folio"].ToString(),
                        fecha = Convert.ToDateTime(row["fecha"]),
                        descripcion = row["descripcion"].ToString(),
                        responsable = row["responsable"].ToString(),
                        tipo_responsable = row["tipo_responsable"].ToString(),
                        almacen_origen = row["almacen_origen"]?.ToString(),
                        almacen_destino = row["almacen_destino"] != DBNull.Value ? row["almacen_destino"].ToString() : null,
                        total_articulos = Convert.ToInt32(row["total_articulos"]),
                        cantidad_total = Convert.ToDecimal(row["cantidad_total"]),
                        estatus = row["estatus"].ToString(),
                        observaciones = row["observaciones"]?.ToString(),
                        detalles = new List<MovimientoDetalle>() // Se pueden cargar en una segunda consulta si es necesario
                    });
                }

                var totalPaginas = (int)Math.Ceiling((double)totalRegistros / request.registros_por_pagina);

                return new ConsultaMovimientosResponse
                {
                    exito = true,
                    mensaje = "Consulta realizada exitosamente",
                    data = new ConsultaMovimientosDatos
                    {
                        total_registros = totalRegistros,
                        pagina_actual = request.pagina,
                        total_paginas = totalPaginas,
                        registros_por_pagina = request.registros_por_pagina,
                        movimientos = movimientos
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar movimientos");
                return new ConsultaMovimientosResponse
                {
                    exito = false,
                    mensaje = "Error al ejecutar la consulta",
                    error = ex.Message
                };
            }
        }

        #endregion

        #region Inventario por Almacén

        /// <summary>
        /// Obtiene inventario en tiempo real por almacén
        /// ✅ EJECUTA: sp_Inventario_PorAlmacen
        /// </summary>
        public async Task<InventarioAlmacenResponse> ObtenerInventarioPorAlmacenAsync(int idAlmacen, InventarioAlmacenRequest request)
        {
            if (idAlmacen <= 0)
                throw new ArgumentException("ID de almacén inválido", nameof(idAlmacen));

            _logger.LogInformation("Obteniendo inventario para almacén {IdAlmacen}", idAlmacen);

            try
            {
                var dataSet = await EjecutarStoredProcedureAsync(
                    "sp_Inventario_PorAlmacen",
                    new SqlParameter[]
                    {
                        new SqlParameter("@id_almacen", idAlmacen),
                        new SqlParameter("@busqueda", (object)request.busqueda ?? DBNull.Value),
                        new SqlParameter("@id_categoria", (object)request.id_categoria ?? DBNull.Value),
                        new SqlParameter("@solo_con_stock", request.solo_con_stock),
                        new SqlParameter("@ordenar_por", request.ordenar_por),
                        new SqlParameter("@orden_direccion", request.orden_direccion)
                    }
                );

                if (dataSet == null || dataSet.Tables.Count < 3)
                {
                    return new InventarioAlmacenResponse
                    {
                        exito = false,
                        mensaje = "No se encontraron datos del almacén",
                        error = "El procedimiento almacenado no retornó datos completos"
                    };
                }

                // Tabla 0: Información del almacén
                var almacenRow = dataSet.Tables[0].Rows[0];
                var almacenInfo = new AlmacenInfo
                {
                    id_almacen = Convert.ToInt32(almacenRow["id_almacen"]),
                    nombre = almacenRow["nombre"].ToString(),
                    ubicacion = null, // SP no retorna este campo
                    responsable = null // SP no retorna este campo
                };

                // Tabla 1: Artículos con stock
                var articulos = new List<ArticuloInventario>();
                foreach (DataRow row in dataSet.Tables[1].Rows)
                {
                    articulos.Add(new ArticuloInventario
                    {
                        id_articulo = Convert.ToInt32(row["id_articulo"]),
                        nombre = row["nombre"].ToString(),
                        descripcion = row["descripcion"]?.ToString(),
                        categoria = new CategoriaInfo
                        {
                            id_categoria = Convert.ToInt32(row["id_categoria"]),
                            nombre = row["nombre_categoria"].ToString()
                        },
                        unidad_medida = "Unidad", // SP no retorna unidad_medida en inventario
                        stock_actual = Convert.ToDecimal(row["stock_actual"]),
                        entradas_totales = Convert.ToDecimal(row["entradas_totales"]),
                        salidas_totales = Convert.ToDecimal(row["salidas_totales"]),
                        fecha_ultima_entrada = row["fecha_ultima_entrada"] != DBNull.Value 
                            ? Convert.ToDateTime(row["fecha_ultima_entrada"]) 
                            : null,
                        fecha_ultima_salida = row["fecha_ultima_salida"] != DBNull.Value 
                            ? Convert.ToDateTime(row["fecha_ultima_salida"]) 
                            : null,
                        movimientos_recientes = Convert.ToInt32(row["movimientos_recientes"]),
                        alerta_stock_bajo = Convert.ToInt32(row["alerta_stock_bajo"]) == 1,
                        activo = true
                    });
                }

                // Tabla 2: Resumen por categoría
                var resumenCategorias = new List<ResumenCategoria>();
                foreach (DataRow row in dataSet.Tables[2].Rows)
                {
                    resumenCategorias.Add(new ResumenCategoria
                    {
                        id_categoria = Convert.ToInt32(row["id_categoria"]),
                        nombre_categoria = row["nombre_categoria"].ToString(),
                        total_articulos = Convert.ToInt32(row["total_articulos"]),
                        articulos_con_stock = Convert.ToInt32(row["articulos_con_stock"]),
                        stock_total = Convert.ToDecimal(row["stock_total"])
                    });
                }

                return new InventarioAlmacenResponse
                {
                    exito = true,
                    mensaje = "Inventario obtenido correctamente",
                    data = new InventarioAlmacenDatos
                    {
                        almacen = almacenInfo,
                        total_articulos = articulos.Count,
                        total_articulos_con_stock = articulos.Count(a => a.stock_actual > 0),
                        articulos = articulos,
                        resumen_por_categoria = resumenCategorias
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener inventario del almacén {IdAlmacen}", idAlmacen);
                return new InventarioAlmacenResponse
                {
                    exito = false,
                    mensaje = "Error al obtener el inventario",
                    error = ex.Message
                };
            }
        }

        #endregion

        #region Detalle de Movimiento

        /// <summary>
        /// Obtiene detalle completo de un movimiento específico
        /// ✅ EJECUTA: sp_Detalle_Movimiento
        /// </summary>
        public async Task<DetalleMovimientoResponse> ObtenerDetalleMovimientoAsync(string tipo, int id)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                throw new ArgumentException("Tipo de movimiento es requerido", nameof(tipo));

            if (id <= 0)
                throw new ArgumentException("ID de movimiento inválido", nameof(id));

            var tiposPermitidos = new[] { "entrada", "salida", "traspaso" };
            if (!tiposPermitidos.Contains(tipo.ToLower()))
                throw new ArgumentException($"Tipo de movimiento inválido. Valores permitidos: {string.Join(", ", tiposPermitidos)}", nameof(tipo));

            _logger.LogInformation("Obteniendo detalle de {Tipo} con ID {Id}", tipo, id);

            try
            {
                var dataSet = await EjecutarStoredProcedureAsync(
                    "sp_Detalle_Movimiento",
                    new SqlParameter[]
                    {
                        new SqlParameter("@tipo_movimiento", tipo.ToLower()),
                        new SqlParameter("@id_movimiento", id)
                    }
                );

                if (dataSet == null || dataSet.Tables.Count < 3)
                {
                    return new DetalleMovimientoResponse
                    {
                        exito = false,
                        mensaje = "Movimiento no encontrado",
                        error = "No se encontraron datos para el movimiento especificado"
                    };
                }

                // Tabla 0: Datos principales
                if (dataSet.Tables[0].Rows.Count == 0)
                {
                    return new DetalleMovimientoResponse
                    {
                        exito = false,
                        mensaje = $"No existe {tipo} con ID {id}",
                        error = "Movimiento no encontrado"
                    };
                }

                var mainRow = dataSet.Tables[0].Rows[0];

                var detalle = new DetalleMovimientoDatos
                {
                    tipo = tipo,
                    id = id,
                    folio = mainRow["folio"].ToString(),
                    fecha = Convert.ToDateTime(mainRow["fecha"]),
                    almacen_origen = new AlmacenDetalleInfo
                    {
                        id_almacen = Convert.ToInt32(mainRow["almacen_origen_id"]),
                        nombre = mainRow["almacen_origen_nombre"].ToString(),
                        ubicacion = null // SP no retorna almacen_origen_ubicacion
                    },
                    almacen_destino = mainRow["almacen_destino_id"] != DBNull.Value
                        ? new AlmacenDetalleInfo
                        {
                            id_almacen = Convert.ToInt32(mainRow["almacen_destino_id"]),
                            nombre = mainRow["almacen_destino_nombre"].ToString(),
                            ubicacion = null // SP no retorna almacen_destino_ubicacion
                        }
                        : null,
                    responsable = new ResponsableInfo
                    {
                        id = Convert.ToInt32(mainRow["responsable_id"]),
                        tipo = mainRow["responsable_tipo"].ToString(),
                        codigo = mainRow["responsable_codigo"]?.ToString(),
                        nombre = mainRow["responsable_nombre"].ToString(),
                        telefono = mainRow["responsable_telefono"]?.ToString(),
                        email = mainRow["responsable_email"]?.ToString(),
                        direccion = mainRow["responsable_direccion"]?.ToString()
                    },
                    estatus = mainRow["estatus"].ToString(),
                    observaciones = mainRow["observaciones"]?.ToString(),
                    tipo_movimiento = mainRow["tipo_movimiento"]?.ToString(),
                    usuario_registro = new UsuarioInfo
                    {
                        id = mainRow["usuario_id"] != DBNull.Value ? Convert.ToInt32(mainRow["usuario_id"]) : 0,
                        nombre = mainRow["usuario_nombre"]?.ToString()
                    },
                    fecha_registro = Convert.ToDateTime(mainRow["fecha_registro"])
                };

                // Tabla 1: Detalles de artículos
                detalle.detalles = new List<ArticuloDetalle>();
                decimal valorTotal = 0;

                // Verificar qué columnas existen (traspasos no retorna unidad_medida ni descripcion)
                var tablaDetalle = dataSet.Tables[1];
                bool tieneUnidadMedida = tablaDetalle.Columns.Contains("unidad_medida");
                bool tieneDescripcion = tablaDetalle.Columns.Contains("descripcion");

                foreach (DataRow row in tablaDetalle.Rows)
                {
                    var valorArticulo = row["valor_total"] != DBNull.Value ? Convert.ToDecimal(row["valor_total"]) : 0;
                    valorTotal += valorArticulo;

                    detalle.detalles.Add(new ArticuloDetalle
                    {
                        id_detalle = Convert.ToInt32(row["id_detalle"]),
                        id_articulo = Convert.ToInt32(row["id_articulo"]),
                        nombre_articulo = row["nombre_articulo"].ToString(),
                        categoria = row["categoria"]?.ToString(),
                        cantidad = Convert.ToDecimal(row["cantidad"]),
                        unidad_medida = tieneUnidadMedida ? row["unidad_medida"].ToString() : "Unidad",
                        valor_unitario = row["valor_unitario"] != DBNull.Value ? Convert.ToDecimal(row["valor_unitario"]) : null,
                        valor_total = row["valor_total"] != DBNull.Value ? Convert.ToDecimal(row["valor_total"]) : null,
                        descripcion = tieneDescripcion && row["descripcion"] != DBNull.Value ? row["descripcion"].ToString() : null
                    });
                }

                detalle.totales = new TotalesMovimiento
                {
                    total_articulos = detalle.detalles.Count,
                    cantidad_total = detalle.detalles.Sum(d => d.cantidad),
                    valor_total = valorTotal
                };

                // Tabla 2: Archivos adjuntos
                detalle.archivos_adjuntos = new List<ArchivoAdjunto>();
                foreach (DataRow row in dataSet.Tables[2].Rows)
                {
                    detalle.archivos_adjuntos.Add(new ArchivoAdjunto
                    {
                        id_archivo = Convert.ToInt32(row["id_archivo"]),
                        nombre_original = row["nombre_original"].ToString(),
                        url = row["url"].ToString(),
                        tamano = 0 // Si no tienes el campo en BD
                    });
                }

                return new DetalleMovimientoResponse
                {
                    exito = true,
                    mensaje = "Detalle obtenido correctamente",
                    data = detalle
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle de {Tipo} con ID {Id}", tipo, id);
                return new DetalleMovimientoResponse
                {
                    exito = false,
                    mensaje = "Error al obtener el detalle",
                    error = ex.Message
                };
            }
        }

        #endregion

        #region Estadísticas de Almacén

        /// <summary>
        /// Obtiene estadísticas y KPIs de un almacén
        /// ✅ EJECUTA: sp_Estadisticas_Almacen
        /// </summary>
        public async Task<EstadisticasAlmacenResponse> ObtenerEstadisticasAlmacenAsync(int idAlmacen)
        {
            if (idAlmacen <= 0)
                throw new ArgumentException("ID de almacén inválido", nameof(idAlmacen));

            _logger.LogInformation("Obteniendo estadísticas para almacén {IdAlmacen}", idAlmacen);

            try
            {
                var dataSet = await EjecutarStoredProcedureAsync(
                    "sp_Estadisticas_Almacen",
                    new SqlParameter[]
                    {
                        new SqlParameter("@id_almacen", idAlmacen)
                    }
                );

                if (dataSet == null || dataSet.Tables.Count < 5)
                {
                    return new EstadisticasAlmacenResponse
                    {
                        exito = false,
                        mensaje = "No se pudieron obtener las estadísticas",
                        error = "El procedimiento almacenado no retornó datos completos"
                    };
                }

                var datos = new EstadisticasAlmacenDatos();

                // Tabla 0: Información del almacén
                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    var almRow = dataSet.Tables[0].Rows[0];
                    datos.almacen = new AlmacenInfo
                    {
                        id_almacen = Convert.ToInt32(almRow["id_almacen"]),
                        nombre = almRow["nombre"].ToString()
                    };
                }

                // Tabla 1: Resumen general
                if (dataSet.Tables[1].Rows.Count > 0)
                {
                    var resRow = dataSet.Tables[1].Rows[0];
                    datos.resumen = new ResumenEstadisticas
                    {
                        total_articulos_catalogo = Convert.ToInt32(resRow["total_articulos_catalogo"]),
                        articulos_con_stock = Convert.ToInt32(resRow["articulos_con_stock"]),
                        articulos_sin_stock = Convert.ToInt32(resRow["articulos_sin_stock"]),
                        stock_total_general = Convert.ToDecimal(resRow["stock_total_general"]),
                        alertas_stock_bajo = Convert.ToInt32(resRow["alertas_stock_bajo"])
                    };
                }

                // Tabla 2: Movimientos (hoy y mes)
                if (dataSet.Tables[2].Rows.Count > 0)
                {
                    var movRow = dataSet.Tables[2].Rows[0];
                    datos.movimientos_hoy = new MovimientosEstadisticas
                    {
                        entradas = Convert.ToInt32(movRow["entradas_hoy"]),
                        salidas = Convert.ToInt32(movRow["salidas_hoy"]),
                        traspasos = Convert.ToInt32(movRow["traspasos_hoy"])
                    };

                    datos.movimientos_este_mes = new MovimientosEstadisticas
                    {
                        entradas = Convert.ToInt32(movRow["entradas_mes"]),
                        salidas = Convert.ToInt32(movRow["salidas_mes"]),
                        traspasos = Convert.ToInt32(movRow["traspasos_mes"]),
                        traspasos_recibidos = Convert.ToInt32(movRow["traspasos_recibidos_mes"]),
                        traspasos_pendientes = Convert.ToInt32(movRow["traspasos_pendientes"])
                    };
                }

                // Tabla 3: Top 5 artículos con más stock
                datos.top5_articulos_mas_stock = new List<ArticuloRanking>();
                foreach (DataRow row in dataSet.Tables[3].Rows)
                {
                    datos.top5_articulos_mas_stock.Add(new ArticuloRanking
                    {
                        id_articulo = Convert.ToInt32(row["id_articulo"]),
                        nombre = row["nombre"].ToString(),
                        stock = Convert.ToDecimal(row["stock"]),
                        unidad = row["unidad"].ToString()
                    });
                }

                // Tabla 4: Top 5 artículos con menos stock
                datos.top5_articulos_menos_stock = new List<ArticuloRanking>();
                foreach (DataRow row in dataSet.Tables[4].Rows)
                {
                    datos.top5_articulos_menos_stock.Add(new ArticuloRanking
                    {
                        id_articulo = Convert.ToInt32(row["id_articulo"]),
                        nombre = row["nombre"].ToString(),
                        stock = Convert.ToDecimal(row["stock"]),
                        unidad = row["unidad"].ToString()
                    });
                }

                // Tabla 5: Categorías con más movimiento (si existe)
                datos.categorias_mas_movimiento = new List<CategoriaMovimiento>();
                if (dataSet.Tables.Count > 5)
                {
                    foreach (DataRow row in dataSet.Tables[5].Rows)
                    {
                        datos.categorias_mas_movimiento.Add(new CategoriaMovimiento
                        {
                            categoria = row["categoria"].ToString(),
                            entradas = Convert.ToInt32(row["entradas"]),
                            salidas = Convert.ToInt32(row["salidas"])
                        });
                    }
                }

                return new EstadisticasAlmacenResponse
                {
                    exito = true,
                    mensaje = "Estadísticas obtenidas correctamente",
                    data = datos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del almacén {IdAlmacen}", idAlmacen);
                return new EstadisticasAlmacenResponse
                {
                    exito = false,
                    mensaje = "Error al obtener las estadísticas",
                    error = ex.Message
                };
            }
        }

        #endregion
    }
}
