angular.module('virtoCommerce.catalogCsvImportModule')
.controller('virtoCommerce.catalogCsvImportModule.catalogCSVexportController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogCsvImportModule.export', 'virtoCommerce.inventoryModule.fulfillments', 'virtoCommerce.pricingModule.pricelists',
    function ($scope, bladeNavigationService, exportResourse, fulfillments, pricelists) {

        $scope.pageSize = 20;
        const blade = $scope.blade;
        blade.fulfilmentCenterId = undefined;
        blade.pricelistId = undefined;
        blade.isLoading = false;
        blade.title = 'catalogCsvImportModule.blades.catalog-CSV-export.title';
        blade.titleValues = { name: blade.catalog ? blade.catalog.name : '' };

        function initializeBlade() {
            fulfillments.search({skip:0, take: 1}, (data) => {
                    if(data.totalCount > 0){
                        $scope.fulfillmentCenters = data.results;
                        blade.fulfilmentCenterId = _.first(data.results).id;
                    }
                })
        }


        $scope.fetchfulfillmentCenters = ($select) => {
            $select.page = 0;
            $scope.fulfillmentCenters = [];
            $scope.fetchNextfulfillmentCenters($select);
        }
    
        $scope.fetchNextfulfillmentCenters = ($select) => {
            let criteria = {
                SearchPhrase: $select.search,
                take: $scope.pageSize,
                skip: $select.page * $scope.pageSize
            }

            fulfillments.search(criteria, (data) => {
                $scope.fulfillmentCenters = $scope.fulfillmentCenters.concat(data.results);
                $select.page++;
            });
        }

        $scope.$on("new-notification-event", function (event, notification) {
            if (blade.notification && notification.id === blade.notification.id) {
                angular.copy(notification, blade.notification);
            }
        });


        $scope.startExport = function () {
            exportResourse.run({
                catalogId: blade.catalog.id,
                categoryIds: _.map(blade.selectedCategories, function (x) { return x.id }),
                productIds: _.map(blade.selectedProducts, function (x) { return x.id }),
                fulfilmentCenterId: blade.fulfilmentCenterId,
                pricelistId: blade.pricelistId
            },
            function (data) { blade.notification = data; },
            function (error) { bladeNavigationService.setError('Error ' + error.status, $scope.blade); });
        };

        $scope.setForm = function (form) {
            $scope.formScope = form;
        };

        pricelists.search({ take: 1000 }, function (result) {
            $scope.pricelists = _.filter(result.results,
                function (x) {
                    return _.some(x.assignments,
                        function (y) {
                            return y.catalogId === blade.catalog.id
                        })
                });
        });

        $scope.blade.headIcon = 'fa-file-archive-o';

        initializeBlade();
    }]);
