define(['services/logger', 'knockout', 'jquery'],
    function (logger, ko, $) {
        ko.observableArray.fn.any = function () {
            return this().length > 0;
        };

        //usage: <button data-bind="toggle: isHidden">Show / Hide</button>
        ko.bindingHandlers.toggle = {
            init: function (element, valueAccessor) {
                var value = valueAccessor();
                ko.applyBindingsToNode(element, {
                    click: function () {
                        value(!value());
                    }
                });
            }
        };

        //usage: <img data-bind="src: imgSrc" />
        ko.bindingHandlers.href = {
            update: function (element, valueAccessor) {
                ko.bindingHandlers.attr.update(element, function () {
                    return {
                        href: valueAccessor()
                    };
                });
            }
        };

        //usage: <a data-bind="href: myUrl">Click Me</a>
        ko.bindingHandlers.src = {
            update: function (element, valueAccessor) {
                ko.bindingHandlers.attr.update(element, function () {
                    return { src: valueAccessor() };
                });
            }
        };

        //usage: <form data-bind="hidden: hideForm"> (inverse of visible)
        //todo: check if this is already in knockout 3
        ko.bindingHandlers.hidden = {
            update: function (element, valueAccessor) {
                var value = ko.utils.unwrapObservable(valueAccessor());
                ko.bindingHandlers.visible.update(element, function() {
                     return !value;
                });
            }
        };

        // usage: <div data-bind="trimText: myText1"></div>
        // or <div data-bind="trimText: myText1, trimTextLength: 10"></div>
        ko.bindingHandlers.trimLengthText = {};
        ko.bindingHandlers.trimText = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel) {

                var trimmedText = ko.computed(function () {
                    var untrimmedText = ko.utils.unwrapObservable(valueAccessor());
                    var defaultMaxLength = 25;
                    var minLength = 5;
                    var maxLength = ko.utils.unwrapObservable(allBindingsAccessor().trimTextLength) || defaultMaxLength;
                    if (maxLength < minLength) maxLength = minLength;
                    var text = untrimmedText.length > maxLength ? untrimmedText.substring(0, maxLength - 1) + '...' : untrimmedText;
                    return text;
                });
                ko.applyBindingsToNode(element, {
                    text: trimmedText
                }, viewModel);

                return {
                    controlsDescendantBindings: true
                };
            }
        };

        //https://github.com/rniemeyer/knockout-jqAutocomplete/blob/master/src/knockout-jqAutocomplete.js
        //jqAuto -- main binding (should contain additional options to pass to autocomplete)
        //jqAutoSource -- the array to populate with choices (needs to be an observableArray)
        //jqAutoQuery -- function to return choices
        //jqAutoValue -- where to write the selected value
        //jqAutoSourceLabel -- the property that should be displayed in the possible choices
        //jqAutoSourceInputValue -- the property that should be displayed in the input box
        //jqAutoSourceValue -- the property to use for the value
        ko.bindingHandlers.jqAuto = {
            init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
                var options = valueAccessor() || {},
                    allBindings = allBindingsAccessor(),
                    unwrap = ko.utils.unwrapObservable,
                    modelValue = allBindings.jqAutoValue,
                    source = allBindings.jqAutoSource,
                    query = allBindings.jqAutoQuery,
                    valueProp = allBindings.jqAutoSourceValue,
                    inputValueProp = allBindings.jqAutoSourceInputValue || valueProp,
                    labelProp = allBindings.jqAutoSourceLabel || inputValueProp;

                //function that is shared by both select and change event handlers
                function writeValueToModel(valueToWrite) {
                    if (ko.isWriteableObservable(modelValue)) {
                        modelValue(valueToWrite);
                    } else {  //write to non-observable
                        if (allBindings['_ko_property_writers'] && allBindings['_ko_property_writers']['jqAutoValue']) {
                            allBindings['_ko_property_writers']['jqAutoValue'](valueToWrite);
                        }
                    }
                }

                //on a selection write the proper value to the model
                options.select = function (event, ui) {
                    writeValueToModel(ui.item ? ui.item.actualValue : null);
                };

                //on a change, make sure that it is a valid value or clear out the model value
                options.change = function(event, ui) {
                    var currentValue = $(element).val();
                    var matchingItem = ko.utils.arrayFirst(unwrap(source),
                        function(item) {
                            return unwrap(inputValueProp ? item[inputValueProp] : item) === currentValue;
                        });

                    if (!matchingItem) {
                        writeValueToModel(null);
                    }
                };

                //hold the autocomplete current response
                var currentResponse = null;

                //handle the choices being updated in a DO, to decouple value updates from source (options) updates
                var mappedSource = ko.dependentObservable({
                    read: function () {
                        mapped = ko.utils.arrayMap(unwrap(source), function (item) {
                            var result = {};
                            result.label = labelProp ? unwrap(item[labelProp]) : unwrap(item).toString();  //show in pop-up choices
                            result.value = inputValueProp ? unwrap(item[inputValueProp]) : unwrap(item).toString();  //show in input box
                            result.actualValue = valueProp ? unwrap(item[valueProp]) : item;  //store in model
                            return result;
                        });
                        return mapped;
                    },
                    write: function (newValue) {
                        source(newValue);  //update the source observableArray, so our mapped value (above) is correct
                        if (currentResponse) {
                            currentResponse(mappedSource());
                        }
                    },
                    disposeWhenNodeIsRemoved: element
                });

                if (query) {
                    options.source = function(request, response) {
                        currentResponse = response;
                        query.call(this, request.term, mappedSource);
                    };
                } else {
                    //whenever the items that make up the source are updated, make sure that autocomplete knows it
                    mappedSource.subscribe(function (newValue) {
                        $(element).autocomplete("option", "source", newValue);
                    });

                    options.source = mappedSource();
                }


                //initialize autocomplete
                $(element).autocomplete(options);
            },
            update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
                //update value based on a model change
                var allBindings = allBindingsAccessor(),
                    unwrap = ko.utils.unwrapObservable,
                    modelValue = unwrap(allBindings.jqAutoValue) || '',
                    valueProp = allBindings.jqAutoSourceValue,
                    inputValueProp = allBindings.jqAutoSourceInputValue || valueProp;

                //if we are writing a different property to the input than we are writing to the model, then locate the object
                if (valueProp && inputValueProp !== valueProp) {
                    var source = unwrap(allBindings.jqAutoSource) || [];
                    modelValue = ko.utils.arrayFirst(source, function (item) {
                        return unwrap(item[valueProp]) === modelValue;
                    }) || {};
                }

                //update the element with the value that should be shown in the input
                $(element).val(modelValue && inputValueProp !== valueProp ? unwrap(modelValue[inputValueProp]) : modelValue.toString());
            }
        };
        logger.log("knockoutExtensions loaded");
    });
