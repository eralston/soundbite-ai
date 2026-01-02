import { faDownload, IconDefinition } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";
import { Button } from "reactstrap";

import { PublicError } from "@soundbite/api";
import { downloadBlob } from "@soundbite/api-axios";

import { ShowWhen } from "./ShowWhen";
import { ErrorDlg } from "../ErrorDlg";

interface IProps {
  mediaUrl?: string;
  asBlob?: boolean;
  filename?: string;
  color?: string;
  outline?: boolean;
  icon?: IconDefinition;
}

/**
 * For the given mediaUrl, provide a button that will download its associated file
 * @param props
 */
export const DownloadBtn: React.FC<IProps> = (props: IProps) => {
  const onDownload = async () => {
    if (props.mediaUrl == null) {
      return;
    }

    let downloadUrl = props.mediaUrl;

    if (props.asBlob === true) {
      // TODO: Consider a means of guaranteed one-time media download
      const blob = await downloadBlob(props.mediaUrl);
      downloadUrl = URL.createObjectURL(blob);
    }

    let filename = props.filename;
    if (filename == null) {
      // By default, Find the last component of the URL and use it as the download name
      const path = decodeURIComponent(props.mediaUrl);
      const url = new URL(path);
      url.search = "";
      const parts = url.pathname.split("/");
      if (parts.length === 0) {
        ErrorDlg.show(
          new PublicError(
            undefined,
            "Cannot Determine Name of File to Download",
            `Cannot analyze path '${url}'; cannot find filename`
          )
        );
        return;
      }
      filename = parts[parts.length - 1];
    }

    const a = document.createElement("a");
    document.body.appendChild(a);
    a.style.display = "none";
    a.href = downloadUrl;
    a.download = filename;
    a.target = "_blank";
    a.click();
    a.remove();
  };

  return (
    <ShowWhen is={props.mediaUrl != null}>
      <Button
        className="sb-record-download-btn"
        outline={props.outline ?? false}
        size="sm"
        type="button"
        color={props.color ?? "primary"}
        onClick={onDownload}
        title="Download"
      >
        <span className="btn-inner--icon">
          <FontAwesomeIcon icon={props.icon ?? faDownload} />
        </span>
      </Button>
    </ShowWhen>
  );
};
