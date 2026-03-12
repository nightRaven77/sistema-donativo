/**
 * ====================================================================
 * TEST-HELPERS.JS - Utilidades para Testing
 * ====================================================================
 * Funciones helper para facilitar el testing del código JavaScript
 * 
 * @author Sistema Donativos
 * @version 1.0.0
 */

/**
 * Helpers para testing de funciones asíncronas
 */
const TestHelpers = {
    /**
     * Crea un mock de respuesta exitosa del servidor
     * @param {*} data - Datos a retornar
     * @returns {Promise} Promesa resuelta con los datos
     */
    mockSuccessResponse: function(data) {
        return Promise.resolve({ exito: true, data: data });
    },
    
    /**
     * Crea un mock de respuesta de error del servidor
     * @param {string} mensaje - Mensaje de error
     * @returns {Promise} Promesa rechazada
     */
    mockErrorResponse: function(mensaje) {
        return Promise.reject(new Error(mensaje));
    },
    
    /**
     * Espera un tiempo determinado (para tests asíncronos)
     * @param {number} ms - Milisegundos a esperar
     * @returns {Promise} Promesa que se resuelve después del tiempo
     */
    wait: function(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    },
    
    /**
     * Crea un elemento DOM temporal para testing
     * @param {string} html - HTML del elemento
     * @returns {Element} Elemento DOM
     */
    createElement: function(html) {
        const div = document.createElement('div');
        div.innerHTML = html;
        return div.firstElementChild;
    },
    
    /**
     * Limpia el DOM después de un test
     * @param {Element} element - Elemento a remover
     */
    cleanupDOM: function(element) {
        if (element && element.parentNode) {
            element.parentNode.removeChild(element);
        }
    },
    
    /**
     * Mock de jQuery para tests sin dependencias
     * @param {string} selector - Selector CSS
     * @returns {Object} Mock de jQuery
     */
    mockJQuery: function(selector) {
        const element = document.querySelector(selector);
        return {
            val: function(value) {
                if (value === undefined) {
                    return element ? element.value : '';
                }
                if (element) element.value = value;
                return this;
            },
            html: function(html) {
                if (html === undefined) {
                    return element ? element.innerHTML : '';
                }
                if (element) element.innerHTML = html;
                return this;
            },
            addClass: function(className) {
                if (element) element.classList.add(className);
                return this;
            },
            removeClass: function(className) {
                if (element) element.classList.remove(className);
                return this;
            },
            length: element ? 1 : 0
        };
    }
};

/**
 * Fixtures de datos para testing
 */
const TestFixtures = {
    /**
     * Donativo de ejemplo
     */
    donativo: {
        idDonativo: 1,
        folio: 'DON-2024-001',
        nombre: 'Juan Pérez',
        fechaRecepcion: '2024-01-15T10:00:00',
        tipo: 'especie',
        estatus: 'Recibido',
        cantidadArticulos: 5
    },
    
    /**
     * Array de donativos
     */
    donativos: [
        {
            idDonativo: 1,
            folio: 'DON-2024-001',
            nombre: 'Juan Pérez',
            fechaRecepcion: '2024-01-15T10:00:00',
            tipo: 'especie',
            estatus: 'Recibido',
            cantidadArticulos: 5
        },
        {
            idDonativo: 2,
            folio: 'DON-2024-002',
            nombre: 'María García',
            fechaRecepcion: '2024-01-16T11:00:00',
            tipo: 'efectivo',
            estatus: 'Pendiente',
            cantidadArticulos: 0
        }
    ],
    
    /**
     * Almacén de ejemplo
     */
    almacen: {
        id_almacen: 1,
        nombre: 'Almacén Central',
        ubicacion: 'Planta baja'
    },
    
    /**
     * Artículo de ejemplo
     */
    articulo: {
        id_articulo: 1,
        nombre: 'Computadora',
        categoria: {
            id_categoria: 1,
            nombre: 'Electrónicos'
        },
        unidad_medida: 'piezas'
    },
    
    /**
     * Respuesta de API exitosa
     */
    apiSuccessResponse: {
        exito: true,
        mensaje: 'Operación exitosa',
        data: []
    },
    
    /**
     * Respuesta de API con error
     */
    apiErrorResponse: {
        exito: false,
        mensaje: 'Error en la operación',
        error: 'Detalles del error'
    }
};

/**
 * Assertions personalizadas para testing
 */
const Assertions = {
    /**
     * Verifica que un valor sea un array
     * @param {*} value - Valor a verificar
     * @param {string} message - Mensaje de error
     */
    assertIsArray: function(value, message) {
        if (!Array.isArray(value)) {
            throw new Error(message || 'Expected value to be an array');
        }
    },
    
    /**
     * Verifica que un array no esté vacío
     * @param {Array} array - Array a verificar
     * @param {string} message - Mensaje de error
     */
    assertNotEmptyArray: function(array, message) {
        this.assertIsArray(array, message);
        if (array.length === 0) {
            throw new Error(message || 'Expected array not to be empty');
        }
    },
    
    /**
     * Verifica que un objeto tenga una propiedad
     * @param {Object} obj - Objeto a verificar
     * @param {string} property - Nombre de la propiedad
     * @param {string} message - Mensaje de error
     */
    assertHasProperty: function(obj, property, message) {
        if (!obj || !obj.hasOwnProperty(property)) {
            throw new Error(message || `Expected object to have property '${property}'`);
        }
    },
    
    /**
     * Verifica que una función lance un error
     * @param {Function} fn - Función a ejecutar
     * @param {string} message - Mensaje de error
     */
    assertThrows: function(fn, message) {
        let threw = false;
        try {
            fn();
        } catch (e) {
            threw = true;
        }
        if (!threw) {
            throw new Error(message || 'Expected function to throw an error');
        }
    },
    
    /**
     * Verifica que una promesa se rechace
     * @param {Promise} promise - Promesa a verificar
     * @returns {Promise} Promesa que se resuelve si se rechazó
     */
    assertRejects: async function(promise) {
        let rejected = false;
        try {
            await promise;
        } catch (e) {
            rejected = true;
        }
        if (!rejected) {
            throw new Error('Expected promise to be rejected');
        }
    }
};

// Exportar para uso en tests
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        TestHelpers,
        TestFixtures,
        Assertions
    };
}

// Hacer disponible globalmente en el navegador
if (typeof window !== 'undefined') {
    window.TestHelpers = TestHelpers;
    window.TestFixtures = TestFixtures;
    window.Assertions = Assertions;
}
