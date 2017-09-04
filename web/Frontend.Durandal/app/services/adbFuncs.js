define([], function () {
    "use strict";
    var
        createObject = function (o) {
            function F() { }
            F.prototype = o;
            return new F();
        },
        extendWithGoodParts = function () {
            var _slice = Array.prototype.slice;
            Function.prototype.method = function (name, func) {
                if (!this.prototype[name]) {
                    this.prototype[name] = func;
                    return this;
                }
            };

            if (typeof Object.create !== 'function') {
                //Object.create = function (o) {
                //    function F() { }
                //    F.prototype = o;
                //    return new F();
                //};
                Object.create = createObject;
            }
            Function.method('inherits', function (Parent) {
                this.prototype = new Parent();
                return this;
            });

            Object.method('superior', function (name) {
                var that = this,
                    method = that[name];
                return function () {
                    return method.apply(that, arguments);
                };
            });


            Function.method('curry', function () {
                var args = _slice.apply(arguments),
                    that = this;
                return function () {
                    return that.apply(null, args.concat(arguments));
                };
            });

            Function.method('bind', function (thisObj) {
                if (typeof this !== "function") {
                    // closest thing possible to the ECMAScript 5 internal IsCallable function
                    throw new TypeError("Function.prototype.bind - what is trying to be bound is not callable");
                }
                var args = _slice.call(arguments, 1),
                    fToBind = this,
                    Noop = function () { },
                    fBound = function () {
                        return fToBind.apply(this instanceof Noop && thisObj
                                ? this
                                : thisObj,
                                args.concat(_slice.call(arguments)));
                    };

                Noop.prototype = this.prototype;
                fBound.prototype = new Noop();

                return fBound;
            });

            // returns a new function that calls the original function with its first argument set to this(!).
            Function.method('methodize', function () {
                if (!this._methodized) {
                    var __method = this;
                    this._methodized = function () {
                        return __method.apply(null, [this].concat(_slice.call(arguments)));
                    };
                }
                return this._methodized;
            });

            Function.method('functionize', function () {
                if (!this._functionized) {
                    var __method = this;
                    this._functionized = function () {
                        var args = _slice.call(arguments);
                        return __method.apply(args.shift(), args);
                    };
                }
                return this._functionized;
            });

            //var momoizer = function (memo, fundamental) {
            //    function shell(n) {
            //        var result = memo[n];
            //        if (typeof result !== 'number') {
            //            result = fundamental(shell, n);
            //            memo[n] = result;
            //        }
            //        return result;
            //    }
            //    return shell;
            //};

            Number.method('integer', function () {
                return Math[this < 0 ? 'ceil' : 'floor'](this);
            });

            Number.method('clamp', function (min, max) {
                return Math.min(Math.max(this, min), max);
            });

            String.method('trim', function () {
                return this.replace(/^\s+|\s+$/g, '');
            });

        },

        isArray = function (obj) {
            var type = Object.prototype.toString.call(obj);
            return (type === '[object Array]');
            // in future, for newer browsers, use one of
            // return Array.isArray(obj);
            // return (v instanceof Array);
        },
        contains = function (arr, s) {
            var r, i;
            r = false;
            i = arr.length;
            while (i--) {
                if (arr[i] === s) {
                    r = true;
                    i = 0;
                }
            }
            return r;
        },
        foreach = function (arr, action) {
            var i,
                l = arr.length;
            for (i = 0; i < l; i++) {
                action(arr[i], i);
            }
        },
        map = function (arr, transform) {
            var result = [];
            foreach(arr, function (item) {
                result.push(transform(item));
            });
            return result;
        },
        filter = function (arr, predicate) {
            var result = [];
            foreach(arr, function (item) {
                if (predicate(item)) {
                    result.push(item);
                }
            });
            return result;
        },
        filterEmpties = function (arr) {
            return filter(arr, function(item) {
                 return !!item;
            });
        },
        reduce = function (compare, acc, arr) {
            foreach(arr, function (item) {
                acc = compare(acc, item);
            });
            return acc;
        },

        // string functions 
        firstToUpperCase = function(str) {
             return str.substr(0, 1).toUpperCase() + str.substr(1);
        },
        stringContainsAny = function (str, arr) {
            var i,
                arrlength = arr.length;
            for (i = 0; i < arrlength; i++) {
                if (str.indexOf(arr[i]) > -1) {
                    return true;
                }
            }
            return false;
        },

        // end string functions 

        getPropertyName = function (obj, caseInsensitiveName) {  // returns the first property name with correct case
            var
                name,
                oName,
                iName = caseInsensitiveName.toLowerCase();
            for (name in obj) {
                if (obj.hasOwnProperty(name)) {
                    oName = name.toLowerCase();
                    if (iName === oName) {
                        return name;
                    }
                }
            }
            return undefined;
        },
        // Warning: A problem with this (and the angular one) is that minification can make problems by renaming the function arguments!
        parameterNameList = function (func) {   // en bedre en her? : https://github.com/angular/angular.js/blob/master/src/auto/injector.js
            var FN_ARGS = /^function\s*[^\(]*\(\s*([^\)]*)\)/m,
                FN_ARG_SPLIT = /,/,
                FN_ARG = /^\s*(_?)(\S+?)\1\s*$/,
                STRIP_COMMENTS = /((\/\/.*$)|(\/\*[\s\S]*?\*\/))/mg,
                args = [],
                funcText = func.toString().replace(STRIP_COMMENTS, ''),
                argDecl = funcText.match(FN_ARGS),
                r = argDecl[1].split(FN_ARG_SPLIT),
                a,
                pushArg = function(all, underscore, name) {
                     args.push(name);
                };
            for (a in r) {
                r[a].replace(FN_ARG, pushArg);
            }
            return args;
        },
        toggleArrayItem = function (array, item, onOff) { // the optional onOff just sets the item (no toggle)
            var isObservable = !!array.__ko_proto__,
                arr = isObservable ? array() : array,
                index = arr.indexOf(item),
                hit = index >= 0,
                arrayChanged = false,
                oO = onOff === undefined ? !hit : onOff;
            if (oO && !hit) {
                arr.push(item);
                arrayChanged = true;
            } else if (!oO && hit) {
                arr.splice(index, 1);
                arrayChanged = true;
            }
            if (isObservable && arrayChanged) {
                array.valueHasMutated();
            }
            return !hit; // returns true if the item is set, false when the item is removed
        },
        toDictionary = function (arr, keyName) { // that is: convert array to object
            var dict = {};
            keyName = keyName || "id";
            foreach(arr, function (item) {
                dict[item[keyName]] = item;
            });
            return dict;
        };

    
        function deleteNullProperties(obj, recurse) {
            var key,
                value,
                ownprop;
            for (key in obj) {
                ownprop = obj.hasOwnProperty(key);
                if (ownprop) {
                    value = obj[key];
                    if (value === null) {
                        delete obj[key];
                    } else if (recurse && typeof value === 'object') {
                        deleteNullProperties(value);   //recurse
                    }
                }
            }
        }

        return {
            extendWithGoodParts: extendWithGoodParts,
            createObject: createObject,
            isArray: isArray,
            contains: contains,
            foreach: foreach,
            filter: filter,
            map: map,
            reduce: reduce,
            filterEmpties: filterEmpties, 
            getPropertyName: getPropertyName,
            getParameterNameList: parameterNameList,
            toggleArrayItem: toggleArrayItem,
            toDictionary: toDictionary,
            deleteNullProperties: deleteNullProperties,
            firstToUpperCase: firstToUpperCase,
            stringContainsAny: stringContainsAny
        };
    });
