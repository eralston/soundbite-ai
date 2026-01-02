import React from "react";
import { SeriesPreview, Utils } from "@soundbite/api";

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
  series: SeriesPreview;
  close: () => void;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const SeriesDeleteDialog: React.FC<IProps> = (props: IProps) => {
  /////[ State ]//////////////////////////////////////////////////////////////////////////////////

  let [isActionRunning, setIsActionRunning] = React.useState(false);

  /////[ Methods ]//////////////////////////////////////////////////////////////////////////////////

  function getBody(): JSX.Element {
    return (
      <React.Fragment>
        <p>
          Are you sure you want to delete{" "}
          <strong>
            {props.series ? props.series.name : "this appointment"}
          </strong>
          ?
        </p>
        <p>
          This will stop the series and delete any future Soundbite Sessions
          that have not already commenced. Past Soundbite Sessions will still be
          available.
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
          await WidgetStore.series.deleteSeriesAsync(
            props.orgRoute,
            props.series.route
          );
        },
      },
    ];
  }

  function getSubTitle(): string {
    return props.series === undefined
      ? ""
      : Utils.recurrenceDescription(
          props.series.template.publish,
          props.series.recurrence
        );
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return DialogHelper.RenderDialogWithSubTitle(
    DialogStyleType.Danger,
    `Delete ${props.series ? props.series.name : "this appointment"}`,
    getSubTitle(),
    getActions(),
    getBody(),
    isActionRunning,
    setIsActionRunning,
    props.close
  );
};
