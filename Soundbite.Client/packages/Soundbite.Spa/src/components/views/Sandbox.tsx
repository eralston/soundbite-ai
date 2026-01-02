import React from "react";
import { observer } from "mobx-react-lite";
import { Private } from "./Private";
import { Card, CardBody } from "reactstrap";
import { useEffect } from "react";
import { SoundbiteApiConfig } from "@soundbite/api";

export const Sandbox: React.FC = observer(() => {
  useEffect(() => {
    (window as any).amp("azuremediaplayer", undefined, () => {});
    SoundbiteApiConfig.getToken().then((token) => {
      alert(token);
    });
  });

  return (
    <Private>
      <Card>
        <CardBody>
          <h1>Azure Media Player Test</h1>
          <video
            id="azuremediaplayer"
            className="azuremediaplayer amp-default-skin amp-big-play-centered"
            autoPlay={true}
            controls
            width="640"
            height="400"
            poster=""
            data-setup='{"logo":{"enabled":false}}'
            tabIndex={0}
            webkit-playsinline={true.toString()}
            playsInline={true}
          >
            <source
              src="https://test-sbdevmedia-uswe.streaming.media.azure.net/efb2d64c-1cee-4588-96d1-96b57e4e79da/clip.ism/manifest(format=m3u8-cmaf,encryption=cbc)"
              type="application/vnd.ms-sstr+xml"
              data-setup='{"protectionInfo": [{"type": "AES", "authenticationToken": "Bearer=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzYnVyIjoiNHA5TlhsOTFuMGd2WWZWbUM5ZWZMIiwic2JvciI6IjEzTVNxN3EwM2dTQWJINFFVeW5XcCIsImF1ZCI6InNidXNlciIsIm5iZiI6MTY4ODU4MDc5MiwiZXhwIjoxNjg4NjY3MTkyLCJpYXQiOjE2ODg1ODA3OTIsImlzcyI6Imh0dHBzOi8vbG9jYWxob3N0OjMwMDAvcHVibGljL3Nzby9sb2dpbk8zNjUifQ.OHkpKM84-U9Lmtt46OZrKW9RRLH16I8PvEy5HATbRxk"}]}'
            />
          </video>
        </CardBody>
      </Card>
    </Private>
  );
});
