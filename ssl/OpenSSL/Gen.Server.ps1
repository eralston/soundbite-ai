# https://stackoverflow.com/questions/59733054/how-to-create-and-trust-certificate-using-openssl

& 'C:\Program Files\git\usr\bin\openssl.exe' genrsa -out server.key 4096
& 'C:\Program Files\git\usr\bin\openssl.exe' req -new -sha256 -nodes -out server.csr -key server.key -config Gen.Server.Request.csr
& 'C:\Program Files\git\usr\bin\openssl.exe' x509 -req -in server.csr -CA RootCA.crt -CAkey RootCA.key -passin pass:pass@word1 -CAcreateserial -out server.crt -days 3650 -sha256 -extfile Gen.Server.v3.ext
& 'C:\Program Files\git\usr\bin\openssl.exe' pkcs12 -export -out server.p12 -passout pass:pass@word1 -inkey server.key -passin pass:pass@word1 -in server.crt -chain -CAfile RootCA.crt
& 'C:\Program Files\git\usr\bin\openssl.exe' pkcs12 -export -out server.pem -passout pass:pass@word1 -inkey server.key -passin pass:pass@word1 -in server.crt -chain -CAfile RootCA.crt
& 'C:\Program Files\git\usr\bin\openssl.exe' pkcs12 -export -out server.pfx -passout pass:pass@word1 -inkey server.key -passin pass:pass@word1 -in server.crt -chain -CAfile RootCA.crt -name "Soundbite Dev Cert"