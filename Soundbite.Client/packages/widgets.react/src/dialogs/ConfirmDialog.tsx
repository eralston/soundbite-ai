import React from "react";

import { DialogAction } from "./DialogAction";
import { DialogButtonStyleType } from "./DialogButtonStyleType";
import { DialogHelper } from "./DialogHelper";
import { DialogStyleType } from "./DialogStyleType";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  title: string;
  description: string;
  close: () => void;
  onConfirm: () => void;
  style?: DialogStyleType;
  buttonTitle?: string;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const ConfirmDialog: React.FC<IProps> = (props: IProps) => {
  /////[ State ]//////////////////////////////////////////////////////////////////////////////////

  let [isActionRunning, setIsActionRunning] = React.useState(false);

  const dialogStyle = props.style ?? DialogStyleType.Normal;
  const buttonStyle =
    dialogStyle === DialogStyleType.Normal
      ? DialogButtonStyleType.Primary
      : DialogButtonStyleType.Danger;

  /////[ Methods ]//////////////////////////////////////////////////////////////////////////////////

  function getBody(): JSX.Element {
    return (
      <React.Fragment>
        <p>{props.description}</p>
      </React.Fragment>
    );
  }

  function getActions(): DialogAction[] {
    return [
      {
        name: "Cancel",
        isClose: true,
        style: DialogButtonStyleType.Secondary,
      },
      {
        name: props.buttonTitle ?? "Confirm",
        isClose: true,
        style: buttonStyle,
        onSelected: async () => {
          props.onConfirm();
        },
      },
    ];
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return DialogHelper.RenderDialog(
    dialogStyle,
    props.title,
    getActions(),
    getBody(),
    isActionRunning,
    setIsActionRunning,
    props.close
  );
};
