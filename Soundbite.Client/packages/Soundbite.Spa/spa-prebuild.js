// This will touch the manifest.json file in the spa project
// If this goes beyond about 20 lines, it should become something more framework-y

const fs = require('fs')

// Build a new manifest file with all metadata
// This makes sure there is always a difference in the manifest.json on every published build
// Which helps with cache busting

console.log("Building manifest.json...")
const timestamp = new Date().getTime()

const manifest = {
  "short_name": "Soundbite",
  "name": "Soundbite",
  "icons": [
    {
      "src": "favicon.ico",
      "sizes": "196x196 96x96 64x64 32x32 24x24 16x16",
      "type": "image/x-icon"
    }
  ],
  "start_url": "./index.html",
  "display": "standalone",
  "theme_color": "#000000",
  "background_color": "#ffffff",
  "version": "1.0.0-" + timestamp
}

const manifestJson = JSON.stringify(manifest);

fs.writeFile('./public/manifest.json', manifestJson, err => {
  console.error("Failed to write new manifest file in spa-prebuild.js:", err)
})

console.log("Soundbite SPA Pre-Build Successful");