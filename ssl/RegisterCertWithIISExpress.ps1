# Theoretically, IIS Express *should* ask a developer if they want to automatically install an SSL certificate
# to run HTTPS during local development. In practice, SSL can break and stop working.  When that happens, This
# script can be used to explicitly associate a certificate with a specific port in IIS Express.  The certificate
# must already exist in the certificate store because it is located by thumbprint and not by file path.  You
# can generate an SSL certificate using the CertScript.ps1 file that should accompany this script.

# Location of the IisExpressAdminCmd.exe executable
$exeLocation = "C:\Program Files (x86)\IIS Express\IisExpressAdminCmd.exe";
$isValid = $true;

# Make sure the IisExpressAdminCmd.exe is available
if(!(Test-Path $exeLocation))
{
	Write-Host "IisExpressAdminCmd.exe was not found in expected location.  Please update script."
	$isValid = $false;
}

# Only run if validation passes
if($isValid)
{
	# The following is the thumbprint of the SoundbiteDevCert.cer file.  This value must be updated
	# if the certificate is ever updated. Certificate may also need to be in the certificate store (not sure)
	$certThumbprint = "1d25b6d32fa3873ee85793cbb6aafaf1208ff5eb"

	# URL where development is taking place
	$url = "https://localhost";
	
	# PORT on which  Visual Studio is configured to run SSL
	$port = 44390;
	
	# Construct and run the command to register the SSL cert with IIS
	$command = ("& '{0}' setupSslUrl -url:{1}:{2} -CertHash:{3}" -f $exeLocation,$url,$port,$certThumbprint) #"$exeLocation setupSslUrl -url:$url:$port -CertHash:$certThumbprint"
	Write-Host "Running: $command"
	iex $command	
}