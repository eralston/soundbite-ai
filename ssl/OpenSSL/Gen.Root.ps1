# https://stackoverflow.com/questions/59733054/how-to-create-and-trust-certificate-using-openssl
& 'C:\Program Files\git\usr\bin\openssl.exe' genrsa -des3 -passout pass:pass@word1 -out RootCA.key 4096
& 'C:\Program Files\git\usr\bin\openssl.exe' req -x509 -new -nodes -sha256 -days 3650 -key RootCA.key -passin pass:pass@word1 -out RootCA.crt -config Gen.Root.Request.csr
& 'C:\Program Files\git\usr\bin\openssl.exe' pkcs12 -export -out RootCA.p12 -passout pass:pass@word1 -inkey RootCA.key -passin pass:pass@word1 -in RootCA.crt
& 'C:\Program Files\git\usr\bin\openssl.exe' pkcs12 -export -out RootCA.pem -passout pass:pass@word1 -inkey RootCA.key -passin pass:pass@word1 -in RootCA.crt
& 'C:\Program Files\git\usr\bin\openssl.exe' pkcs12 -export -out RootCA.pfx -passout pass:pass@word1 -inkey RootCA.key -passin pass:pass@word1 -in RootCA.crt