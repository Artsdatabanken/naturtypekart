define(['services/logger', "knockout", 'durandal/app', 'services/config', 'services/dataServices', 'services/application'],
    function (logger, ko, app, config, dataServices, application) {
        var title = "import",
            vm = {
                newUserUrl: config.newUserUrl,
                forgotPasswordUrl: config.forgotPasswordUrl,

                activate: function() {
                    application.fixDates();
                },

                compositionComplete: function() {
                    $('#listUserDataDeliveriesCollapse').on('show.bs.collapse', function () {
                        vm.listUserDataDeliveries();
                    });
                    $('#listNewDataDeliveriesCollapse').on('show.bs.collapse', function () {
                        vm.listNewDataDeliveries();
                    });
                    $('#listAllGridDeliveriesCollapse').on('show.bs.collapse', function () {
                        vm.listAllGridDeliveries();
                    });

                },
                isImage: function (fileExt) {
                    var newWindowFileTypes = ['jpg', 'png', 'gif', 'bmp', 'tif', 'jpeg', 'tiff'];
                    return ($.inArray(fileExt, newWindowFileTypes) > -1);
                },
                findFileExtension: function (filename) {
                    var re = /(?:\.([^.]+))?$/;
                    var fileExt = re.exec(filename)[1];
                    return fileExt;
                },

                username: ko.observable(""),
                password: ko.observable(""),
                loggedIn: ko.observable(false),
                loginError: ko.observable(false),
                loginSpinner: ko.observable(false),

                isAdministrator: ko.observable(false),
                isProvider: ko.observable(false),

                login: function () {
                    vm.loginSpinner(true);

                    $("#loginForm").validate();

                    var formData = new FormData();
                    formData.append("username", vm.username());
                    formData.append("password", vm.password());

                    dataServices.authenticate(formData).then(
                        function(result) { // Success
                            if (result === false) {
                                vm.loginError(true);
                                vm.loginSpinner(false);
                                return;
                            }
                            vm.loggedIn(true);
                            vm.loginError(false);
                            if (result.length > 0) {
                                for (var i = 0; i < result.length; i++) {
                                    switch (result[i]) {
                                    case "Administrator":
                                        vm.isAdministrator(true);
                                        vm.isProvider(true); // Administrator is omnipotent
                                        break;
                                    case "Dataleverandør":
                                        vm.isProvider(true);
                                        break;
                                    }
                                }
                            }
                            vm.loginSpinner(false);
                        },
                        function() { // Error
                            vm.loginError(true);
                            vm.loginSpinner(false);
                        }
                    );
                },

                uploadDataDeliveryReady: ko.observable(false),
                uploadDataDeliverySuccess: ko.observable(false),
                uploadDataDeliveryResult: ko.observable(""),
                uploadDataDeliverySpinner: ko.observable(false),

                uploadDataDelivery: function() {
                    vm.uploadDataDeliveryReady(false);
                    vm.uploadDataDeliverySpinner(true);
                    var formData = new FormData();
                    var selectedFile = $('#metadata').get(0).files;
                    var selectedFiles = $('#files').get(0).files;

                    formData.append("username", vm.username());
                    formData.append("password", vm.password());
                    formData.append("metadata", selectedFile[0]);
                    for (var i = 0; i < selectedFiles.length; i++) {
                        formData.append("files" + i, selectedFiles[i]);
                    }

                    dataServices.uploadDataDelivery(formData).
                        then(function(result) { // Success
                            vm.uploadDataDeliverySpinner(false);
                            vm.uploadDataDeliveryReady(true);
                            vm.uploadDataDeliverySuccess(result === "OK");
                            vm.uploadDataDeliveryResult("Feil " + result);
                        }, function() { // Error
                            vm.uploadDataDeliverySpinner(false);
                            vm.uploadDataDeliveryReady(true);
                            vm.uploadDataDeliverySuccess(false);
                        vm.uploadDataDeliveryResult("Feil: " + arguments[0].responseText);
                        });
                },

                listUserDataDeliveriesReady: ko.observable(false),
                listUserDataDeliveriesSuccess: ko.observable(false),
                listUserDataDeliveriesResult: ko.observable(""),
                listUserDataDeliveriesSpinner: ko.observable(false),
                listUserData: ko.observableArray([]),

                listUserDataDeliveries: function () {
                    vm.listUserDataDeliveriesReady(false);
                    vm.listUserDataDeliveriesSpinner(true);

                    dataServices.getListOfDataDeliveries(vm.username()).
                        then(function (result) { // Success
                            vm.listUserDataDeliveriesSpinner(false);
                            vm.listUserDataDeliveriesReady(true);
                            vm.listUserDataDeliveriesSuccess(true);
                            vm.listUserData.removeAll();

                            for (var i = 0; i < result.length; ++i) {
                                var status = "";
                                var statusDescription = "";
                                var publishable = false;
                                switch(result[i].status) {
                                    case 1:
                                        status = "Gjeldende";
                                        statusDescription = "Dette er gjeldende versjon som er publisert og vises i web-applikasjonen. Leveransedato: " + application.formatDate(result[i].DeliveryDate);
                                        break;
                                    case 2:
                                        status = "Utgått";
                                        statusDescription = "Dette er en tidligere versjon. Utgått siden " + application.formatDate(result[i].Expired);
                                        break;
                                    case 3:
                                        status = "Importert";
                                        statusDescription = "Dette er ny versjon. Den ble lastet opp " + application.formatDate(result[i].Created);
                                        publishable = true;
                                        break;
                                }
                                var shortId = result[i].id.substring(15, result[i].length);
                                vm.listUserData.push({
                                    id: shortId,
                                    url: config.dataDeliveryApiUrl + 'DownloadDocument/' + shortId,
                                    fullId: result[i].id,
                                    name: result[i].name,
                                    status: status,
                                    upload: "Lastet opp " + application.formatDate(result[i].created),
                                    statusDescription: statusDescription,
                                    description: result[i].description,
                                    projectName: result[i].metadataProjectName,
                                    projectDescription: result[i].metadataProjectDescription,
                                    publishable: publishable,
                                    spinner: ko.observable(false),
                                    ready: ko.observable(false),
                                    success: ko.observable(false),
                                    result: ko.observable("")
                                });
                            }

                        }, function () { // Error
                            vm.listUserDataDeliveriesSpinner(false);
                            vm.listUserDataDeliveriesReady(true);
                            vm.listUserDataDeliveriesSuccess(false);
                            vm.listUserDataDeliveriesResult("Feil: " + arguments[0].responseText);
                        });
                },


                listUserDataSortOrder: ko.observable({
                    id: true,
                    name: true,
                    status: true,
                    publishable: true
                }),

                sortColumn: function (col, table, order) {
                    if (order()[col]) {
                        table.sort(function(left, right) {
                            return left[col] === right[col] ? 0 : (left[col] < right[col] ? -1 : 1);
                        });
                    } else {
                        table.sort(function (right, left) {
                            return left[col] === right[col] ? 0 : (left[col] < right[col] ? -1 : 1);
                        });
                    }
                    order()[col] = !order()[col];
                },

                sortUserColumn: function(col) {
                    vm.sortColumn(col, vm.listUserData, vm.listUserDataSortOrder);
                },


                publishOwn: function(dataset) {
                    vm.validateCurrent(dataset, vm.listUserDataDeliveries);
                },

                publishAny: function (dataset) {
                    vm.validateCurrent(dataset, vm.listNewDataDeliveries);
                },
                deleteGridUpload: function (dataset) {
                    if (confirm("Er du sikker på at du vil slette " + dataset.name + " (id=" + dataset.id + ")?")) {
                        dataServices.deleteGridDelivery(dataset.id)
                            .then(function() { // Success
                                vm.listAllGridDeliveries();
                            }, function() { // Error
                                // could not delete
                            });
                    }
                },

                validateCurrent: function (dataset, callback) {
                    dataset.ready(false);
                    dataset.spinner(true);

                    var formData = new FormData();
                    formData.append("id", dataset.id);
                    formData.append("username", vm.username());
                    formData.append("password", vm.password());

                    dataServices.publiserDataleveranse(formData).
                        then(function () { // Success
                            dataset.spinner(false);
                            dataset.ready(true);
                            dataset.success(true);
                            // reload list after a short delay
                            setTimeout(function () {
                                callback();
                            }, 500);
                        }, function () { // Error
                            dataset.spinner(false);
                            dataset.ready(true);
                            dataset.success(false);
                            dataset.result("Feil: " + arguments[0].responseText);
                        });
                },

                listNewDataDeliveriesReady: ko.observable(false),
                listNewDataDeliveriesSuccess: ko.observable(false),
                listNewDataDeliveriesResult: ko.observable(""),
                listNewDataDeliveriesSpinner: ko.observable(false),
                listNewData: ko.observableArray([]),

                listNewDataDeliveries: function () {
                    vm.listNewDataDeliveriesReady(false);
                    vm.listNewDataDeliveriesSpinner(true);

                    dataServices.getListOfImportedDataDeliveries().
                        then(function (result) { // Success
                            vm.listNewDataDeliveriesSpinner(false);
                            vm.listNewDataDeliveriesReady(true);
                            vm.listNewDataDeliveriesSuccess(true);
                            vm.listNewData.removeAll();

                            for (var i = 0; i < result.length; ++i) {
                                var shortId = result[i].Id.substring(15, result[i].length);

                                vm.listNewData.push({
                                    id: shortId,
                                    url: config.dataDeliveryApiUrl + 'DownloadDocument/' + shortId,
                                    fullId: result[i].Id,
                                    name: result[i].Name,
                                    description: result[i].Description,
                                    projectName: result[i].MetadataProjectName,
                                    projectDescription: result[i].MetadataProjectDescription,
                                    statusDescription: "Lastet opp " + application.formatDate(result[i].Created),
                                    username: result[i].Username,
                                    company: result[i].OperatorCompany,
                                    contactPerson: result[i].OperatorContactPerson,
                                    email: result[i].OperatorEmail,
                                    companyUrl: result[i].OperatorHomesite,
                                    spinner: ko.observable(false),
                                    ready: ko.observable(false),
                                    success: ko.observable(false),
                                    result: ko.observable("")
                                });
                            }
                        }, function () { // Error
                            vm.listNewDataDeliveriesSpinner(false);
                            vm.listNewDataDeliveriesReady(true);
                            vm.listNewDataDeliveriesSuccess(false);
                            vm.listNewDataDeliveriesResult("Feil: " + arguments[0].responseText);
                        });
                },
                listNewDataSortOrder: ko.observable({
                    id: true,
                    name: true,
                    username: true,
                    contactPerson: true
                }),

                sortNewColumn: function (col) {
                    vm.sortColumn(col, vm.listNewData, vm.listNewDataSortOrder);
                },

                listAllGridDeliveriesReady: ko.observable(false),
                listAllGridDeliveriesSuccess: ko.observable(false),
                listAllGridDeliveriesResult: ko.observable(""),
                listAllGridDeliveriesSpinner: ko.observable(false),
                listAllGrid: ko.observableArray([]),

                getAreaTypeName: function(code) {
                    switch (code) {
                        case 1:
                            return "Kommune";
                        case 2:
                            return "Fylke";
                    }
                    return "";
                },
                getGridTypeName: function (code) {
                    switch (code) {
                        case 1:
                            return "SSB 250m";
                        case 2:
                            return "SSB 500m";
                        case 3:
                            return "SSB 1km";
                        case 4:
                            return "SSB 2km";
                        case 5:
                            return "SSB 5km";
                        case 6:
                            return "SSB 10km";
                        case 7:
                            return "SSB 25km";
                        case 8:
                            return "SSB 50km";
                        case 9:
                            return "SSB 100km";
                        case 10:
                            return "SSB 250km";
                    }
                    return "";
                },

                listAllGridDeliveries: function () {
                    vm.listAllGridDeliveriesReady(false);
                    vm.listAllGridDeliveriesSpinner(true);

                    dataServices.getAllGridDeliveries().
                        then(function (result) { // Success
                            vm.listAllGridDeliveriesSpinner(false);
                            vm.listAllGridDeliveriesReady(true);
                            vm.listAllGridDeliveriesSuccess(true);
                            vm.listAllGrid.removeAll();

                            for (var i = 0; i < result.length; ++i) {
                                var docs = ko.observableArray([]);
                                for (var j = 0; j < result[i].Documents.length; j++) {
                                    docs.push({
                                        author: result[i].Documents[j].Author,
                                        description: result[i].Documents[j].Description,
                                        filename: result[i].Documents[j].FileName,
                                        guid: result[i].Documents[j].Guid,
                                        title: result[i].Documents[j].Title,
                                        url: config.dataDeliveryApiUrl + 'DownloadDocument/' + result[i].Documents[j].Guid,
                                        isimage: vm.isImage(vm.findFileExtension(result[i].Documents[j].FileName))
                                    });
                                }
                                vm.listAllGrid.push({
                                    id: result[i].Id.substring(15, result[i].length),
                                    fullId: result[i].Id,
                                    name: result[i].Name,
                                    description: result[i].Description,
                                    documentDescription: result[i].DocumentDescription,
                                    username: result[i].Username,
                                    company: result[i].Owner.Company,
                                    contactPerson: result[i].Owner.ContactPerson,
                                    email: result[i].Owner.Email,
                                    url: result[i].Owner.Homesite,
                                    gridType: vm.getGridTypeName(result[i].GridType) + vm.getAreaTypeName(result[i].AreaType),
                                    established: application.formatDate(result[i].Established),
                                    registryVersion: result[i].Code.Registry + " " + result[i].Code.Version,
                                    value: result[i].Code.Value,
                                    sortDate: application.formatDateSorting(result[i].Established),
                                    documents: docs
                                });
                            }
                        }, function () { // Error
                            vm.listAllGridDeliveriesSpinner(false);
                            vm.listAllGridDeliveriesReady(true);
                            vm.listAllGridDeliveriesSuccess(false);
                            vm.listAllGridDeliveriesResult("Feil: " + arguments[0].responseText);
                        });
                },
                listAllGridSortOrder: ko.observable({
                    id: true,
                    name: true,
                    username: true
                }),

                sortGridColumn: function (col) {
                    vm.sortColumn(col, vm.listAllGrid, vm.listAllGridSortOrder);
                },


                uploadGridReady: ko.observable(false),
                uploadGridSuccess: ko.observable(false),
                uploadGridResult: ko.observable(""),
                uploadGridSpinner: ko.observable(false),

                uploadGrid: function () {
                    vm.uploadGridReady(false);
                    vm.uploadGridSpinner(true);
                    var formData = new FormData();
                    var selectedFile = $('#gridmetadata').get(0).files;
                    var selectedFiles = $('#gridfiles').get(0).files;

                    formData.append("username", vm.username());
                    formData.append("password", vm.password());
                    formData.append("grid", selectedFile[0]);
                    for (var i = 0; i < selectedFiles.length; i++) {
                        formData.append("files" + i, selectedFiles[i]);
                    }

                    dataServices.uploadGrid(formData).
                        then(function(result) { // Success
                            vm.uploadGridSpinner(false);
                            vm.uploadGridReady(true);
                            vm.uploadGridSuccess(result === "OK");
                            vm.uploadGridResult("Feil " + result);
                        }, function() { // Error
                            vm.uploadGridSpinner(false);
                            vm.uploadGridReady(true);
                            vm.uploadGridSuccess(false);
                            vm.uploadGridResult("Feil: " + arguments[0].responseText);
                        });
                },

                uploadOtherGridReady: ko.observable(false),
                uploadOtherGridSuccess: ko.observable(false),
                uploadOtherGridResult: ko.observable(""),
                uploadOtherGridSpinner: ko.observable(false),
                navn: ko.observable(""),
                beskrivelse: ko.observable(""),
                kode: ko.observable(""),
                koderegister: ko.observable(""),
                kodeversjon: ko.observable(""),
                etablertDato: ko.observable(),
                dokumentBeskrivelse: ko.observable(""),
                kartType: ko.observable(),
                firmanavn: ko.observable(""),
                kontaktperson: ko.observable(""),
                ownerEmail: ko.observable(""),
                telefon: ko.observable(""),
                hjemmeside: ko.observable(""),
                ssbType: ko.observable(),
                aoType: ko.observable(),

                uploadOtherGrid: function () {
                    var val = $("#otherGridForm").validate();
                    if (val.form()) {

                        vm.uploadOtherGridReady(false);
                        vm.uploadOtherGridSpinner(true);
                        var formData = new FormData();
                        var selectedFiles = $('#dokumenter').get(0).files;

                        formData.append("username", vm.username());
                        formData.append("password", vm.password());
                        formData.append("navn", vm.navn());
                        formData.append("beskrivelse", vm.beskrivelse());
                        formData.append("kode", vm.kode());
                        formData.append("koderegister", vm.koderegister());
                        formData.append("kodeversjon", vm.kodeversjon());
                        formData.append("firmanavn", vm.firmanavn());
                        formData.append("kontaktperson", vm.kontaktperson());
                        formData.append("ownerEmail", vm.ownerEmail());
                        formData.append("telefon", vm.telefon());
                        formData.append("hjemmeside", vm.hjemmeside());
                        formData.append("etablertDato", vm.etablertDato());
                        formData.append("dokumentBeskrivelse", vm.dokumentBeskrivelse());
                        formData.append("kartType", vm.kartType());
                        formData.append("ssbType", vm.ssbType());
                        formData.append("aoType", vm.aoType());
                        for (var i = 0; i < selectedFiles.length; i++) {
                            formData.append("files" + i, selectedFiles[i]);
                        }

                        dataServices.uploadGridDelivery(formData).
                        then(function (result) { // Success
                            vm.uploadOtherGridSpinner(false);
                            vm.uploadOtherGridReady(true);
                            vm.uploadOtherGridSuccess(result === "OK");
                            vm.uploadOtherGridResult("Feil " + result);
                        }, function () { // Error
                            vm.uploadOtherGridSpinner(false);
                            vm.uploadOtherGridReady(true);
                            vm.uploadOtherGridSuccess(false);
                            vm.uploadOtherGridResult("Feil: " + arguments[0].responseText);
                        });
                    }
                }

            };

            vm.userRole = ko.computed(function () {
                if (vm.isAdministrator()) {
                    return "Administrator";
                } else if (vm.isProvider()) {
                    return "Dataleverandør";
                }
                return "";
            }, vm);

        return vm;
    });
