import React from 'react';
import { render } from 'react-dom';
import ol from 'openlayers';
import { createStore, compose } from 'redux';
import { Provider, connect } from 'react-redux';

require("openlayers/css/ol.css");
require("./popup.css");

var placeLayer = new ol.layer.Vector({
    source: new ol.source.Vector({
        format: new ol.format.GeoJSON(),
        //url: "http://www.geoforall.org/locations/OSGEoLabs.json" raises
        //Cross-Origin Request Blocked: The Same Origin Policy disallows reading the remote resource at http://www.geoforall.org/locations/OSGEoLabs.json. (Reason: CORS header 'Access-Control-Allow-Origin' missing).
        url: "OSGEoLabs.json"
    })
});


var map = new ol.Map({
    target: 'map',
    layers: [
        new ol.layer.Tile({
            source: new ol.source.OSM()
        }),
        placeLayer
    ],
    view: new ol.View({
        center: [949282, 9702552],
        zoom: 3
    })
});

var popupElement = document.getElementById('popup');
var popup = new ol.Overlay({
    element: popupElement,
    autoPan: true,
    autoPanAnimation: {
        duration: 250
    }
});
map.addOverlay(popup);

function placeName(place) {
    return place.name.replace(/<(?:.|\n)*?>/g, '');
}

// OL callbacks
function updateVisiblePlaces() {
    var extent = map.getView().calculateExtent(map.getSize());
    var places = placeLayer.getSource().getFeaturesInExtent(extent).map(function(feature) {
        return feature.getProperties();
    });
    // Update state in Redux store
    store.dispatch(visiblePlacesAction(places))
}
placeLayer.on('change', updateVisiblePlaces);
map.on('moveend', updateVisiblePlaces);

function updateSelection(name) {
    var extent = map.getView().calculateExtent(map.getSize());
    console.log(extent);
    var feats = placeLayer.getSource().getFeaturesInExtent(extent);
    var selected = feats.filter(function(feature) {
        return name == placeName(feature.getProperties());
    });
    if (selected.length > 0) {
        var feature = selected[0];
        popupElement.innerHTML = feature.getProperties().name;
        var coordinate = feature.getGeometry().getFirstCoordinate();
        popup.setPosition(coordinate);
    }
}

var PlaceList = React.createClass({
    render: function() {
        var onSelectClick = this.props.onSelectClick;
        var selected = this.props.selected;
        var createItem = function(place) {
            var name = placeName(place);
            var selClass = (name == selected) ? 'selected' : '';
            return <li key={ name } className={ selClass } onClick={ onSelectClick.bind(this, name) }>
                     { name }
                   </li>;
        };
        return (
            <ul>
              { this.props.places.map(createItem) }
            </ul>
            );
    },
    getDefaultProps: function() {
        return {
            places: []
        };
    }
}
);

/* REDUX ACTIONS */
function visiblePlacesAction(places) {
    return {
        type: 'visible',
        places: places
    };
}

function selectAction(placeName) {
    return {
        type: 'select',
        placeName: placeName
    };
}

/* =============== */

// Reducer:
function placeSelector(state, action) {
    switch (action.type) {
        case 'visible':
            return {
                places: action.places,
                selected: state.selected
            };
        case 'select':
            return {
                places: state.places,
                selected: action.placeName
            };
        default:
            return state;
    }
}

// Store:
var store = createStore(placeSelector, {
    places: [],
    selected: null
}, compose(
    // Add other middleware on this line...
    window.devToolsExtension ? window.devToolsExtension() : f => f //add support for Redux dev tools
));

// Map Redux state to component props
function mapStateToProps(state) {
    return {
        places: state.places,
        selected: state.selected
    };
}

// Map Redux actions to component props
function mapDispatchToProps(dispatch) {
    return {
        onSelectClick: function(name) {
            dispatch(selectAction(name));
            // Update map
            updateSelection(name)
        }
    };
}

var App = connect(mapStateToProps, mapDispatchToProps)(PlaceList);

var Map = React.createClass({
    render: function() {
        return (
            <Provider store={ store }>
              <div>
                <App/>
              </div>
            </Provider>
        )
    }
});

export default Map;
//export default PlaceList;
//export default Map;
