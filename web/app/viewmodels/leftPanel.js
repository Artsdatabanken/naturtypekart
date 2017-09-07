define(['services/logger', 'viewmodels/shell'], function (logger, shell) {
    var title = 'Selected';
    function activate() {
        return true;
    }
    var vm = {
        activate: activate,
        title: title,
        toggleview: shell.toggleleftmenu
    };

    return vm;
});
