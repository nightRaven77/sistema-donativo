/**
 * ====================================================================
 * PARSEAPIRESPONSE.TEST.JS - Tests para parseApiResponse
 * ====================================================================
 * Tests unitarios para la función parseApiResponse
 * 
 * Para ejecutar estos tests:
 * 1. Instalar Jest: npm install --save-dev jest
 * 2. Ejecutar: npm test
 */

describe('parseApiResponse', () => {
    
    /**
     * Test 1: Debe retornar el array si la respuesta ya es un array
     */
    test('debe retornar array si la respuesta ya es array', () => {
        const input = [1, 2, 3];
        const result = parseApiResponse(input);
        expect(result).toEqual([1, 2, 3]);
        expect(Array.isArray(result)).toBe(true);
    });
    
    /**
     * Test 2: Debe extraer data de un objeto
     */
    test('debe extraer data de objeto con clave "data"', () => {
        const input = { 
            data: [{ id: 1 }, { id: 2 }],
            metadata: { total: 2 }
        };
        const result = parseApiResponse(input, ['data']);
        expect(result).toEqual([{ id: 1 }, { id: 2 }]);
    });
    
    /**
     * Test 3: Debe buscar en múltiples claves posibles
     */
    test('debe buscar en múltiples claves posibles', () => {
        const input = { 
            items: [{ id: 1 }, { id: 2 }]
        };
        const result = parseApiResponse(input, ['data', 'items']);
        expect(result).toEqual([{ id: 1 }, { id: 2 }]);
    });
    
    /**
     * Test 4: Debe parsear string JSON
     */
    test('debe parsear string JSON correctamente', () => {
        const input = '[{"id":1},{"id":2}]';
        const result = parseApiResponse(input);
        expect(result).toEqual([{ id: 1 }, { id: 2 }]);
    });
    
    /**
     * Test 5: Debe retornar array vacío si la respuesta es null
     */
    test('debe retornar array vacío si la respuesta es null', () => {
        const result = parseApiResponse(null);
        expect(result).toEqual([]);
    });
    
    /**
     * Test 6: Debe retornar array vacío si la respuesta es undefined
     */
    test('debe retornar array vacío si la respuesta es undefined', () => {
        const result = parseApiResponse(undefined);
        expect(result).toEqual([]);
    });
    
    /**
     * Test 7: Debe manejar errores de parseo JSON
     */
    test('debe manejar errores de parseo JSON', () => {
        const input = '{invalid json}';
        const result = parseApiResponse(input);
        expect(result).toEqual([]);
    });
    
    /**
     * Test 8: Debe retornar array vacío si no encuentra la clave
     */
    test('debe retornar array vacío si no encuentra la clave', () => {
        const input = { 
            otherKey: [1, 2, 3]
        };
        const result = parseApiResponse(input, ['data', 'items']);
        expect(result).toEqual([]);
    });
    
    /**
     * Test 9: Debe manejar objetos anidados
     */
    test('debe manejar objetos anidados correctamente', () => {
        const input = {
            response: {
                data: [{ id: 1 }]
            }
        };
        // Solo busca en el primer nivel
        const result = parseApiResponse(input, ['data']);
        expect(result).toEqual([]);
    });
    
    /**
     * Test 10: Debe validar que la data sea un array
     */
    test('debe retornar vacío si data no es array', () => {
        const input = {
            data: { id: 1 } // No es array
        };
        const result = parseApiResponse(input, ['data']);
        expect(result).toEqual([]);
    });
});

/**
 * ====================================================================
 * Tests para validarRespuesta
 * ====================================================================
 */
describe('validarRespuesta', () => {
    
    test('debe retornar false para null', () => {
        expect(validarRespuesta(null)).toBe(false);
    });
    
    test('debe retornar false para undefined', () => {
        expect(validarRespuesta(undefined)).toBe(false);
    });
    
    test('debe retornar true para objeto vacío', () => {
        expect(validarRespuesta({})).toBe(true);
    });
    
    test('debe retornar true para objeto con datos', () => {
        expect(validarRespuesta({ data: [] })).toBe(true);
    });
    
    test('debe retornar true para array', () => {
        expect(validarRespuesta([1, 2, 3])).toBe(true);
    });
    
    test('debe retornar true para string', () => {
        expect(validarRespuesta('test')).toBe(true);
    });
});

/**
 * ====================================================================
 * Tests para validarRespuestaConDatos
 * ====================================================================
 */
describe('validarRespuestaConDatos', () => {
    
    test('debe retornar false si response es null', () => {
        expect(validarRespuestaConDatos(null, 'data')).toBe(false);
    });
    
    test('debe retornar false si no contiene la clave', () => {
        expect(validarRespuestaConDatos({ other: 'value' }, 'data')).toBe(false);
    });
    
    test('debe retornar true si contiene la clave', () => {
        expect(validarRespuestaConDatos({ data: [] }, 'data')).toBe(true);
    });
    
    test('debe usar "data" como clave por defecto', () => {
        expect(validarRespuestaConDatos({ data: [1, 2, 3] })).toBe(true);
    });
    
    test('debe validar diferentes claves', () => {
        expect(validarRespuestaConDatos({ items: [] }, 'items')).toBe(true);
    });
});

// ====================================================================
// EJEMPLO DE CÓMO EJECUTAR ESTOS TESTS
// ====================================================================
/*

1. Instalar Jest:
   npm install --save-dev jest

2. Agregar script en package.json:
   {
     "scripts": {
       "test": "jest"
     }
   }

3. Ejecutar tests:
   npm test

4. Ejecutar con cobertura:
   npm test -- --coverage

5. Ejecutar en modo watch:
   npm test -- --watch

*/
