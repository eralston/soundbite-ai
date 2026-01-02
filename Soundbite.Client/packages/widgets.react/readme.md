# Soundbite API Axios HTTP Adapter
The @soundbite/api-axios package contains an **IHttpAdapter** implementation for the Soundbite API that uses an Axios client to make HTTP requests.

For more information on **Soundbite.AI** please visit our website at http://www.soundbite.ai

## Instructions

The package exports a single function called **setupAxiosAdapter**.  Import the function and call the **setupAxiosAdapter** method from your startup code.  This configures the Soundbite API to use the Axios client.

# Dependencies
Here is a quick summary of some considerations around dependencies for this module.

## React
This uses an earlier version of React in order to maintain compatibility with SPFx as of March 2022. Do NOT upgrade until SPFx support is also upgraded.

## Font Awesome Icons
This package includes packages in order to provide SVG-based loading w/o referencing CSS (https://fontawesome.com/v5/docs/web/setup/use-package-managers).

This project must consume icons via this package and should NEVER rely on including the Font Awesome stylesheet; otherwise, the widgets will fail to load such icons.