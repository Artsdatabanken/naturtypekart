define(['adbFuncs'], function (funcs) {

    describe('adbFuncs', function() {
        it('is defined', function() {
            expect(funcs).not.toBe(undefined);
        });
    });

    describe('adbFuncs.isArray helper for type', function () {
        it('array', function () {
            expect(funcs.isArray([1, 2, 3])).toBe(true);
        });
        it('string', function () {
            expect(funcs.isArray("hola")).not.toBe(true);
        });
        it('object', function () {
            expect(funcs.isArray({ value: [] })).not.toBe(true);
        });
    });
    
    describe('adbFuncs.reduce', function () {
        it('reduces using max', function () {
            expect(funcs.reduce(Math.max, -1, [1, 2, 3, 7, 6, 3] )).toBe(7);
        });
    });
});
