import { Session } from "@soundbite/api";
import React from "react";
import { WidgetStore } from "../store/WidgetStore";
import { DialogAction } from "./DialogAction";
import { DialogButtonStyleType } from "./DialogButtonStyleType";
import { DialogHelper } from "./DialogHelper";
import { DialogStyleType } from "./DialogStyleType";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  orgRoute: string;
  session: Session;
  close: () => void;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const SessionDeleteDialog: React.FC<IProps> = (props: IProps) => {
  /////[ State ]//////////////////////////////////////////////////////////////////////////////////

  let [isActionRunning, setIsActionRunning] = React.useState(false);

  /////[ Methods ]//////////////////////////////////////////////////////////////////////////////////

  function getBody(): JSX.Element {
    return (
      <React.Fragment>
        <p>
          Are you sure you want to delete <strong>{props.session.name}</strong>?
        </p>
        <p>
          This will remove the Soundbite Session for{" "}
          <strong>all participants</strong> and they will no longer be able to
          contribute to or playback this session.
        </p>
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
        name: "Delete",
        isClose: true,
        style: DialogButtonStyleType.Danger,
        onSelected: async () => {
          await WidgetStore.sessions.deleteSessionAsync(
            props.orgRoute,
            props.session.route
          );
        },
      },
    ];
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return DialogHelper.RenderDialog(
    DialogStyleType.Danger,
    "Test Title",
    getActions(),
    getBody(),
    isActionRunning,
    setIsActionRunning,
    props.close
  );
};
