define('services/logger', ['durandal/system'],
    function (system) {
        function logIt(message, data, source) {
            source = source ? '[' + source + '] ' : '';
            if (data) {
                system.log(source, message, data);
            } else {
                system.log(source, message);
            }
        }

        function log(message, data, source) {
            logIt(message, data, source);
        }

        function logError(message, data, source) {
            logIt(message, data, source);
        }

        var logger = {
            log: log,
            logError: logError
        };

        return logger;

    });
