import { IconPrefix, library } from "@fortawesome/fontawesome-svg-core";
import {
  faCalendarAlt,
  faHouseUser,
  faHourglassHalf,
  faKey,
  faPen,
  faSyncAlt,
  IconName,
} from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React, { Component } from "react";
import { InputGroupAddon } from "reactstrap";

// List of icons accepted by FaPrepend
library.add(
  faCalendarAlt,
  faHouseUser,
  faHourglassHalf,
  faKey,
  faPen,
  faSyncAlt
);

interface IProps {
  icon: IconName;
  prefix?: string;
}

/**
 * Component for prepending a Font Awesome icon onto a control in a Bootstrap InputGroup
 * */
export class FaPrepend extends Component<IProps> {
  render() {
    return (
      <InputGroupAddon addonType="prepend">
        <span className="input-group-text">
          <FontAwesomeIcon
            icon={[(this.props.prefix ?? "fas") as IconPrefix, this.props.icon]}
          />
        </span>
      </InputGroupAddon>
    );
  }
}
