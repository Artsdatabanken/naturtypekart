define(
    "services/cssclass",
    [],
    function () {
        "use strict";
        var getElement = function (element) {
                return (typeof element === 'string') ?
                    document.getElementById(element) :
                    element;
            },
            checkForClass = function (element, nameOfClass) {
                element = getElement(element);
                if (!element || !element.className){
                    return false;
                }
                return (element.className === '') ?
                    false :
                    new RegExp('\\b' + nameOfClass + '\\b').test(element.className);
            },
            addClass = function (element, nameOfClass) {
                element = getElement(element);
                var containsClass = checkForClass(element, nameOfClass);
                if (!containsClass) {
                    element.className += (element.className ? ' ' : '') + nameOfClass;
                }
                return !containsClass;
            },
            removeClass = function (element, nameOfClass) {
                element = getElement(element);
                var containsClass = checkForClass(element, nameOfClass);
                if (containsClass) {
                    element.className = element.className.replace(
                        (element.className.indexOf(' ' + nameOfClass) >= 0 ? ' ' + nameOfClass : nameOfClass),
                        '');
                }
                return containsClass;
            },
            replaceClass = function (element, class1, class2) {
                element = getElement(element);
                var containsClass = checkForClass(element, class1);
                if (containsClass) {
                    removeClass(element, class1);
                    addClass(element, class2);
                }
                return containsClass;
            },
            toggleClass = function (element, nameOfClass) {
                element = getElement(element);
                if (checkForClass(element, nameOfClass)) {
                    removeClass(element, nameOfClass);
                } else {
                    addClass(element, nameOfClass);
                }
                return true;
            },
            addEvent = function (elem, type, eventHandle) {
                if (elem === null || elem === 'undefined') {
                    return;
                }
                if (elem.addEventListener) {
                    elem.addEventListener(type, eventHandle, false);
                } else if (elem.attachEvent) {
                    elem.attachEvent("on" + type, eventHandle);
                } else {
                    elem["on" + type] = eventHandle;
                }
            },
            module = {
                getElement: getElement,
                checkForClass: checkForClass,
                addClass: addClass,
                removeClass: removeClass,
                replaceClass: replaceClass,
                toggleClass: toggleClass,
                addEvent: addEvent
            };

        return module;
    }
);
