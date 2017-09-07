define(['viewmodels/nav'],
function (nav) {
    var title = 'Header';
    var vm = {
        title: title,
        restart: function () {
            nav.restart();
        }
    };

    return vm;
});
