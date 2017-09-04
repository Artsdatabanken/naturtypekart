svcutil /language:cs /out:"Service References\geonorge.Stedsnavn.cs" https://ws.geonorge.no/SKWS3Index/ssrIndexSearch?wsdl
svcutil /language:cs /out:"Service References\geonorge.StedsRegister.cs" https://ws.geonorge.no/SKWS2/services/SSR?wsdl
svcutil /language:cs /out:"Service References\geonorge.KommuneData.cs"  https://ws.geonorge.no/SKWS2/services/SokKomData?wsdl
svcutil /language:cs /out:"Service References\geonorge.EiendomBygg.cs" https://ws.geonorge.no/SKWS2/services/EiendomBygg?wsdl
svcutil /language:cs /out:"Service References\geonorge.Eiendom.cs" https://ws.geonorge.no/SKWS2/services/Eiendom?wsdl 
svcutil /language:cs /out:"Service References\geonorge.Adresse.cs" https://ws.geonorge.no/SKWS2/services/Adresse?wsdl
svcutil /namespace:http://rep.geointegrasjon.no/Matrikkel/Kart,GImatrikkelWS /language:cs /out:"Service References\GImatrikkelWS.MatrikkelkartPort.cs" http://www.test.matrikkel.no:7003/geointegrasjon/matrikkel/wsapi/v1/KartService?wsdl
svcutil /namespace:*,BasisFelles /language:cs /out:"Service References\GImatrikkelWS.BasisService.cs" https://www.test.matrikkel.no:443/geointegrasjon/matrikkel/wsapi/v1/BasisService?wsdl
