define(
    function () {
        var natureLevelNames = Object.freeze({
            0: "Udefinert",
            1: "Landskapstype",
            2: "Landskapsdel",
            3: "Naturkompleks",
            4: "Natursystem",
            5: "Naturkomponent",
            6: "Livsmedium",
            7: "Egenskapsområde"
        });

        var counties = Object.freeze({
            0: "",
            1: "Østfold",
            2: "Akershus",
            3: "Oslo",
            4: "Hedmark",
            5: "Oppland",
            6: "Buskerud",
            7: "Vestfold",
            8: "Telemark",
            9: "Aust-Agder",
            10: "Vest-Agder",
            11: "Rogaland",
            12: "Hordaland",
            13: "Bergen",
            14: "Sogn og Fjordane",
            15: "Møre og Romsdal",
            16: "Sør-Trøndelag",
            17: "Nord-Trøndelag",
            18: "Nordland",
            19: "Troms",
            20: "Finnmark/Finnmárku"
        });


        var vm = {
            natureLevelNames: natureLevelNames,
            counties: counties
        };

        return vm;
    });
