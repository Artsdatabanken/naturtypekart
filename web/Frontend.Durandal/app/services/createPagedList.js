define(['services/logger', 'jquery', "knockout", 'services/application', 'viewmodels/nav', "durandal/app"],
    function (logger, $, ko, application, nav, app) {
        return function (getListDataFunc, title) {
            var vm;
            vm = {
                title: title || "pagedList",
                activate: function (center, zoom, background, id) {
                    //logger.log(vm.title + ' View Activated', null, vm.title, true);
                    
                    app.trigger('listview:activate', '');
                    vm.updateSelection();

                },
                compositionComplete: function () {
                    vm.updateSelection();
                },
                currentPage: ko.observable(0),
                updating: ko.observable(false),
                availablePageIndexes: ko.observableArray(),
                lastPageIndex: ko.observable(0),

                itemList: ko.observableArray(),
                getListData: getListDataFunc,

                fillList: function () {
                    var promise = vm.getListData(vm.currentPage());
                    return promise.then(function (data) {
                        vm.itemList(data.itemList);
                        vm.lastPageIndex(data.totalPages - 1);
                    });
                },
                updateSelection: function () {
                    //logger.log(vm.title + ' updateSelection', null, vm.title, true);
                    if (!vm.updating()) {
                        vm.updating(true);
                        vm.itemList.removeAll();
                        vm.availablePageIndexes.removeAll();
                        vm.currentPage(0);
                        vm.fillList().then(function () {
                            var maxListItems = ($('body').width() > 481) ? application.config.maxListShowPages : application.config.maxMobileListShowPages;

                            if (vm.lastPageIndex() > 0) {
                                var lastAvailablePageIndex, i;
                                if (vm.lastPageIndex() > maxListItems) {
                                    lastAvailablePageIndex = maxListItems;
                                } else {
                                    lastAvailablePageIndex = vm.lastPageIndex();
                                }
                                for (i = 0; i <= lastAvailablePageIndex; i++) {
                                    vm.availablePageIndexes.push(i);
                                }
                            }
                            vm.updating(false);
                        });
                    }
                },
                goToPage: function (pageIndex) {
                    app.trigger('listview:pagechange', '');
                    var maxListItems = ($('body').width() > 481) ? application.config.maxListShowPages : application.config.maxMobileListShowPages;

                    if (pageIndex === vm.availablePageIndexes()[maxListItems] && pageIndex < vm.lastPageIndex()) {
                        vm.availablePageIndexes.shift();
                        vm.availablePageIndexes.push(pageIndex + 1);
                    } else if (pageIndex === vm.availablePageIndexes()[0] && pageIndex !== 0) {
                        vm.availablePageIndexes.pop();
                        vm.availablePageIndexes.unshift(pageIndex - 1);
                    }
                    vm.itemList.removeAll();
                    vm.currentPage(pageIndex);
                    vm.fillList().then(function () {
                        $("#mainPanel").scrollTop(0);
                    });
                },
                nextPage: function () {
                    vm.goToPage(vm.currentPage() + 1);
                },
                previousPage: function () {
                    vm.goToPage(vm.currentPage() - 1);
                },
                listSortOrder: ko.observable({
                    NatureLevelDescription: true,
                    SurveyScale: true,
                    SurveyedYear: true,
                    Contractor: true,
                    Surveyer: true,
                    Program: true,
                    Parameters: {
                        MainTypeDescription: true,
                        CodeDescription: true,
                        Share: true
                    },
                    natureAreaDescriptionVariables: {
                        codeAndValue: true
                    }
                }),
                sortColumn: function (col, table, order) {
                    if (order()[col]) {
                        table.sort(function (left, right) {
                            return left[col] === right[col] ? 0 : (left[col] < right[col] ? -1 : 1);
                        });
                    } else {
                        table.sort(function (right, left) {
                            return left[col] === right[col] ? 0 : (left[col] < right[col] ? -1 : 1);
                        });
                    }
                    order()[col] = !order()[col];
                },
                deepSortColumn: function (col1, col2, table, order) {
                    if (order()[col1][col2]) {
                        table.sort(function (left, right) {
                            if (left[col1][0] && right[col1][0]) {
                                return left[col1][0][col2] === right[col1][0][col2] ? 0 : (left[col1][0][col2] < right[col1][0][col2] ? -1 : 1);
                            }
                            return 1;
});
                    } else {
                        table.sort(function (right, left) {
                            if (left[col1][0] && right[col1][0]) {
                                return left[col1][0][col2] === right[col1][0][col2] ? 0 : (left[col1][0][col2] < right[col1][0][col2] ? -1 : 1);
                            }
                            return 1;
                        });
                    }
                    order()[col1][col2] = !order()[col1][col2];
                },

                sortListColumn: function (col) {
                    vm.sortColumn(col, vm.itemList, vm.listSortOrder);
                },
                deepSortListColumn: function(col1, col2) {
                    vm.deepSortColumn(col1, col2, vm.itemList, vm.listSortOrder);
                }
            };
            vm.pagePositionText = ko.computed(function () {
                return Number(this.currentPage() + 1) + " / " + Number(this.lastPageIndex() + 1);
            }, vm);

            return vm;
        };
    });
