define(['services/config'], function (config) {

    describe('config', function () {
        it('is defined', function () {
            expect(config).not.toBe(undefined);
        });
    });

    describe('routeInfo', function () {       
        it('has values', function () {
            expect(config.routeInfo.length).toBeGreaterThan(0);
        });
    });
});
