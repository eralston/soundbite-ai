import React from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { observer } from "mobx-react-lite";
import { IconProp } from "@fortawesome/fontawesome-svg-core";

interface IProps {
  title: string;
  icon: IconProp;
  tooltip?: string;
  alwaysShowTitle?: boolean;
}

export const IconLink: React.FC<IProps> = observer((props: IProps) => {
  return (
    <h2 className="text-white" title={props.tooltip ?? props.title}>
      <FontAwesomeIcon className="mr-1" icon={props.icon} />
      <span className="d-none d-lg-inline sb-icon-link-title">
        {props.title}
      </span>
    </h2>
  );
});
