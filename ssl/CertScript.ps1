#===================================================================================================
# Script for generating an SSL certificate that can be used for hosting the local development
# environment.  This does *most* of the heavy lifting but requires some manual intervention.
#
# After running the script you should end up with a .PEM file containing two distinct sections
# in the same directory where the script was run.  One section contains the Certificate and one 
# section contains the Private Key.  You will need to split the files out into two files -- a .cer 
# and a .key file containing just their respective sections.
#
# You will then need to import the .PFX file into the trusted root certificate store either for
# your user account or for the computer. 
#
# There may also be remnants of the certifate that was generated in the store.  The "Friendly" 
# name of the certificate should be "Soundbite Dev Cert" and the DNS name will be "localhost".  
# Make sure not to remove it from the trusted root certs.
#===================================================================================================

# https://medium.com/the-new-control-plane/generating-self-signed-certificates-on-windows-7812a600c2d8

$pfxFileName = "SoundbiteDevCert.pfx"
$pemFileName = "SoundbiteDevCert.pem"

# Generate the Certificate
$cert_path = "cert:\localmachine\my"
$date_now = Get-Date
$date_ext = $date_now.AddYears(3)
$cert = New-SelfSignedCertificate -certstorelocation $cert_path -dnsname "localhost" -notafter $date_ext
$cert.FriendlyName = "Soundbite Dev Cert"

# Export the PFX
$pwd = ConvertTo-SecureString -String "pass@word1" -Force -AsPlainText
$cert_store_path = $path = "cert:\localMachine\my\" + $cert.thumbprint
Export-PfxCertificate -cert $cert_store_path -FilePath $pfxFileName -Password $pwd

# Remove the Certificate from the Store
Get-ChildItem $cert_store_path | Remove-Item

#############################################################################
#NOTE: Generating with a PEM is broken becuase of some issue with Convert-PfxToPem command
#############################################################################
# Generate PEM File
#Install-Module -Name PSPKI
#Import-Module PSPKI
#Convert-PfxToPem -InputFile $pfxFileName -Outputfile $pemFileName
#############################################################################


# Generate PEM with OPENSSL
openssl pkcs12 -in SoundbiteDevCert.pfx -nocerts -out SoundbiteDevCert.enc.key
openssl pkcs12 -in SoundbiteDevCert.pfx -clcerts -nokeys -out SoundbiteDevCert.cer
openssl rsa -in SoundbiteDevCert.enc.key -out SoundbiteDevCert.key
Remove-Item SoundbiteDevCert.enc.key

Write-Host
Write-Host "You must manually create the .PEM file using the contents of the"
Write-Host "SoundbiteDevCert.key and SoundbiteDevCert.cer files"
Write-Host
Write-Host "You will also need to install PFX as trusted root Cert in Local Machine"
Write-Host