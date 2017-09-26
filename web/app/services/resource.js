define(["knockout", 'durandal/app'],
    function (ko, app) {
        "use strict";
        var title = "Resource",
            norwegian = {
                // system
                code: "No",
                norwegian: "Norsk",
                english: "Engelsk",
                warningServerUnavailable: "Får ikke kontakt med server!",
                warningLoadRecords: "Kunne ikke laste observasjonspunkter!",
                warningLoadRecordDetails: "Kunne ikke laste observasjonsdetaljer!",
                warningLoadArea: "Kunne ikke hente områdepolygon!",
                notImplemented: "Denne funksjonaliteten er foreløpig ikke ferdig implementert.",

                warningIllegalToken: "NB! Kunne ikke hente gyldig token for Norge Digitalt. Enkelte bakgrunnskart er defor ikke tilgjengelige.",
                warningLayerUnavailable: "Dette bakgrunnskartet er dessverre ikke tilgjengelig pga ugyldig eller manglende Norge Digitalt token.",
                hideWarning: "Skjul feilmeldingen",
                clickHideWarning: "Klikk skjuler feilmeldingen",
                yes: "Ja",
                no: "Nei",
                unknown: "Ukjent",
                about: "Om Naturtyper i Norge",
                aboutUrl: "http://www.artsdatabanken.no/Pages/229607",
                // bookmark.html
                loadingBookmarkStatus: "Laster utvalg fra bokmerke.."
            },
            english = {
                // system
                code: "En",
                norwegian: "Norwegian",
                english: "English",
                warningServerUnavailable: "Server unavailable!",
                warningLoadRecords: "Could not load records!",
                warningLoadRecordDetails: "Could not load record details!",
                warningLoadArea: "Could not load area polygon!",
                notImplemented: "This view is not yet implemented.",

                warningIllegalToken: "NB! Could not find valid token for Norge digitalt. Consequently, some maps are not available.",
                warningLayerUnavailable: "This map layer is unfortunately not available due to missing Norge digitalt token.",
                hideWarning: "Hide warning",
                clickHideWarning: "Click to hide warning",
                yes: "Yes",
                no: "No",
                unknown: "Unknown",
                about: "About Nature in Norway",
                aboutUrl: "http://www.artsdatabanken.no/Pages/229607",
                // bookmark.html
                loadingBookmarkStatus: "Loading filter from bookmark.."
            },
            selectedLanguage = ko.observable(norwegian),
            vm = {
                chooseEnglish: function () {
                    console.log("Change language to English");
                    selectedLanguage(english);
                    app.trigger('resource:languageChanged', selectedLanguage().Code);
                },
                chooseNorwegian: function () {
                    console.log("Change language to Norwegian");
                    selectedLanguage(norwegian);
                    app.trigger('resource:languageChanged', selectedLanguage().Code);
                },
                res: selectedLanguage
            };

        return vm;

    });
