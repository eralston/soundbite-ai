import React from "react";

// Version contains the current version number of the UI.  Version can be updated when it seems
// appropriate and the build number will be automatically appended to the version.
// Should match the Soundbite.Api project's package version TODO: Load it dynamically via API
const version = "v1.14.0";

// Do not modify this next line of code because the line is sought out by the automated build
// process and will be replaced with an automated build number version.  This value will then
// be appended to the version number specified above to help identify the build in an environment.
const buildInfo = (window as any).sbVerInfo ?? "";

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const VersionNumber: React.FC = () => {
  return (
    <div className="sb-versionNum">
      {version}
      {buildInfo}
    </div>
  );
};
